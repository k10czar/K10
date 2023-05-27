using System;
using UnityEngine;

[System.Serializable]
public class DoubleState : INumericValueState<double>, ICustomDisposableKill
{
	[SerializeField] double _value;
	[System.NonSerialized] private EventSlot<double> _onChange;

	public double Value { get { return _value; } set { Setter( value ); } }
	public double Get() { return _value; }

	public void SetInt( int value ) { Setter( value ); }

	public void Setter( double value )
	{
		var diff = _value - value;
		if( diff < double.Epsilon && diff > -double.Epsilon ) return;
		_value = value;
		_onChange?.Trigger( value );
	}

	public void Increment( double increment )
	{
		if( increment < double.Epsilon && increment > -double.Epsilon ) return;
		Setter( _value + increment );
	}

	public void Kill()
	{
		_onChange?.Kill();
		_onChange = null;
	}

	public IEventRegister<double> OnChange => Lazy.Request( ref _onChange );

	public DoubleState( double initialValue = default( double ) ) { _value = initialValue; }


	public override string ToString() { return $"DS({_value})"; }
}
