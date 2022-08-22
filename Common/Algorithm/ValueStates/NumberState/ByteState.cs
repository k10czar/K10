using UnityEngine;

[System.Serializable]
public class ByteState : INumericValueState<byte>, ICustomDisposableKill
{
	[SerializeField] byte _value;
	[System.NonSerialized] EventSlot<byte> _onChange = new EventSlot<byte>();

	public byte Value { get { return _value; } set { Setter( value ); } }
	public byte Get() { return _value; }

	public void Setter( byte value )
	{
		if( _value == value ) return;
		_value = value;
		_onChange?.Trigger( value );
	}

	public void Increment( byte value = 1 )
	{
		if( value == 0 ) return;
		Setter( (byte)( _value + value ) );
	}

	public void Kill()
	{
		_onChange?.Kill();
		_onChange = null;
	}

	public IEventRegister<byte> OnChange => Lazy.Request( ref _onChange );

	public ByteState( byte initialValue = default( byte ) ) { _value = initialValue; }

	public override string ToString() { return $"ByS({_value})"; }
}