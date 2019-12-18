using UnityEngine;
using System.Collections;
using System;

public class FloatAnimator01 : FloatAnimator
{
    public FloatAnimator01( float baseValue = 10 ) : base( 0, 1, baseValue, baseValue, baseValue ) { }
    public FloatAnimator01( float acceleration, float deceleration, float maxSpeed ) : base( 0, 1, acceleration, deceleration, maxSpeed ) { }
    public FloatAnimator01( IUpdaterOnDemand updater, float baseValue = 10 ) : base( updater, 0, 1, baseValue, baseValue, baseValue ) { }
    public FloatAnimator01( float startValue, IUpdaterOnDemand updater, float baseValue = 10 ) : base( updater, 0, 1, baseValue, baseValue, baseValue ) { Start( startValue ); }

	public void Reset( float acceleration, float deceleration, float maxSpeed ){ SetAcceleration( acceleration );SetDeacceleration(deceleration); SetMaximumVelocity(maxSpeed); Force(0); }
}

[System.Serializable]
public class FloatAnimator : IValueState<float>, IUpdatableOnDemand
{
    [SerializeField] float _min = float.MinValue;
    [SerializeField] float _max = float.MaxValue;
    [SerializeField] float _acceleration = 60;
    [SerializeField] float _deacceleration = 60;
    [SerializeField] float _maxVelocity = 10;

	IUpdaterOnDemand _updater;

	public FloatAnimator() { _isOnDesired.OnChange.Register( CheckOnMin ); _isOnDesired.OnChange.Register( CheckOnMax ); _isOnDesired.OnFalseState.Register( RequestUpdate ); }
	public FloatAnimator( float min, float max ) : this( min, max, 60, 60, 10 ) {}
	public FloatAnimator( float min, float max, float baseValue ) : this( min, max, baseValue, baseValue, baseValue ) {}
	public FloatAnimator( float min, float max, float accel, float deaccel ) : this( min, max, accel, deaccel, 10 ) {}
	public FloatAnimator( float min, float max, float accel, float deaccel, float maxVelocity ) : this() { _min = min; _max = max; _acceleration = accel; _deacceleration = deaccel; _maxVelocity = maxVelocity; }
	public FloatAnimator( IUpdaterOnDemand updater ) { _updater = updater; }
	public FloatAnimator( IUpdaterOnDemand updater, float min, float max ) : this( min, max, 60, 60, 10 ) { _updater = updater; }
	public FloatAnimator( IUpdaterOnDemand updater, float min, float max, float accel, float deaccel ) : this( min, max, accel, deaccel, 10 ) { _updater = updater; }
	public FloatAnimator( IUpdaterOnDemand updater, float min, float max, float accel, float deaccel, float maxVelocity ) : this( min, max, accel, deaccel, maxVelocity )  { _updater = updater; }

    float _velocity, _current, _desired;

	void CheckOnMin() { _isOnMinimumValue.Value = Mathf.Approximately( _current, _min ) && Mathf.Approximately( _desired, _min ); }
    void CheckOnMax() { _isOnMaximumValue.Value = Mathf.Approximately( _current, _max ) && Mathf.Approximately( _desired, _max ); }
    void CheckDesired() { _isOnDesired.Value = Mathf.Approximately( _current, _desired ); }

	public IUpdaterOnDemand Updater { set { _updater = value; } }
    public IEventRegister OnValueReach { get { return _isOnDesired.OnTrueState; } }

    BoolState _isOnDesired = new BoolState( false );
    public IBoolStateObserver IsOnDesired { get { return _isOnDesired; } }

    BoolState _isOnMinimumValue = new BoolState();
    public IBoolStateObserver IsOnMinimumValue { get { return _isOnMinimumValue; } }

    BoolState _isOnMaximumValue = new BoolState();
    public IBoolStateObserver IsOnMaximumValue { get { return _isOnMaximumValue; } }

	EventSlot<float> _onValueUpdate = new EventSlot<float>();
	public IEventRegister<float> OnChange { get { return _onValueUpdate; } }

    void IValueStateSetter<float>.Setter( float t ) { SetDesire( t ); }
    

	public float Value { get { return _current; } }
    public float Get() { return _current; }
	public float DesiredValue { get { return _desired; } set { SetDesire( value ); } }

