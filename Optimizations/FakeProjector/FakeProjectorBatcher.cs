using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws every live <see cref="FakeProjector"/> in the scene, culled per camera and submitted as instanced
/// draw calls. This is what makes the replacement worth doing: cost tracks the projections actually on screen
/// rather than the number authored into the level.
/// </summary>
/// <remarks>
/// <para>
/// Hooks <see cref="Camera.onPreCull"/> rather than living on a GameObject, which keeps it out of the scene
/// hierarchy and gives it the camera currently being rendered — so culling is exact per camera instead of
/// approximated against the main one, and scene-view cameras get the same treatment during level design.
/// This is built-in-render-pipeline only; an SRP port would hook RenderPipelineManager.beginCameraRendering.
/// </para>
/// <para>
/// Projections are bucketed by material and texture, since that pair is what a single instanced draw can
/// cover. Bucketing happens only when the registration set changes, not per frame.
/// </para>
/// </remarks>
public static class FakeProjectorBatcher
{
    /// <summary>Unity's hard limit on instances per <see cref="Graphics.DrawMeshInstanced"/> call.</summary>
    const int MAX_INSTANCES_PER_DRAW = 1023;

    readonly struct BatchKey : System.IEquatable<BatchKey>
    {
        public readonly Material Material;
        public readonly Texture Texture;

        public BatchKey( Material material, Texture texture )
        {
            Material = material;
            Texture = texture;
        }

        public bool Equals( BatchKey other ) => Material == other.Material && Texture == other.Texture;
        public override bool Equals( object obj ) => obj is BatchKey other && Equals( other );

        public override int GetHashCode()
        {
            var hash = Material != null ? Material.GetInstanceID() : 0;
            return ( hash * 397 ) ^ ( Texture != null ? Texture.GetInstanceID() : 0 );
        }
    }

    sealed class Batch
    {
        public readonly List<FakeProjector> Projectors = new();
        public Matrix4x4[] Matrices = System.Array.Empty<Matrix4x4>();
        public MaterialPropertyBlock Properties;

        /// <summary>Grows the submission buffer to hold every registered projector in this batch.</summary>
        public void EnsureCapacity()
        {
            var needed = Mathf.Min( Projectors.Count, MAX_INSTANCES_PER_DRAW );
            if( Matrices.Length >= needed ) return;
            Matrices = new Matrix4x4[Mathf.Max( needed, MAX_INSTANCES_PER_DRAW / 8 )];
        }
    }

    static readonly List<FakeProjector> _registered = new();
    static readonly Dictionary<BatchKey, Batch> _batches = new();
    static readonly Plane[] _frustum = new Plane[6];

    static Mesh _quad;
    static bool _hooked;
    static bool _batchesDirty;

    /// <summary>Materials already reported as non-instanced, so the warning fires once instead of every frame.</summary>
    static readonly HashSet<int> _warnedMaterials = new();

    public static int RegisteredCount => _registered.Count;

