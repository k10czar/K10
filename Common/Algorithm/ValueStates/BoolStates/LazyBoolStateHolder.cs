using UnityEngine;

[System.Serializable]
public struct LazyBoolStateHolder
{
	private bool _killed;
	[SerializeField] bool _value;
	private BoolState _state;

	public LazyBoolStateHolder( bool value )
	{
		_killed = false;
		_value = value;
		_state = null;
	}

	public bool Value => _state?.Value ?? _value;
	
	// public IEventRegister OnTrueState => TryRequestState()?.OnTrueState;
	// public IEventRegister OnFalseState => TryRequestState()?.OnFalseState;
	// public IEventRegister<bool> OnChange => TryRequestState()?.OnChange;

	public IBoolStateObserver Observer => TryRequestState();

	public void SetTrue() => Setter( true );
	public void SetFalse() => Setter( false );

	private BoolState TryRequestState()
	{
		if( _killed || _state != null ) return _state;
		_state = new BoolState( _value );
		return _state;
	}

	public bool Get() => Value;

	public void Kill()
	{
		_killed = true;
		_state?.Kill();
		_state = null;
	}

	public void Setter( bool t )
	{
		if( _state != null ) _state.Value = t;
		_value = t;
	}
}
