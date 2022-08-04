public class BoolStateRelay : IBoolStateObserver, ICustomDisposableKill
{
	IBoolStateObserver _currentSource;
	readonly ConditionalEventsCollection _events = new ConditionalEventsCollection();

	EventSlot<bool> _onChange = new EventSlot<bool>();
	EventSlot _onTrueState = new EventSlot();
	EventSlot _onFalseState = new EventSlot();

	public bool Value { get { return _currentSource != null && _currentSource.Value; } }
	public bool Get() { return _currentSource != null && _currentSource.Value; }
	public IEventRegister<bool> OnChange { get { return _onChange; } }
	public IEventRegister OnTrueState { get { return _onTrueState; } }
	public IEventRegister OnFalseState { get { return _onFalseState; } }

	public void ChangeSource( IBoolStateObserver newSource )
	{
		if( newSource == _currentSource ) return;

		var oldValue = Value;
		_events.Void();

		_currentSource = newSource;
		var newValue = Value;
		if( oldValue != newValue )
		{
			_onChange.Trigger( newValue );
			if( newValue ) _onTrueState.Trigger();
			else _onFalseState.Trigger();
		}

		if( _currentSource != null )
		{
			_currentSource.OnChange.Register( _events.Validated<bool>( _onChange ) );
			_currentSource.OnTrueState.Register( _events.Validated( _onTrueState ) );
			_currentSource.OnFalseState.Register( _events.Validated( _onFalseState ) );
		}
	}

	public void Kill()
	{
		_events.Kill();
		_onChange.Kill();
		_onTrueState.Kill();
		_onFalseState.Kill();
	}

	public override string ToString() { return string.Format( "BSR({0})", _currentSource.ToStringOrNull() ); }
}
