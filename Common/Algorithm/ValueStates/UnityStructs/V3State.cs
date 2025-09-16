using UnityEngine;

#if TRY_USE_NEW_MATH && !DO_NOT_USE_NEW_MATH //USE_NEW_MATHEMATICS
using Unity.Mathematics;
using v3 = Unity.Mathematics.float3;
#else
using v3 = UnityEngine.Vector3;
#endif

[System.Serializable]
public class V3State : IValueState<v3>, ICustomDisposableKill
{
    [SerializeField] v3 _value;

	// TODO: LazyOptimization
	[System.NonSerialized] EventSlot<v3> _onChange;
	// [System.NonSerialized] EventSlot<Vector3> _onChange = new EventSlot<Vector3>();
	public IEventRegister<v3> OnChange => Lazy.Request( ref _onChange );

    public v3 Value { get { return _value; } set { Setter( value ); } }
    public v3 Get() { return _value; }

    public void Setter( v3 value )
    {
        var xDiff = _value.x - value.x;
        if( xDiff < float.Epsilon && xDiff > FloatHelper.NegativeEpsilon )
        {
            var yDiff = _value.y - value.y;
            if( yDiff < float.Epsilon && yDiff > FloatHelper.NegativeEpsilon )
            {
                var zDiff = _value.z - value.z;
                if( zDiff < float.Epsilon && zDiff > FloatHelper.NegativeEpsilon )
                {
                    return;
                }
            }
        }

        _value = value;
        _onChange?.Trigger( value );
    }

	public void Kill()
	{
		_onChange?.Kill();
	}

    public V3State( v3 initialValue = default( v3 ) ) { _value = initialValue; }

    public override string ToString() { return string.Format( "V3S({1})", typeof( v3 ).ToString(), _value ); }
}