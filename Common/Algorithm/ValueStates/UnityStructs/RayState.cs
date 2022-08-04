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

        if( Mathf.Approximately( _vd.x, vd.x ) && Mathf.Approximately( _vd.y, vd.y ) && Mathf.Approximately( _vd.z, vd.z ) &&
            Mathf.Approximately( _vo.x, vo.x ) && Mathf.Approximately( _vo.y, vo.y ) && Mathf.Approximately( _vo.z, vo.z ) ) return;

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