	public float CurrentVelocity { get { return _velocity; } }
	public float MaxVelocity { get { return _maxVelocity; } }

    public float Max { get { return _max; } }
    public float Min { get { return _min; } }

    public void SetDesiredToMaximum() { SetDesiredMax( true ); }
    public void SetDesiredMax( bool max ) { SetDesire( max ? _max : _min ); }
    public void SetDesiredToMinimum() { SetDesiredMin( true ); }
    public void SetDesiredMin( bool min ) { SetDesire( min ? _min : _max ); }

	public void Force( float value )
	{
        var val = Mathf.Clamp( value, _min, _max );

		if( Mathf.Approximately( _current, val ) )
			return;

		_current = val;

        _onValueUpdate.Trigger( _current );
        CheckDesired();
	}

    public void Start( float startValue )
    {
		var val = Mathf.Clamp( startValue, _min, _max );
        _desired = val;
		_velocity = 0;

        _current = val;

        _onValueUpdate.Trigger( _current );
        CheckDesired();
	}

	public static float EasyInEasyOut( float val, float power = .5f )
	{
		val -= .5f;
		val *= 2;
		if( val < 0 ) val = -Mathf.Pow( -val, power );
		else val = Mathf.Pow( val, power );

		return ( val + 1 ) * .5f;
	}

	public void SetRange( float min, float max ) { _min = min; _max = max; _desired = Mathf.Clamp( _desired, _min, _max ); _current = Mathf.Clamp( _current, _min, _max ); }
    public void SetMinimum( float min ) { _min = min; _desired = Mathf.Clamp( _desired, _min, _max ); _current = Mathf.Clamp( _current, _min, _max ); CheckOnMin(); if( Mathf.Approximately( _min, _max ) ) CheckOnMax(); }
    public void SetMaximum( float max ) { _max = max; _desired = Mathf.Clamp( _desired, _min, _max ); _current = Mathf.Clamp( _current, _min, _max ); CheckOnMax(); if( Mathf.Approximately( _min, _max ) ) CheckOnMin(); }

	public void SetAcceleration( float acc ) { _acceleration = acc; }
	public void SetDeacceleration( float deacc ) { _deacceleration = deacc; }
	public void SetMaximumVelocity( float velocity ) { _maxVelocity = velocity; }

    public void SetDesire( float desired )
    {
		if( Mathf.Approximately( _desired, desired ) )
			return;
        _desired = Mathf.Clamp( desired, _min, _max );
		CheckDesired();
    }

	void RequestUpdate() { if( _updater != null ) _updater.RequestUpdate( this ); }

    public bool Update( float deltaTime )
	{
		if( Mathf.Approximately( deltaTime, 0 ) ) return !_isOnDesired.Value;

        float diff = _desired - _current;
        var aVel = _velocity + Mathf.Sign( diff ) * _acceleration * deltaTime;
        var dVel = Mathf.Sqrt( Mathf.Abs( 2 * _deacceleration * diff ) );
        var vel = Mathf.Sign( aVel ) * Mathf.Min( Mathf.Abs( aVel ), Mathf.Abs( dVel ) );
        _velocity = Mathf.Clamp( vel, -_maxVelocity, _maxVelocity );
		var step = _velocity * deltaTime;

		if( !Mathf.Approximately( step, 0 ) )
		{
			if( Mathf.Abs( step ) >= Mathf.Abs( diff ) )
	        {
	            _current = _desired;
				_onValueUpdate.Trigger( _current );
				_isOnDesired.Value = true;

	            if( _velocity != 0 ) { _velocity = 0; }
	        }
	        else
	        {
				_current += step;
				_onValueUpdate.Trigger( _current );

				if( Mathf.Approximately( _current, _desired ) )
				{
					_current = _desired;
					_isOnDesired.Value = true;
					_velocity = 0;
	            }
	        }
		}

		if( float.IsNaN( _current ) ) Debug.LogError( string.Format( "NaN on float animator | _velocity:{0} _maxVelocity:{1} deltaTime:{2} _desired:{3} _current:{4} diff:{5} aVel:{6} dVel:{7} vel:{8} step:{9}", _velocity, _maxVelocity, deltaTime, _desired, _current, diff, aVel, dVel, vel, step ) );

		return !_isOnDesired.Value;
    }
}