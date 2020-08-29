using UnityEngine;

[System.Serializable]
public class DoubleState : INumericValueState<double>, ISerializationCallbackReceiver
{
	[SerializeField] double _value;
	[System.NonSerialized] private EventSlot<double> _onChange = new EventSlot<double>();

	public double Value { get { return _value; } set { Setter( value ); } }
	public double Get() { return _value; }

	public void SetInt( int value ) { Setter( value ); }

	public void Setter( double value )
	{
		if( _value - value < double.Epsilon ) return;
		_value = value;
		_onChange.Trigger( value );
	}

	public void Increment( double increment )
	{
		if( increment == 0 ) return;
		Setter( _value + increment );
	}

	public IEventRegister<double> OnChange { get { return _onChange; } }

	public DoubleState( double initialValue = default( double ) ) { _value = initialValue; Init(); }
	void Init()
	{
		if( _onChange == null ) _onChange = new EventSlot<double>();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
	void ISerializationCallbackReceiver.OnAfterDeserialize() { Init(); }


	public override string ToString() { return string.Format( "FS({1})", typeof( double ).ToString(), _value ); }
}