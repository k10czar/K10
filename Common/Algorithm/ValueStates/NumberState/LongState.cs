using UnityEngine;

[System.Serializable]
public class LongState : INumericValueState<long>, ISerializationCallbackReceiver
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

	public void Clear()
	{
		_onChange?.Clear();
		_onChange = null;
	}

    public IEventRegister<long> OnChange => _onChange ?? ( _onChange = new EventSlot<long>() );

    public LongState( long initialValue = default( long) ) { _value = initialValue; Init(); }
	void Init()
	{
		if( _onChange == null ) _onChange = new EventSlot<long>();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
	void ISerializationCallbackReceiver.OnAfterDeserialize() { Init(); }

	public override string ToString() { return $"LS({_value})"; }
}