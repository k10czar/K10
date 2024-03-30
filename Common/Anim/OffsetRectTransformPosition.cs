using UnityEngine;

public class OffsetRectTransformPosition : MonoBehaviour, IAnimatable
{
    [SerializeField] float _offset;
    [SerializeField] Vector3[] _direction;
    [SerializeField] Vector3[] _rotation;

    Vector3 _snapshotLocalPos;
    Quaternion _snapshotLocalRot;
    float _lastOffset;

    public float AnimationValue => _offset;

    void Awake()
    {
        Snapshot();
    }

#if UNITY_EDITOR
    void Update()
    {
        if( MathAdapter.Approximately( _lastOffset, _offset ) ) return;
        UpdateOffset();
    }
#endif

    void Snapshot()
    {
        var rectT = (RectTransform)transform;
        _snapshotLocalPos = rectT.localPosition;
        _snapshotLocalRot = rectT.localRotation;
        _lastOffset = _offset;
    }

    public void SetOffset( float value )
    {
        _offset = value;
        UpdateOffset();
    }

    public void SetAnimationValue( float value )
    {
        SetOffset( value );
    }

    void UpdateOffset()
    {
        _lastOffset = _offset;
        var rectT = (RectTransform)transform;
        var offsetPos = _snapshotLocalPos + _direction.GetHasPolynomialResult( _offset );
        var offsetRot = _snapshotLocalRot * Quaternion.Euler( _rotation.GetHasPolynomialResult( _offset ) );
        rectT.SetPositionAndRotation( offsetPos, offsetRot );
    }
}