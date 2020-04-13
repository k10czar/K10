using UnityEngine;

[System.Serializable]
public class ByteState : INumericValueState<byte>, ISerializationCallbackReceiver
{
	[SerializeField] byte _value;
	[System.NonSerialized] EventSlot<byte> _onChange = new EventSlot<byte>();

	public byte Value { get { return _value; } set { Setter( value ); } }
	public byte Get() { return _value; }

	public void Setter( byte value )
	{
		if( _value == value ) return;
		_value = value;
		_onChange.Trigger( value );
	}

	public void Increment( byte value )
	{
		if( value == 0 ) return;
		Setter( (byte)( _value + value ) );
	}

	public IEventRegister<byte> OnChange { get { return _onChange; } }

	public ByteState( byte initialValue = default( byte ) ) { _value = initialValue; Init(); }
	void Init()
	{
		if( _onChange == null ) _onChange = new EventSlot<byte>();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
	void ISerializationCallbackReceiver.OnAfterDeserialize() { Init(); }

	public override string ToString() { return string.Format( "BS({1})", typeof( byte ).ToString(), _value ); }
}