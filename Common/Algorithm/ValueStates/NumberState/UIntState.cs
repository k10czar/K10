using UnityEngine;

[System.Serializable]
public class UIntState : INumericValueState<uint>, ICustomDisposableKill
{
	[SerializeField] uint _value;
	[System.NonSerialized] EventSlot<uint> _onChange;

	public uint Value { get { return _value; } set { Setter( value ); } }
	public uint Get() { return _value; }

	public void Setter( uint value )
	{
		if( _value == value ) return;
		_value = value;
		_onChange?.Trigger( value );
	}

	public void Increment( uint value = 1 )
	{
		if( value == 0 ) return;
		Setter( _value + value );
	}

	public void Kill()
	{
		_onChange?.Kill();
		// _onChange = null;
	}

	public IEventRegister<uint> OnChange => _onChange ??= new();

	public UIntState( uint initialValue = default ) { _value = initialValue; }


	public override string ToString() { return $"US({_value})"; }
}