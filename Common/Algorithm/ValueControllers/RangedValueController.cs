using UnityEngine;
using System.Collections.Generic;
using System;

public class PoliModifiedValue : ICustomDisposableKill
{
	private readonly INumericValueState<float> _modifiedValue;
	private IInterpolationFunction _interpolation;

	private readonly List<InterpolatedOverTimeModifier> _overTimeModifiers = new List<InterpolatedOverTimeModifier>();

	public IValueStateObserver<float> ModifiedValue => _modifiedValue;

	float _lastTime = 0;

	public PoliModifiedValue( INumericValueState<float> value = null, IInterpolationFunction interpolation = null )
	{
		_modifiedValue = value ?? new FloatState();
		_interpolation = interpolation ?? LinearInterpolation.Instance;
	}

	public void Kill()
	{
		(_modifiedValue as ICustomDisposableKill)?.Kill();
		_interpolation = null;
		_overTimeModifiers?.Clear();
	}

	public void Tick( float deltaTime )
	{
		for( int i = _overTimeModifiers.Count - 1; i >= 0; i-- )
		{
			var mod = _overTimeModifiers[i];
			mod.Tick( deltaTime, _modifiedValue );
			if( mod.IsOver )
			{
				_overTimeModifiers.RemoveAt( i );
				ObjectPool<InterpolatedOverTimeModifier>.Return( mod );
			}
		}
		_lastTime = Time.time;
	}

	public void Clear()
	{
		for( int i = _overTimeModifiers.Count - 1; i >= 0; i-- )
		{
			var mod = _overTimeModifiers[i];
			_overTimeModifiers.RemoveAt( i );
			ObjectPool<InterpolatedOverTimeModifier>.Return( mod );
		}
	}

	public void SetInitialValue( float value ) { _modifiedValue.Setter( value ); }

	public void SetValue( float value, float time = 0 )
	{
		float penddingModifications = 0;
		for( int i = _overTimeModifiers.Count - 1; i >= 0; i-- ) penddingModifications += _overTimeModifiers[i].LastingValue;
		var mod = value - ( _modifiedValue.Value + penddingModifications );
		Add( mod, time );
	}

	public void Add( IInterpolatedValueOverTime value ) { Add( value.Value, value.Seconds, value.Interpolation ); }

	public void Add( float value, float seconds = 0, IInterpolationFunction interpolation = null )
	{
		if( value < float.Epsilon && value > -float.Epsilon ) return;
		if( seconds < float.Epsilon ) _modifiedValue.Increment( value );
		else { _overTimeModifiers.Add( ObjectPool<InterpolatedOverTimeModifier>.Request().Reseted( value, seconds, interpolation ?? _interpolation, _lastTime - Time.time ) ); }
	}
}
