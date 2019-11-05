using UnityEngine;

public class OverTimeModifier
{
	private float _value;
	private float _speed;
	private float _addedValue = 0;
	private float _offset;

	public bool IsOver => Mathf.Approximately( _addedValue, _value );

	public OverTimeModifier() { }

	public void Reset( float value, float time, float offset = 0 )
	{
		_value = value;
		_speed = value / time;
		_offset = offset;
	}

	public OverTimeModifier Reseted( float value, float time, float offset = 0 )
	{
		Reset( value, time, offset );
		return this;
	}

	public void Tick( float deltaTime, INumericValueStateSetter<float> value )
	{
		var maxIncrement = _value - _addedValue;
		var delta = deltaTime + _offset;
		var increment = Mathf.Min( _speed * delta, maxIncrement );
		_offset = 0;
		_addedValue += increment;
		value.Increment( increment );
	}
}
