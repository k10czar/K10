using UnityEngine;

[System.Serializable]
public class LongState : INumericValueState<long>, ICustomDisposableKill
{
    [SerializeField] long _value;
    [System.NonSerialized] EventSlot<long> _onChange;

    public long Value { get { return _value; } set { Setter( value ); } }
    public long Get() { return _value; }

    public void Setter( long value )
    {
        if( _value == value ) return;
        _value = value;

        _onChange?.Trigger( value );
    }

    public void Increment( long value = 1 )
    {
		if( value == 0 ) return;
		Setter( _value + value );
	}

	public void Kill()
	{
		_onChange?.Kill();
		_onChange = null;
	}

    public IEventRegister<long> OnChange => Lazy.Request( ref _onChange );

    public LongState( long initialValue = default( long) ) { _value = initialValue; }

	public override string ToString() { return $"LS({_value})"; }
}