using System;
using UnityEngine;
using System.Runtime.CompilerServices;

[System.Serializable]
public class ColorAnimator : IValueState<Color>, IUpdatableOnDemand
{
	const float DEFAULT_ACCELERATION = 1;
	const float DEFAULT_DEACCELERATION = 1;
	const float DEFAULT_MAX_SPEED = 1;

	IUpdaterOnDemand _updater;

	[SerializeField] FloatAnimator01 _r;
	[SerializeField] FloatAnimator01 _g;
	[SerializeField] FloatAnimator01 _b;
	[SerializeField] FloatAnimator01 _a;

	float test;

	Color _cachedColor = Color.white;

	EventSlot<Color> _desiredReach = new EventSlot<Color>();
	public IEventRegister<Color> OnDesiredReach { get { return _desiredReach; } }

	public Color Value { get { return _cachedColor; } }
	public Color Get() { return _cachedColor; }
	public Color GetColor() { return _cachedColor; }

	EventSlot<Color> _colorUpdate = new EventSlot<Color>();
	public IEventRegister<Color> OnChange { get { return _colorUpdate; } }

	public Color Desired { get { return new Color( _r.DesiredValue, _g.DesiredValue, _b.DesiredValue, _a.DesiredValue ); } }

	public ColorAnimator() : this( null ) { }
	public ColorAnimator( float acceleration, float deceleration, float maxSpeed ) : this( null, acceleration, deceleration, maxSpeed ) { }
	public ColorAnimator( IUpdaterOnDemand updater ) : this( updater, DEFAULT_ACCELERATION, DEFAULT_DEACCELERATION, DEFAULT_MAX_SPEED ) { }
	public ColorAnimator( IUpdaterOnDemand updater, float acceleration, float deceleration, float maxSpeed )
	{
		_updater = updater;

		_r = new FloatAnimator01( acceleration, deceleration, maxSpeed );
		_g = new FloatAnimator01( acceleration, deceleration, maxSpeed );
		_b = new FloatAnimator01( acceleration, deceleration, maxSpeed );
		_a = new FloatAnimator01( acceleration, deceleration, maxSpeed );

		Start();
	}

	public ColorAnimator( float transitionTime ) : this( null, transitionTime ) { }
	public ColorAnimator( IUpdaterOnDemand updater, float transitionTime )
	{
		_updater = updater;
		var val = Mathf.Approximately( transitionTime, 0 ) ? Mathf.Infinity : ( 2 / ( transitionTime * transitionTime ) );

		_r = new FloatAnimator01( val, val, val );
		_g = new FloatAnimator01( val, val, val );
		_b = new FloatAnimator01( val, val, val );
		_a = new FloatAnimator01( val, val, val );

		Start();
	}

	public ColorAnimator( IUpdaterOnDemand updater, float acceleration, float deceleration, float maxSpeed, float alphaAcceleration, float alphaDeceleration, float alphaMaxSpeed )
	{
		_updater = updater;

		_r = new FloatAnimator01( acceleration, deceleration, maxSpeed );
		_g = new FloatAnimator01( acceleration, deceleration, maxSpeed );
		_b = new FloatAnimator01( acceleration, deceleration, maxSpeed );
		_a = new FloatAnimator01( alphaAcceleration, alphaDeceleration, alphaMaxSpeed );

		Start();
	}

	public void Start()
	{
		_r.Start( _cachedColor.r );
		_g.Start( _cachedColor.g );
		_b.Start( _cachedColor.b );
		_a.Start( _cachedColor.a );
		_colorUpdate.Trigger( _cachedColor );
	}

	public void Start( Color color )
	{
		_cachedColor = color;
		Start();
	}

	public void Reset(float transitionTime)
	{
		var val = Mathf.Approximately( transitionTime, 0 ) ? Mathf.Infinity : ( 2 / ( transitionTime * transitionTime ) );

		_r.Reset(val, val, val);
		_g.Reset(val, val, val);
		_b.Reset(val, val, val);
		_a.Reset(val, val, val);

	}

	public bool Update( float delta )
	{
		bool updated = false;
		bool updateMore = false;

		if( !_r.IsOnDesired.Value )
		{
			updated = true;
			_r.Update( delta );
			_cachedColor.r = _r.Value;
			updateMore |= !_r.IsOnDesired.Value;
		}

		if( !_g.IsOnDesired.Value )
		{
			updated = true;
			_g.Update( delta );
			_cachedColor.g = _g.Value;
			updateMore |= !_b.IsOnDesired.Value;
		}

		if( !_b.IsOnDesired.Value )
		{
			updated = true;
			_b.Update( delta );
			_cachedColor.b = _b.Value;
			updateMore |= !_g.IsOnDesired.Value;
		}

		if( !_a.IsOnDesired.Value )
		{
			updated = true;
			_a.Update( delta );
			_cachedColor.a = _a.Value;
			updateMore |= !_a.IsOnDesired.Value;
		}

		if( updated ) _colorUpdate.Trigger( _cachedColor );
		if( !updateMore )
		{
			var d = Time.time - test;
			_desiredReach.Trigger( _cachedColor );
		}

		return updateMore;
	}

	void IValueStateSetter<Color>.Setter( Color t ) { SetColor( t ); }
	public void SetColor( Color color, bool useAlpha = false )
	{
		_r.SetDesire( color.r );
		_g.SetDesire( color.g );
		_b.SetDesire( color.b );
		if( useAlpha ) _a.SetDesire( color.a );

		if( _updater != null && ( !_r.IsOnDesired.Value || !_g.IsOnDesired.Value || !_b.IsOnDesired.Value || ( useAlpha && !_a.IsOnDesired.Value ) ) ) _updater.RequestUpdate( this );
	}

	public void SetAlpha( float value )
	{
		_a.SetDesire( value );

		if( _updater != null && !_a.IsOnDesired.Value ) _updater.RequestUpdate( this );
	}

	public void SetAlphaMax( bool max = true ) { SetAlpha( max ? 1 : 0 ); }
	public void SetAlphaMin( bool min = true ) { SetAlpha( min ? 0 : 1 ); }
}
