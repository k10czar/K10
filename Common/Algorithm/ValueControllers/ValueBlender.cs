using System.Collections.Generic;


public interface IBlendable<T>
{
	T  BlendWith( T other, float blendAmount );
}

public interface IOverridableOf<T>
{
    void RemoveOverride( object key );
    void SetOverride( object key, T value );
}

public class ValueBlender<T> : IOverridableOf<T>, IUpdatable where T : IBlendable<T>
{
	T _defaultValue;
	T _currentValue;
	T _desiredValue;
	T _blendStartValue;

	FastFloatAnimator _blend = new FastFloatAnimator( 0, 10, 10, 10, 0, 1 );

	Dictionary<object, T> _overrides = new Dictionary<object, T>();
	List<object> _overrideStack = new List<object>();
    private bool _changed;

    public T CurrentValue => _currentValue;
    public bool Changed => _changed;

    public void Start( T force )
	{
		_defaultValue = force;
		_currentValue = force;
		_desiredValue = force;
		_blendStartValue = force;
		_blend.Reset( 0 );
	}

    public void RemoveOverride( object key )
    {
        _overrideStack.Remove(key);
        BlendNewDesiredValue();
    }

    private void BlendNewDesiredValue()
    {
        _blendStartValue = _currentValue;
        if (_overrideStack.Count > 0) _overrides.TryGetValue(_overrideStack[_overrideStack.Count - 1], out _desiredValue);
        else _desiredValue = _defaultValue;
        _blend.Reset(0);
		_blend.SetDesire( 1 );
    }

    public void SetOverride( object key, T value )
    {
		_overrideStack.Remove( key );
		_overrideStack.Add( key );
		_overrides[key] = value;
		BlendNewDesiredValue();
    }

	public void Update( float deltaTime )
	{
		_changed = false;
		if( _blend.IsOnDesired() ) return;
		_changed = true;
		_blend.Update( deltaTime );
		if( _blend.IsOnDesired() ) 
		{
			_currentValue = _desiredValue;
			return;
		}
		var blendValue = _blend._currentValue;
		_currentValue = _blendStartValue.BlendWith( _desiredValue, blendValue );
	}
}
