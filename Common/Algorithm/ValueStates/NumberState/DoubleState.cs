using System;
using UnityEngine;

[System.Serializable]
public class DoubleState : INumericValueState<double>, ISerializationCallbackReceiver
{
	[SerializeField] double _value;
	[System.NonSerialized] private EventSlot<double> _onChange;

	public double Value { get { return _value; } set { Setter( value ); } }
	public double Get() { return _value; }

	public void SetInt( int value ) { Setter( value ); }

	public void Setter( double value )
	{
		if( Math.Abs( _value - value ) < double.Epsilon ) return;
		_value = value;
		_onChange?.Trigger( value );
	}

	public void Increment( double increment )
	{
		if( increment == 0 ) return;
		Setter( _value + increment );
	}

	public void Clear()
	{
		_onChange?.Clear();
		_onChange = null;
	}

	public IEventRegister<double> OnChange => _onChange ?? ( _onChange = new EventSlot<double>() );

	public DoubleState( double initialValue = default( double ) ) { _value = initialValue; Init(); }
	void Init()
	{
		if( _onChange == null ) _onChange = new EventSlot<double>();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
	void ISerializationCallbackReceiver.OnAfterDeserialize() { Init(); }


	public override string ToString() { return $"DS({_value})"; }
}