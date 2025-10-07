using System;
using UnityEngine;
using System.Runtime.CompilerServices;

[System.Serializable]
public class ColorAnimator : IValueState<Color>, IUpdatableOnDemand
{
	bool _killed = false;

	const float DEFAULT_ACCELERATION = 1;
	const float DEFAULT_DEACCELERATION = 1;
	const float DEFAULT_MAX_SPEED = 1;

	IUpdaterOnDemand _updater;

	FastFloatAnimator _r;
	FastFloatAnimator _g;
	FastFloatAnimator _b;
	FastFloatAnimator _a;

	Color _cachedColor = Color.white;
	float _transitionTime = 0;
	bool _dirty = false;

	EventSlot<Color> _desiredReach;
	public IEventRegister<Color> OnDesiredReach => Lazy.Request( ref _desiredReach );

	public Color Value { get { return _cachedColor; } }
	public Color Get() { return _cachedColor; }
	public Color GetColor() { return _cachedColor; }

	public float CurrentDuration => _transitionTime - (_transitionTime * _r._currentValue);
	EventSlot<Color> _colorUpdate;
	public IEventRegister<Color> OnChange  => Lazy.Request( ref _colorUpdate );

	public Color Desired { get { return new Color( _r._desiredValue, _g._desiredValue, _b._desiredValue, _a._desiredValue ); } }

	public ColorAnimator() : this( null ) { }
	public ColorAnimator( float acceleration, float deceleration, float maxSpeed ) : this( null, acceleration, deceleration, maxSpeed ) { }
	public ColorAnimator( IUpdaterOnDemand updater ) : this( updater, DEFAULT_ACCELERATION, DEFAULT_DEACCELERATION, DEFAULT_MAX_SPEED ) { }
	public ColorAnimator( IUpdaterOnDemand updater, float acceleration, float deceleration, float maxSpeed )
	{
		_updater = updater;

		_r.Rebuild01( _cachedColor.r, acceleration, deceleration, maxSpeed );
		_g.Rebuild01( _cachedColor.g, acceleration, deceleration, maxSpeed );
		_b.Rebuild01( _cachedColor.b, acceleration, deceleration, maxSpeed );
		_a.Rebuild01( _cachedColor.a, acceleration, deceleration, maxSpeed );
	}

	public ColorAnimator( float transitionTime ) : this( null, transitionTime ) { }
	public ColorAnimator( IUpdaterOnDemand updater, float transitionTime )
	{
		_transitionTime = transitionTime;
		_updater = updater;
		var val = MathAdapter.Approximately( transitionTime, 0 ) ? Mathf.Infinity : ( 2 / ( transitionTime * transitionTime ) );

		_r.Rebuild01( _cachedColor.r, val, val, float.MaxValue );
		_g.Rebuild01( _cachedColor.g, val, val, float.MaxValue );
		_b.Rebuild01( _cachedColor.b, val, val, float.MaxValue );
		_a.Rebuild01( _cachedColor.a, val, val, float.MaxValue );
	}

	public ColorAnimator( IUpdaterOnDemand updater, float acceleration, float deceleration, float maxSpeed, float alphaAcceleration, float alphaDeceleration, float alphaMaxSpeed )
	{
		_updater = updater;

		_r.Rebuild01( _cachedColor.r, acceleration, deceleration, maxSpeed );
		_g.Rebuild01( _cachedColor.g, acceleration, deceleration, maxSpeed );
		_b.Rebuild01( _cachedColor.b, acceleration, deceleration, maxSpeed );
		_a.Rebuild01( _cachedColor.a, alphaAcceleration, alphaDeceleration, alphaMaxSpeed );
	}

	public void Start()
	{
		_r.Reset( _cachedColor.r );
		_g.Reset( _cachedColor.g );
		_b.Reset( _cachedColor.b );
		_a.Reset( _cachedColor.a );
		_colorUpdate?.Trigger( _cachedColor );
	}

	public void Start( Color color )
	{
		_cachedColor = color;
		Start();
	}

	public void Reset(float transitionTime)
	{
		_transitionTime = transitionTime;
		var val = MathAdapter.Approximately( transitionTime, 0 ) ? Mathf.Infinity : ( 2 / ( transitionTime * transitionTime ) );

		_r.Rebuild01(0, val, val, float.MaxValue);
		_g.Rebuild01(0, val, val, float.MaxValue);
		_b.Rebuild01(0, val, val, float.MaxValue);
		_a.Rebuild01(0, val, val, float.MaxValue);
	}

	public bool Update( float delta )
	{
		if( !_dirty || _killed ) return false;

		bool updated = false;
		_dirty = false;
		bool changed = false;

		if( !_r.IsOnDesired() )
		{
			updated = true;
			_r.Update( delta );
			_cachedColor.r = _r._currentValue;
			_dirty |= !_r.IsOnDesired();
			changed = true;
		}

		if( !_g.IsOnDesired() )
		{
			updated = true;
			_g.Update( delta );
			_cachedColor.g = _g._currentValue;
			_dirty |= !_b.IsOnDesired();
			changed = true;
		}

		if( !_b.IsOnDesired() )
		{
			updated = true;
			_b.Update( delta );
			_cachedColor.b = _b._currentValue;
			_dirty |= !_g.IsOnDesired();
			changed = true;
		}

		if( !_a.IsOnDesired() )
		{
			updated = true;
			_a.Update( delta );
			_cachedColor.a = _a._currentValue;
			_dirty |= !_a.IsOnDesired();
			changed = true;
		}

		if( updated ) _colorUpdate?.Trigger( _cachedColor );
		if( !_dirty && changed ) _desiredReach?.Trigger( _cachedColor );

		return _dirty;
	}

	void IValueStateSetter<Color>.Setter( Color t ) { SetColor( t ); }
	public void SetColor( Color color, bool useAlpha = false )
	{
		_r.SetDesire( color.r );
		_g.SetDesire( color.g );
		_b.SetDesire( color.b );
		if( useAlpha ) _a.SetDesire( color.a );

		bool needsUpdate = !_r.IsOnDesired() || !_g.IsOnDesired() || !_b.IsOnDesired() || !_a.IsOnDesired();
		_dirty |= needsUpdate;

		if( _updater != null && needsUpdate ) _updater.RequestUpdate( this );
	}

	public void SetAlpha( float value )
	{
		_a.SetDesire( value );

		bool needsUpdate = !_a.IsOnDesired();
		_dirty |= needsUpdate;

		if( _updater != null && needsUpdate ) _updater.RequestUpdate( this );
	}

	public void SetAlphaMax( bool max = true ) { SetAlpha( max ? 1 : 0 ); }
	public void SetAlphaMin( bool min = true ) { SetAlpha( min ? 0 : 1 ); }

	public void GoToEnd()
	{
		_r.ForceToDesired();
		_g.ForceToDesired();
		_b.ForceToDesired();
		_a.ForceToDesired();
	}

	public void Kill()
	{
		_killed = true;
		_desiredReach?.Kill();
		_colorUpdate?.Kill();
		_updater = null;
	}
}