    /// <summary>
    /// Clears everything before a new play session. Static state outlives play mode when domain reloading is
    /// turned off, which would otherwise leave the camera callback hooked and the previous scene's projectors
    /// registered as destroyed objects.
    /// </summary>
    [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
    static void ResetStatics()
    {
        Unhook();
        _registered.Clear();
        _batches.Clear();
        _warnedMaterials.Clear();
        _batchesDirty = false;
        // _quad is deliberately kept: it is immutable and flagged DontSave, so reusing it across play sessions
        // avoids both a per-session leak and destroying an asset from inside a subsystem callback.
    }

    public static void Register( FakeProjector projector )
    {
        if( projector == null || _registered.Contains( projector ) ) return;
        _registered.Add( projector );
        _batchesDirty = true;
        Hook();
    }

    public static void Unregister( FakeProjector projector )
    {
        if( !_registered.Remove( projector ) ) return;
        _batchesDirty = true;
        if( _registered.Count == 0 ) Unhook();
    }

    /// <summary>Forces a re-bucket, for when a projector's material or texture changes after registration.</summary>
    public static void SetDirty() => _batchesDirty = true;

    static void Hook()
    {
        if( _hooked ) return;
        Camera.onPreCull += OnPreCull;
        _hooked = true;
    }

    static void Unhook()
    {
        if( !_hooked ) return;
        Camera.onPreCull -= OnPreCull;
        _hooked = false;
    }

    static void OnPreCull( Camera camera )
    {
        if( camera == null || camera.cameraType == CameraType.Preview ) return;

        if( _batchesDirty ) RebuildBatches();
        if( _batches.Count == 0 ) return;

        GeometryUtility.CalculateFrustumPlanes( camera, _frustum );
        var mesh = GetQuad();

        foreach( var pair in _batches )
        {
            var batch = pair.Value;
            var material = pair.Key.Material;
            if( material == null || batch.Projectors.Count == 0 ) continue;
            if( !SupportsInstancing( material ) ) continue;

            DrawBatch( mesh, material, batch, camera );
        }
    }

    /// <summary>Culls the batch against the current frustum and submits the survivors, chunked to Unity's limit.</summary>
    static void DrawBatch( Mesh mesh, Material material, Batch batch, Camera camera )
    {
        batch.EnsureCapacity();

        var count = 0;
        var projectors = batch.Projectors;
        for( int i = 0; i < projectors.Count; i++ )
        {
            var projector = projectors[i];
            if( projector == null ) { _batchesDirty = true; continue; }

            var baked = projector.Baked;
            if( !baked.Built ) continue;
            if( !IsVisible( baked ) ) continue;

            batch.Matrices[count++] = baked.ToMatrix();

            if( count < batch.Matrices.Length ) continue;
            Submit( mesh, material, batch, count, camera );
            count = 0;
        }

        if( count > 0 ) Submit( mesh, material, batch, count, camera );
    }

    static void Submit( Mesh mesh, Material material, Batch batch, int count, Camera camera )
    {
        Graphics.DrawMeshInstanced(
            mesh, 0, material, batch.Matrices, count, batch.Properties,
            UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, camera );
    }

    /// <summary>Bounding-sphere test — see <see cref="FakeProjectorQuad.BoundingRadius"/> for why a sphere suffices.</summary>
    static bool IsVisible( FakeProjectorQuad baked )
    {
        var center = baked.Position;
        var radius = baked.BoundingRadius;
        for( int i = 0; i < _frustum.Length; i++ )
            if( _frustum[i].GetDistanceToPoint( center ) < -radius ) return false;
        return true;
    }

    static void RebuildBatches()
    {
        _batchesDirty = false;

        foreach( var pair in _batches ) pair.Value.Projectors.Clear();

        for( int i = _registered.Count - 1; i >= 0; i-- )
        {
            var projector = _registered[i];
            if( projector == null ) { _registered.RemoveAt( i ); continue; }

            var material = projector.Material;
            if( material == null ) continue;

            var key = new BatchKey( material, projector.Texture );
            if( !_batches.TryGetValue( key, out var batch ) )
            {
                batch = new Batch();
                if( projector.Texture != null )
                {
                    batch.Properties = new MaterialPropertyBlock();
                    batch.Properties.SetTexture( "_MainTex", projector.Texture );
                }
                _batches.Add( key, batch );
            }

            batch.Projectors.Add( projector );
        }

        if( _registered.Count == 0 ) Unhook();
    }

    static bool SupportsInstancing( Material material )
    {
        if( material.enableInstancing ) return true;
        if( _warnedMaterials.Add( material.GetInstanceID() ) )
            Debug.LogWarning( $"Material '{material.name}' needs GPU instancing enabled to batch fake projectors.", material );
        return false;
    }

    /// <summary>
    /// Unit quad in the XY plane with its normal on +Z, so a bake orients it with
    /// <c>Quaternion.LookRotation( surfaceNormal, ... )</c>. Built here rather than taken from Unity's
    /// primitives so the normal direction and UV layout are guaranteed rather than assumed.
    /// </summary>
    static Mesh GetQuad()
    {
        if( _quad != null ) return _quad;

        _quad = new Mesh { name = "FakeProjectorQuad", hideFlags = HideFlags.HideAndDontSave };
        _quad.SetVertices( new List<Vector3>
        {
            new( -0.5f, -0.5f, 0f ), new( 0.5f, -0.5f, 0f ), new( 0.5f, 0.5f, 0f ), new( -0.5f, 0.5f, 0f ),
        } );
        _quad.SetNormals( new List<Vector3> { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward } );
        _quad.SetUVs( 0, new List<Vector2> { new( 0f, 0f ), new( 1f, 0f ), new( 1f, 1f ), new( 0f, 1f ) } );
        _quad.SetTriangles( new[] { 0, 2, 1, 0, 3, 2 }, 0 );
        _quad.RecalculateBounds();
        return _quad;
    }
}
