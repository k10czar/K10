using UnityEngine;

public class InterpolatedOverTimeModifier
{
	private float _value;
	private float _lastValue;
	private float _offset;
	private float _duration;
	private float _time;
	IInterpolationFunction _interpolation;

	public bool IsOver => Mathf.Approximately( _time, _duration );
	public float LastingValue => _value - _lastValue;

	public InterpolatedOverTimeModifier() { }

	public void Reset( float value, float duration, IInterpolationFunction interpolation = null, float offset = 0 )
	{
		_value = value;
		_duration = duration;
		_time = _offset;
		_lastValue = 0;
		_interpolation = interpolation ?? LinearInterpolation.Instance;
	}

	public InterpolatedOverTimeModifier Reseted( float value, float duration, IInterpolationFunction interpolation = null, float offset = 0 )
	{
		Reset( value, duration, interpolation, offset );
		return this;
	}

	public void Tick( float deltaTime, INumericValueStateSetter<float> value )
	{
		_time += deltaTime;
		if( _time > _duration ) _time = _duration;
		var normilizedDuration = _time / _duration;
		var mod = _interpolation.Evaluate( normilizedDuration );
		var currentValue = mod * _value;
		var diff = currentValue - _lastValue;
		value.Increment( diff );
		_lastValue = currentValue;
	}
}
