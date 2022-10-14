using System;
using UnityEngine;

public class DegreeAnimator : CircularFloatAnimator
{
    public DegreeAnimator() : base( -180f, 180f ) { }
    public DegreeAnimator( float accel, float deaccel, float maxVelocity ) : base( -180f, 180f, accel, deaccel, maxVelocity ) { }
    public DegreeAnimator( float baseValue ) : this( null, baseValue, baseValue, baseValue ) { }
    public DegreeAnimator( IUpdaterOnDemand updater, float baseValue ) : this( updater, baseValue, baseValue, baseValue ) { }
    public DegreeAnimator( IUpdaterOnDemand updater, float accel, float deaccel, float maxVelocity ) : base( updater, -180f, 180f, accel, deaccel, maxVelocity ) { }
}

public class RadianAnimator : CircularFloatAnimator
{
    public RadianAnimator() : base( -Mathf.PI, Mathf.PI ) { }
    public RadianAnimator( IUpdaterOnDemand updater, float baseValue ) : this( updater, baseValue, baseValue, baseValue ) { }
    public RadianAnimator( IUpdaterOnDemand updater, float accel, float deaccel, float maxVelocity ) : base( updater, -Mathf.PI, Mathf.PI, accel, deaccel, maxVelocity ) { }
}

public class CircularFloatAnimator : IValueState<float>, ICustomDisposableKill
{
    FloatAnimator _animator;

    CircularFloatAnimator( FloatAnimator value ) { _animator = value; _animator.OnChange.Register( ( val ) => _onValueUpdate.Trigger( Convert( val ) ) ); }
    public CircularFloatAnimator( float min, float max ) : this( new FloatAnimator( min, RealMax( min, max ) ) ) { }
    public CircularFloatAnimator( float min, float max, float accel, float deaccel ) : this( new FloatAnimator( min, RealMax( min, max ), accel, deaccel ) ) { }
    public CircularFloatAnimator( float min, float max, float accel, float deaccel, float maxVelocity ) : this( new FloatAnimator( min, RealMax( min, max ), accel, deaccel, maxVelocity ) ) { }
    public CircularFloatAnimator( IUpdaterOnDemand updater, float min, float max, float accel, float deaccel, float maxVelocity ) : this( new FloatAnimator( updater, min, RealMax( min, max ), accel, deaccel, maxVelocity ) ) { }

    public float CurrentVelocity { get { return _animator.CurrentVelocity; } }
    public float MaxVelocity { get { return _animator.MaxVelocity; } }
    public float Max { get { var min = Min; return min + ( _animator.Max - min ) * .5f; } }
    public float Min { get { return _animator.Min; } }

    EventSlot<float> _onValueUpdate = new EventSlot<float>();
    public IEventRegister<float> OnChange { get { return _onValueUpdate; } }
    void IValueStateSetter<float>.Setter( float t ) { SetDesire( t ); }

	public void Kill()
	{
		_animator?.Kill();
		_onValueUpdate?.Kill();
	}

    static float RealMax( float min, float max ) { return min + 2 * ( max - min ); }
    float Convert( float value ) { var min = Min; return min + ( ( value - min ) % ( ( _animator.Max - min ) * .5f ) ); }

    public float Value { get { return Convert( _animator.Value ); } }
    public float Get() { return Convert( _animator.Value ); }
    public float DesiredValue { get { return Convert( _animator.DesiredValue ); ; } set { SetDesire( value ); } }

    public void SetRange( float min, float max ) { _animator.SetRange( min, min + ( max - min ) * 2 ); }

    public void SetAcceleration( float acc ) { _animator.SetAcceleration( acc ); }
    public void SetDeacceleration( float deacc ) { _animator.SetAcceleration( deacc ); }
    public void SetMaximumVelocity( float velocity ) { _animator.SetAcceleration( velocity ); }

    public void Start( float value ) { _animator.Start( value ); }

    public void SetDesire( float value )
    {
        //TODO: Predict better side with current speed and distance
        var cur = Value;

        var min = Min;
        var half = ( _animator.Max - min ) * .5f;

        var a = Mathf.Abs( cur - ( value + half ) );
        var b = Mathf.Abs( ( cur + half ) - value );
        var c = Mathf.Abs( cur - value );

        if( c < a && c < b ) { _animator.SetDesire( value ); _animator.Force( cur ); }
        else if( b < a ) { _animator.SetDesire( value ); _animator.Force( cur + half ); }
        else { _animator.SetDesire( value + half ); _animator.Force( cur ); }
    }

    public void Update( float delta ) { _animator.Update( delta ); }
}