using UnityEngine;

/// <summary>
/// The baked result of a <see cref="FakeProjector"/>: the world-space placement of the quad that replaces
/// the projection. Serialized so an editor-time bake survives domain reloads and scene reloads.
/// </summary>
/// <remarks>
/// Stored as loose TRS components rather than a <see cref="Matrix4x4"/> so the values stay readable and
/// diffable in the inspector and in version control. The batcher converts to a matrix once per frame.
/// </remarks>
[System.Serializable]
public struct FakeProjectorQuad
{
    [SerializeField] bool _built;
    [SerializeField] bool _hitReceiver;
    [SerializeField] Vector3 _position;
    [SerializeField] Quaternion _rotation;
    [SerializeField] Vector2 _size;

    /// <summary>False until a successful bake, and the only thing callers should test before using the rest.</summary>
    public bool Built => _built;

    /// <summary>
    /// Whether the projection actually landed on a collider. False means it fell back to a fixed distance —
    /// the bake is valid but the decal is floating in mid-air rather than lying on anything, which is worth
    /// surfacing because it looks like a bug in the bake rather than missing colliders on the receiver.
    /// </summary>
    public bool HitReceiver => _hitReceiver;

    public Vector3 Position => _position;
    public Quaternion Rotation => _rotation;

    /// <summary>The surface normal the quad was laid against — its local +Z, by construction.</summary>
    public Vector3 Normal => _rotation * Vector3.forward;

    /// <summary>The decal's roll direction on the surface — its local +Y.</summary>
    public Vector3 Up => _rotation * Vector3.up;

    /// <summary>World-space width and height of the quad.</summary>
    public Vector2 Size => _size;

    /// <summary>
    /// Radius of the bounding sphere centred on <see cref="Position"/>. Half the diagonal, so it holds for any
    /// orientation — which is what lets the batcher cull without rebuilding bounds when the quad rotates.
    /// </summary>
    public float BoundingRadius => 0.5f * _size.magnitude;

    public FakeProjectorQuad( Vector3 position, Quaternion rotation, Vector2 size, bool hitReceiver )
    {
        _built = true;
        _hitReceiver = hitReceiver;
        _position = position;
        _rotation = rotation;
        _size = size;
    }

    /// <summary>Local-to-world matrix for the unit quad the batcher instances.</summary>
    public Matrix4x4 ToMatrix() => Matrix4x4.TRS( _position, _rotation, new Vector3( _size.x, _size.y, 1f ) );
}
