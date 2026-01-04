using System;
using UnityEngine;

[System.Serializable]
public class DoubleState : INumericValueState<double>, ICustomDisposableKill
{
	[System.NonSerialized] bool _killed;
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
		if (_killed) return;
		_onChange?.Trigger( value );
	}

	public void Increment( double increment )
	{
		if( increment < double.Epsilon && increment > -double.Epsilon ) return;
		Setter( _value + increment );
	}

	public void Kill()
	{
		_killed = true;
		_onChange?.Kill();
		_onChange = null;
	}

	public IEventRegister<double> OnChange => _killed ? _onChange : _onChange ??= new();

	public DoubleState( double initialValue = default ) { _value = initialValue; }


	public override string ToString() { return $"DS({_value})"; }
}
