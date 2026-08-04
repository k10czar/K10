using UnityEngine;

/// <summary>
/// Cheap stand-in for Unity's <see cref="Projector"/>, meant to replace decal projectors placed in a scene.
/// <para>
/// A real <see cref="Projector"/> re-renders every affected object once per projector, so a level dressed with
/// blob shadows, light pools and grunge decals pays for them in extra draw calls and overdraw. Most of those
/// projections are static and land on flat-enough geometry, so they don't need the general case: this component
/// bakes the projection down to plain geometry that renders as regular batched/instanced draws instead.
/// </para>
/// <para>
/// Two authoring modes, selected by the runtime flag:
/// <list type="bullet">
/// <item><description><b>Editor-only</b> — the projection is built at level-design time and baked into the scene.</description></item>
/// <item><description><b>Runtime</b> — the projection is built in <c>Start</c>, for objects spawned during play.</description></item>
/// </list>
/// </para>
/// <para>
/// Settings can be authored directly on this component, or mirrored from an existing <see cref="Projector"/> via
/// the reference field, which makes converting an already-dressed scene a matter of pointing each FakeProjector at
/// the projector it replaces. When a build succeeds, the referenced projector is disabled so the two never overlap.
/// </para>
/// <para>
/// The first implementation only handles projections that collapse to a scaled quad; baking into a generated mesh
/// that conforms to the receiving surface is the intended follow-up. Live instances register with
/// <see cref="FakeProjectorBatcher"/>, which owns their culling and instanced draw calls, so cost scales with
/// visible projections rather than with authored ones.
/// </para>
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class FakeProjector : MonoBehaviour
{
    /// <summary>Minimum span between near and far, so a degenerate range still produces a castable ray.</summary>
    const float MIN_RANGE = 0.01f;

    /// <summary>Below this squared length a candidate up-vector is treated as parallel to the surface normal.</summary>
    const float DEGENERATE_UP_SQR = 1e-6f;

    /// <summary>
    /// When to build the projection: 
    /// <c>false</c> bakes it at level-design time in the editor,
    /// <c>true</c> builds it from <see cref="Start"/> for objects spawned during play.
    /// </summary>
    [SerializeField] bool _runtime;

    /// <summary>
    /// Optional source projector to mirror. When set, the projection settings below are read from it at build
    /// time rather than from this component, and it is disabled once the projection is successfully built.
    /// </summary>
    [SerializeField] Projector _refProjector;

    /// <summary>
    /// How the decal combines with the surface under it, when no material override is assigned.
    /// </summary>
    [SerializeField] EFakeProjectorRenderMode _renderMode = EFakeProjectorRenderMode.Diffuse;

    /// <summary>
    /// Optional material override. Leave empty to use the shared material for <see cref="_renderMode"/>, which
    /// is the batching-friendly default — see <see cref="FakeProjectorRenderModes"/>.
    /// <para>
    /// This is deliberately never the source projector's own material: projector shaders reconstruct UVs from a
    /// projection matrix the quad doesn't have, so the replacement needs an ordinary decal material. An override
    /// must have GPU instancing enabled or the batcher will skip it.
    /// </para>
    /// </summary>
    [SerializeField] Material _material;

    /// <summary>
    /// Optional texture override, applied per batch through a <see cref="MaterialPropertyBlock"/>. Lets many
    /// projectors share one material and still show different decals without breaking instancing — batches are
    /// keyed on the material/texture pair. Mirrored from the source projector's material when one is assigned.
    /// </summary>
    [SerializeField] Texture _texture;

    [Header( "Projection" )]
    [SerializeField] bool _orthographic = true;
    [SerializeField] float _orthographicSize = 1f;
    [SerializeField] float _fieldOfView = 60f;
    [SerializeField] float _aspectRatio = 1f;
    [SerializeField] float _nearClipPlane;
    [SerializeField] float _farClipPlane = 10f;

    /// <summary>Surfaces the projection can land on. Mirrored as the complement of the source projector's ignoreLayers.</summary>
    [Header( "Receiver" )]
    [SerializeField] LayerMask _receiverMask = ~0;

    /// <summary>Lift along the surface normal, to keep the quad off the receiving geometry and out of z-fighting.</summary>
    [SerializeField] float _surfaceOffset = 0.01f;

    /// <summary>
    /// When no receiver is hit, place the quad this far down the projection axis instead of failing the build.
    /// Keeps decals authored over geometry without colliders usable; disable to treat a miss as a failed bake.
    /// </summary>
    [SerializeField] bool _buildWithoutReceiver = true;
    [SerializeField] float _fallbackDistance = 1f;

    /// <summary>Rebuild in the editor whenever the settings change, so a tweak shows its result immediately.</summary>
    [SerializeField] bool _autoRebuild = true;

    /// <summary>The bake. Serialized, so an editor-built projection is ready before any code runs.</summary>
    [SerializeField] FakeProjectorQuad _baked;

    bool _registered;

    public FakeProjectorQuad Baked => _baked;
    public Texture Texture => _texture;

    /// <summary>
    /// The material this actually renders with: the override when one is assigned, otherwise the shared
    /// material for the chosen render mode.
    /// </summary>
    public Material Material => _material != null ? _material : FakeProjectorRenderModes.Get( _renderMode );

    /// <summary>True when a material was assigned by hand rather than taken from the render mode.</summary>
    public bool HasMaterialOverride => _material != null;

    public EFakeProjectorRenderMode RenderMode => _renderMode;
    public bool IsRuntime => _runtime;
    public bool Orthographic => _orthographic;
    public Projector RefProjector => _refProjector;

    /// <summary>
    /// True when this projection can join an instanced draw. A material without GPU instancing still renders,
    /// but the batcher skips it — which loses the entire point of replacing the projector.
    /// </summary>
    public bool IsBatchable
    {
        get
        {
            var material = Material;
            return material != null && material.enableInstancing;
        }
    }

    /// <summary>True when the source projector supplies the projection settings and they should not be hand-edited.</summary>
    public bool MirrorsRefProjector => _refProjector != null;

    void Start()
    {
        // Editor-only projections are already baked by this point, so only runtime ones build here.
        if( _runtime && Application.isPlaying )
            Build();
    }

    void OnEnable()
    {
        Register();
    }

    void OnDisable()
    {
        Unregister();
    }

    void Register()
    {
        // Nothing to draw until a bake exists — a runtime projector registers from Build instead.
        if( _registered || !_baked.Built || Material == null ) return;
        FakeProjectorBatcher.Register( this );
        _registered = true;
    }

    void Unregister()
    {
        if( !_registered ) return;
        FakeProjectorBatcher.Unregister( this );
        _registered = false;
    }

    /// <summary>
    /// Bakes the current settings into the renderable quad, replacing the source projector.
    /// </summary>
    /// <remarks>
    /// Only handles projections that collapse to a single scaled quad — flat-enough receiving geometry, which
    /// covers most authored decals. A later pass can bake into a mesh that conforms to the surface by replacing
    /// the placement step below; nothing outside this method assumes the result is a quad except the batcher's
    /// shared mesh.
    /// </remarks>
    /// <returns>True when a projection was produced, in which case the source projector has been disabled.</returns>
    public bool Build()
    {
        Unregister();
        _baked = default;

        // No material check: an unassigned material falls back to the shared render-mode one, so a projector
        // migrated with nothing configured still bakes and renders.
        MirrorRefProjector();

        var t = transform;
        var origin = t.position;
        var direction = t.forward;

        var near = Mathf.Max( 0f, _nearClipPlane );
        var far = Mathf.Max( near + MIN_RANGE, _farClipPlane );

        if( !TryFindReceiver( origin, direction, near, far, out var point, out var normal, out var hitReceiver ) )
            return false;

        var distance = Vector3.Distance( origin, point );
        var height = _orthographic
            ? _orthographicSize * 2f
            : 2f * distance * Mathf.Tan( _fieldOfView * 0.5f * Mathf.Deg2Rad );
        var width = height * Mathf.Max( 0.0001f, _aspectRatio );

        // Keep the decal's roll: the projector's own up, flattened onto the receiving surface. Falls back to
        // its right vector when the projection comes in parallel to the surface and up flattens to nothing.
        var up = Vector3.ProjectOnPlane( t.up, normal );
        if( up.sqrMagnitude < DEGENERATE_UP_SQR ) up = Vector3.ProjectOnPlane( t.right, normal );
        if( up.sqrMagnitude < DEGENERATE_UP_SQR ) up = Vector3.up;

        _baked = new FakeProjectorQuad(
            point + normal * _surfaceOffset,
            Quaternion.LookRotation( normal, up.normalized ),
            new Vector2( width, height ),
            hitReceiver );

        // Only now that the replacement exists is it safe to drop the original — a failed bake above leaves the
        // real projector running rather than losing the decal entirely.
        if( _refProjector != null && _refProjector.enabled )
        {
#if UNITY_EDITOR
            if( !Application.isPlaying ) UnityEditor.Undo.RecordObject( _refProjector, "Disable Replaced Projector" );
#endif
            _refProjector.enabled = false;
        }

        if( isActiveAndEnabled ) Register();
        return true;
    }

    /// <summary>Discards the bake and stops the quad from rendering, leaving settings untouched.</summary>
    public void Clear()
    {
        Unregister();
        _baked = default;
    }

    /// <summary>Copies the projection settings from the source projector, if one is assigned.</summary>
    public void MirrorRefProjector()
    {
        if( _refProjector == null ) return;

        _orthographic = _refProjector.orthographic;
        _orthographicSize = _refProjector.orthographicSize;
        _fieldOfView = _refProjector.fieldOfView;
        _aspectRatio = _refProjector.aspectRatio;
        _nearClipPlane = _refProjector.nearClipPlane;
        _farClipPlane = _refProjector.farClipPlane;
        _receiverMask = ~_refProjector.ignoreLayers;

        // Projector shaders carry the decal in _ShadowTex; fall back to _MainTex for custom ones. Only the
        // texture transfers — the material itself can't render without the projector's projection matrix.
        var refMaterial = _refProjector.material;
        if( refMaterial == null ) return;
        if( refMaterial.HasProperty( "_ShadowTex" ) ) _texture = refMaterial.GetTexture( "_ShadowTex" );
        else if( refMaterial.HasProperty( "_MainTex" ) ) _texture = refMaterial.GetTexture( "_MainTex" );

        if( !HasMaterialOverride ) _renderMode = InferRenderMode( refMaterial );
    }

    /// <summary>
    /// Guesses the render mode from the source material's shader, so migrating a scene doesn't turn every blob
    /// shadow into an alpha-blended square. Only the legacy projector shader family is recognised; anything
    /// else keeps whatever mode is already set.
    /// </summary>
    EFakeProjectorRenderMode InferRenderMode( Material refMaterial )
    {
        var shaderName = refMaterial.shader != null ? refMaterial.shader.name : string.Empty;

        // "Projector/Multiply" darkens, "Projector/Light" adds — the two shaders Unity ships.
        if( shaderName.Contains( "Multiply" ) ) return EFakeProjectorRenderMode.Multiply;
        if( shaderName.Contains( "Light" ) || shaderName.Contains( "Additive" ) ) return EFakeProjectorRenderMode.Additive;
        return _renderMode;
    }

    /// <summary>
    /// Finds where the projection lands, as a point and surface normal. Falls back to a fixed distance facing
    /// back along the projection axis when nothing is hit and that is allowed.
    /// </summary>
    bool TryFindReceiver( Vector3 origin, Vector3 direction, float near, float far, out Vector3 point, out Vector3 normal, out bool hitReceiver )
    {
        hitReceiver = false;
#if UNITY_EDITOR
        // Colliders follow transforms lazily outside play mode, so a projector moved this frame would otherwise
        // be cast against stale positions.
        if( !Application.isPlaying ) Physics.SyncTransforms();
#endif
        // Cast in the physics scene this object actually belongs to, not the default one. For a normal scene
        // those are the same, but a prefab stage puts its contents in a preview scene with its own physics —
        // casting globally there would miss every collider in the prefab being edited and silently fall back.
        var scene = gameObject.scene;
        var physics = scene.IsValid() ? scene.GetPhysicsScene() : Physics.defaultPhysicsScene;

        if( physics.Raycast( origin + direction * near, direction, out var hit, far - near, _receiverMask, QueryTriggerInteraction.Ignore ) )
        {
            point = hit.point;
            normal = hit.normal;
            hitReceiver = true;
            return true;
        }

        if( !_buildWithoutReceiver )
        {
            point = default;
            normal = default;
            return false;
        }

        point = origin + direction * Mathf.Clamp( _fallbackDistance, near, far );
        normal = -direction;
        return true;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        _orthographicSize = Mathf.Max( 0f, _orthographicSize );
        _fieldOfView = Mathf.Clamp( _fieldOfView, 1f, 179f );
        _aspectRatio = Mathf.Max( 0.0001f, _aspectRatio );
        _nearClipPlane = Mathf.Max( 0f, _nearClipPlane );
        _farClipPlane = Mathf.Max( _nearClipPlane + MIN_RANGE, _farClipPlane );

        // The material or texture may have just changed, which is what the batcher buckets on.
        if( _registered ) FakeProjectorBatcher.SetDirty();

        // Rebuild on edit rather than dirty-checking every editor update: a scene holding hundreds of these
        // would pay for the poll every frame, while edits are rare and already notify us here. Deferred because
        // OnValidate runs mid-serialization, where building (and touching physics) is not allowed.
        if( !_autoRebuild || _runtime || Application.isPlaying ) return;
        UnityEditor.EditorApplication.delayCall += RebuildFromEditor;
    }

    void RebuildFromEditor()
    {
        UnityEditor.EditorApplication.delayCall -= RebuildFromEditor;
        if( this == null || Application.isPlaying ) return;

        UnityEditor.Undo.RecordObject( this, "Build Fake Projector" );
        Build();
        UnityEditor.EditorUtility.SetDirty( this );
    }

    // ─── Gizmos ──────────────────────────────────────────────────────────────
    //
    // Everything the bake decided, drawn where it happened: the volume being projected, where it landed, which
    // way the decal is facing and rolled, and how big the batcher thinks it is. The point is that the usual
    // failures — nothing to land on, a decal facing the wrong way, a projector aimed at nothing — are visible
    // in the scene view instead of having to be inferred from a wrong-looking result.

    static readonly Color GIZMO_VOLUME_COLOR = new( 1f, 0.85f, 0.3f, 0.75f );
    static readonly Color GIZMO_QUAD_COLOR = new( 0.3f, 0.9f, 1f, 0.9f );
    static readonly Color GIZMO_QUAD_FILL = new( 0.3f, 0.9f, 1f, 0.15f );
    static readonly Color GIZMO_FALLBACK_COLOR = new( 1f, 0.5f, 0.15f, 0.9f );
    static readonly Color GIZMO_FALLBACK_FILL = new( 1f, 0.5f, 0.15f, 0.15f );
    static readonly Color GIZMO_NORMAL_COLOR = new( 0.4f, 1f, 0.5f, 1f );
    static readonly Color GIZMO_UP_COLOR = new( 1f, 0.45f, 0.45f, 1f );
    static readonly Color GIZMO_BOUNDS_COLOR = new( 1f, 1f, 1f, 0.18f );
    static readonly Color GIZMO_ERROR_COLOR = new( 1f, 0.3f, 0.3f, 1f );

    static GUIStyle _gizmoLabelStyle;

    /// <summary>
    /// Draws the debug gizmos. Called from an editor-side <c>DrawGizmo</c> drawer rather than through
    /// <c>OnDrawGizmosSelected</c>, because Unity skips gizmo messages on disabled components — and a disabled
    /// projection is exactly the case worth looking at while migrating a scene.
    /// </summary>
    /// <param name="detailed">
    /// False for the ambient pass over unselected projections, which draws the shapes but skips the labels,
    /// arrows and culling sphere so a whole level's worth of them stays readable.
    /// </param>
    public void DrawDebugGizmos( bool detailed = true )
    {
        DrawProjectionVolumeGizmo();
        DrawBakeGizmo( detailed );
    }

    /// <summary>Draws what the projector covers — the same volume a real Projector would affect.</summary>
    void DrawProjectionVolumeGizmo()
    {
        var near = Mathf.Max( 0f, _nearClipPlane );
        var far = Mathf.Max( near + MIN_RANGE, _farClipPlane );
        var aspect = Mathf.Max( 0.0001f, _aspectRatio );

        var previousMatrix = Gizmos.matrix;
        var previouscolor = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = GIZMO_VOLUME_COLOR;

        if( _orthographic )
        {
            var height = _orthographicSize * 2f;
            Gizmos.DrawWireCube(
                new Vector3( 0f, 0f, ( near + far ) * 0.5f ),
                new Vector3( height * aspect, height, far - near ) );
        }
        else
        {
            // DrawFrustum takes the vertical FOV and looks down +Z, which is exactly how a Projector projects.
            Gizmos.DrawFrustum( Vector3.zero, _fieldOfView, far, near, aspect );
        }

        Gizmos.matrix = previousMatrix;
        Gizmos.color = previouscolor;
    }

    /// <summary>Draws the baked quad, its orientation, and the sphere the batcher culls it by.</summary>
    void DrawBakeGizmo( bool detailed )
    {
        if( !_baked.Built )
        {
            if( detailed ) DrawGizmoLabel( transform.position, "not built", GIZMO_ERROR_COLOR );
            return;
        }

        // A fallback bake is the single most misleading state — it looks like a working decal that is simply
        // in the wrong place, so it gets its own colour rather than blending in with a real hit.
        var hit = _baked.HitReceiver;
        var outline = hit ? GIZMO_QUAD_COLOR : GIZMO_FALLBACK_COLOR;
        var fill = hit ? GIZMO_QUAD_FILL : GIZMO_FALLBACK_FILL;

        var previousMatrix = Gizmos.matrix;
        var previousColor = Gizmos.color;
        Gizmos.matrix = _baked.ToMatrix();
        Gizmos.color = fill;
        Gizmos.DrawCube( Vector3.zero, new Vector3( 1f, 1f, 0f ) );
        Gizmos.color = outline;
        Gizmos.DrawWireCube( Vector3.zero, new Vector3( 1f, 1f, 0f ) );
        Gizmos.matrix = previousMatrix;

        var position = _baked.Position;

        // Where the projection actually travelled, which is not the full near/far range once it hits something.
        Gizmos.color = outline;
        Gizmos.DrawLine( transform.position, position );

        // The rest is per-projection detail — useful on the one being inspected, noise across a whole level.
        if( detailed )
        {
            var axisLength = Mathf.Max( _baked.Size.x, _baked.Size.y ) * 0.35f;
            DrawArrowGizmo( position, _baked.Normal * axisLength, GIZMO_NORMAL_COLOR );
            DrawArrowGizmo( position, _baked.Up * axisLength, GIZMO_UP_COLOR );

            Gizmos.color = GIZMO_BOUNDS_COLOR;
            Gizmos.DrawWireSphere( position, _baked.BoundingRadius );

            DrawGizmoLabel( position, DescribeBake(), hit ? GIZMO_QUAD_COLOR : GIZMO_FALLBACK_COLOR );
        }

        Gizmos.color = previousColor;
    }

    string DescribeBake()
    {
        var mode = HasMaterialOverride ? "override" : _renderMode.ToString();
        var text = $"{_baked.Size.x:0.##} x {_baked.Size.y:0.##}  {mode}  {( _runtime ? "runtime" : "editor" )}";

        if( !_baked.HitReceiver ) text += "\nno receiver — fallback placement";
        if( !IsBatchable ) text += "\nnot batchable — material lacks GPU instancing";
        return text;
    }

    /// <summary>Line with a simple two-stroke head, enough to read direction at scene-view scale.</summary>
    static void DrawArrowGizmo( Vector3 origin, Vector3 direction, Color color )
    {
        if( direction.sqrMagnitude < DEGENERATE_UP_SQR ) return;

        Gizmos.color = color;
        var tip = origin + direction;
        Gizmos.DrawLine( origin, tip );

        // Any vector not parallel to the arrow works to spread the head; the cross picks one deterministically.
        var side = Vector3.Cross( direction, Vector3.up );
        if( side.sqrMagnitude < DEGENERATE_UP_SQR ) side = Vector3.Cross( direction, Vector3.right );
        side = side.normalized * ( direction.magnitude * 0.15f );

        var back = tip - direction * 0.25f;
        Gizmos.DrawLine( tip, back + side );
        Gizmos.DrawLine( tip, back - side );
    }

    static void DrawGizmoLabel( Vector3 position, string text, Color color )
    {
        _gizmoLabelStyle ??= new GUIStyle( UnityEditor.EditorStyles.miniLabel ) { richText = false };
        _gizmoLabelStyle.normal.textColor = color;
        UnityEditor.Handles.Label( position, text, _gizmoLabelStyle );
    }
#endif
}
