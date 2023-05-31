using UnityEngine;

[System.Serializable]
public class RayState : IValueState<Ray>, ICustomDisposableKill
{
    [SerializeField] Ray _value;
    [System.NonSerialized] EventSlot<Ray> _onChange = new EventSlot<Ray>();

    public Ray Value { get { return _value; } set { Setter( value ); } }
    public Ray Get() { return _value; }

    public void Setter( Ray value )
    {
        var vd = value.direction;
        var vo = value.origin;
        var _vd = _value.direction;
        var _vo = _value.origin;

        var diffDX = _vd.x - vd.x;
        if( diffDX < float.Epsilon && diffDX > FloatHelper.NegativeEpsilon )
        {
            var diffDY = _vd.y - vd.y;
            if( diffDY < float.Epsilon && diffDY > FloatHelper.NegativeEpsilon )
            {
                var diffDZ = _vd.z - vd.z;
                if( diffDZ < float.Epsilon && diffDZ > FloatHelper.NegativeEpsilon )
                {
                    var diffOX = _vo.x - _vo.x;
                    if( diffOX < float.Epsilon && diffOX > FloatHelper.NegativeEpsilon )
                    {
                        var diffOY = _vo.y - _vo.y;
                        if( diffOY < float.Epsilon && diffOY > FloatHelper.NegativeEpsilon )
                        {
                            var diffOZ = _vo.z - _vo.z;
                            if( diffOZ < float.Epsilon && diffOZ > FloatHelper.NegativeEpsilon ) return;
                        }
                    }
                }
            }
        }

        _value = value;
        _onChange.Trigger( value );
    }

    public IEventRegister<Ray> OnChange { get { return _onChange; } }

    public RayState( Ray initialValue = default( Ray ) ) { _value = initialValue; }

    public override string ToString() { return string.Format( "V2S({1})", typeof( Ray ).ToString(), _value ); }

	public void Kill()
	{
		_onChange?.Kill();
	}
}