using UnityEngine;

[System.Serializable]
public class QuaternionState : IValueState<Quaternion>, ISerializationCallbackReceiver
{
	[SerializeField] Quaternion _value;
	[System.NonSerialized] EventSlot<Quaternion> _onChange = new EventSlot<Quaternion>();

	public Quaternion Value { get { return _value; } set { Setter( value ); } }
	public Quaternion Get() { return _value; }

	public void Setter( Quaternion value )
	{
		if( _value == value ) return;
		_value = value;
		_onChange.Trigger( value );
	}

	public IEventRegister<Quaternion> OnChange { get { return _onChange; } }

	public QuaternionState( Quaternion initialValue = default( Quaternion ) ) { _value = initialValue; Init(); }
	void Init()
	{
		if( _onChange == null ) _onChange = new EventSlot<Quaternion>();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
	void ISerializationCallbackReceiver.OnAfterDeserialize() { Init(); }


	public override string ToString() { return string.Format( "QS({1})", typeof( Quaternion ).ToString(), _value ); }
}
