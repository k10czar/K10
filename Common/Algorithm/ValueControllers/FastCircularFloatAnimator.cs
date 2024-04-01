using UnityEngine;

[System.Serializable]
public struct FastCircularFloatAnimator
{
    [SerializeField] private FastFloatAnimator _animator;
    float _realRangeDelta;
    public float _currentValue;

    public FastCircularFloatAnimator( float initialValue, float accel, float deaccel, float maximumSpeed, float min, float max )
    {
        _currentValue = initialValue;
        _realRangeDelta = max - min;
        if( !float.IsFinite( _realRangeDelta ) ) _realRangeDelta = float.MaxValue;
        _animator = new FastFloatAnimator( initialValue, accel, deaccel, maximumSpeed, min, ExpandedMax( min, _realRangeDelta ) );
    }
    
    static float ExpandedMax( float min, float range ) 
    {
        return min + 2 * range;
    }
    // float Convert( float value ) { var min = _animator._min; return min + ( ( value - min ) % _halfRangeDelta ); }
    
	public void ForceToDesired()
	{
		_animator.ForceToDesired();
        UpdateValue();
	}

    private void UpdateValue()
    {
        var min = _animator._min;
        _currentValue = min + ( ( _animator._currentValue - min ) % _realRangeDelta );
        // _currentValue = Convert( _animator._currentValue );
    }
    
    public void SetDesire( float value )
    {
        var diff = _animator._desiredValue - value;
        if( diff < float.Epsilon && diff > FloatHelper.NegativeEpsilon ) return;

        // if( value < _animator._min ) value = _animator._min;
        // if( value > _animator._min + _realRangeDelta ) value = _animator._min + _realRangeDelta;
        
        // diff = _animator._desiredValue - value;
        // if( diff < float.Epsilon && diff > FloatHelper.NegativeEpsilon ) return;

        var a = ( value + _realRangeDelta ) - _currentValue;
        var b = ( _currentValue + _realRangeDelta ) - value;
        var c = ( _currentValue < value ) ? ( value - _currentValue ) : ( _currentValue - value );

        if( c < a && c < b ) { _animator.SetDesire( value ); _animator._currentValue = _currentValue; }
        else if( b < a ) { _animator.SetDesire( value ); _animator._currentValue = _currentValue + _realRangeDelta; }
        else { _animator.SetDesire( value + _realRangeDelta ); _animator._currentValue = _currentValue; }
    }

    public void Update( float deltaTime )
    {
        _animator.Update( deltaTime );
        UpdateValue();
    }

    public override string ToString() => $"[(FastCircularFloatAnimator)_realRangeDelta:{_realRangeDelta}|_currentValue:{_currentValue}|_animator:{_animator}]";
}
