using System;
using UnityEngine;

[System.Serializable]
public struct FastFloatAnimator
{
    public float _currentValue;
    public float _desiredValue;
	public float _min;
    public float _max;
    public float _acceleration;
    public float _deacceleration;
    public float _currentSpeed;
    public float _maximumSpeed;
    
    public FastFloatAnimator( float baseValue ) : this( 0, baseValue, baseValue, baseValue ) { }
    public FastFloatAnimator( float initialValue, float accel, float deaccel, float maximumSpeed, float min = float.MinValue, float max = float.MaxValue )
    {
        _acceleration = accel;
        _deacceleration = deaccel;
        _maximumSpeed = maximumSpeed;
        _desiredValue = _currentValue = initialValue;
		_currentSpeed = 0;
	    _min = min;
        _max = max;
    }

	public bool IsOnDesired() { return Mathf.Approximately( _currentValue, _desiredValue ); }

	public void Reset( float value )
	{
        if( value > _max ) value = _max;
        else if( value < _min ) value = _min;
		_currentValue = _desiredValue = value;
		_currentSpeed = 0;
	}

	public void ForceToDesired()
	{
		_currentValue = _desiredValue;
		_currentSpeed = 0;
	}

	public void SetDesire( float desired )
    {
		var diff = desired - _desiredValue;
		if( diff < float.Epsilon && diff > FloatHelper.NegativeEpsilon )
			return;

        if( desired > _max ) _desiredValue = _max;
        else if( desired < _min ) _desiredValue = _min;
        else _desiredValue = desired;
    }

    public bool Update( float deltaTime )
    {
		if( deltaTime < float.Epsilon ) return false;

        var diff = _desiredValue - _currentValue;

        if( diff < float.Epsilon && diff > FloatHelper.NegativeEpsilon ) return false;

		var diffSign = 1f;
		if( diff < float.Epsilon ) diffSign = -1f;

        var acceleratedSpeed = _currentSpeed + diffSign * _acceleration * deltaTime;
        var deacceleratedSpeed = Mathf.Sqrt( 2 * _deacceleration * diff * diffSign );

		var speedDirectionSign = 1f;
		if( acceleratedSpeed < float.Epsilon ) speedDirectionSign = -1f;

		var speedAbs = acceleratedSpeed * speedDirectionSign;
		if( speedAbs > deacceleratedSpeed ) speedAbs = deacceleratedSpeed;
		if( speedAbs > _maximumSpeed ) speedAbs = _maximumSpeed;
        _currentSpeed = speedDirectionSign * speedAbs;

		var step = _currentSpeed * deltaTime;

		if( step > float.Epsilon ) // Not 0 == !Mathf.Approximately( step, 0 )
		{
			if( diffSign > 0 && ( diff - step ) < float.Epsilon )
	        {
	            _currentValue = _desiredValue;
	            _currentSpeed = 0;
	        }
	        else
	        {
				_currentValue += step;

        		var newDiff = _desiredValue - _currentValue;
				if( newDiff < float.Epsilon && newDiff > FloatHelper.NegativeEpsilon ) //if( Mathf.Approximately( _current, _desired ) )
				{
					_currentValue = _desiredValue;
					_currentSpeed = 0;
	            }
	        }
		}
		else if( step < FloatHelper.NegativeEpsilon )
		{
			if( diffSign < 0 && ( step - diff ) < float.Epsilon )
	        {
	            _currentValue = _desiredValue;
	            _currentSpeed = 0;

	        }
	        else
	        {
				_currentValue += step;

        		var newDiff = _desiredValue - _currentValue;
				if( newDiff < float.Epsilon && newDiff > FloatHelper.NegativeEpsilon ) //if( Mathf.Approximately( _current, _desired ) )
				{
					_currentValue = _desiredValue;
	            	_currentSpeed = 0;
	            }
	        }
		}
		else
		{
			_currentValue = _desiredValue;
			_currentSpeed = 0;
		}

		return true;
    }

    public override string ToString() => $"[(FastFloatAnimator) _min:{_min} _max:{_max} _acceleration:{_acceleration} _deacceleration:{_deacceleration} _currentValue:{_currentValue} _desiredValue:{_desiredValue} _currentSpeed:{_currentSpeed} _maximumSpeed:{_maximumSpeed}]";
}
