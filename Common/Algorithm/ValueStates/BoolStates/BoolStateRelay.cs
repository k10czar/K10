public class BoolStateRelay : IBoolStateObserver, ICustomDisposableKill
{
	bool _killed = false;
	IBoolStateObserver _currentSource;
	ConditionalEventsCollection _validator;
	private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();

	EventSlot<bool> _onChange;
	EventSlot _onTrueState;
	EventSlot _onFalseState;

	public bool Value { get { return _currentSource?.Value ?? false; } }
	public bool Get() { return _currentSource?.Value ?? false; }
	public IEventValidator Validator
	{
		get
		{
			if( _killed ) return NullValidator.Instance;
			if( _validator == null )
			{
				_validator = new ConditionalEventsCollection();
			}
			return _validator;
		}
	}
	public IEventRegister<bool> OnChange
	{ 
		get 
		{
			if( _killed ) return FakeEvent<bool>.Instance;
			if( _onChange == null )
			{
				_onChange = new EventSlot<bool>();
				if( _currentSource != null ) _currentSource.OnChange.Register( Validator.Validated<bool>( _onChange ) );
			}
			return _onChange; 
		}
	}
	public IEventRegister OnTrueState
	{
		get
		{
			if( _killed ) return FakeEvent.Instance;
			if( _onTrueState == null )
			{
				_onTrueState = new EventSlot();
				if( _currentSource != null ) _currentSource.OnTrueState.Register( Validator.Validated( _onTrueState ) );
			}
			return _onTrueState;
		}
	}
	public IEventRegister OnFalseState
	{
		get
		{
			if( _killed ) return FakeEvent.Instance;
			if( _onFalseState == null )
			{
				_onFalseState = new EventSlot();
				if( _currentSource != null ) _currentSource.OnFalseState.Register( Validator.Validated( _onFalseState ) );
			}
			return _onFalseState;
		}
	}
	public IBoolStateObserver Not => _not.Request( this );

	public void ChangeSource( IBoolStateObserver newSource )
	{
		if( newSource == _currentSource ) return;

		var oldValue = Value;
		_validator?.Void();

		_currentSource = newSource;
		var newValue = Value;
		if( oldValue != newValue )
		{
			_onChange?.Trigger( newValue );
			if( newValue ) _onTrueState?.Trigger();
			else _onFalseState?.Trigger();
		}

		if( _currentSource != null )
		{
			if( _onChange != null ) _currentSource.OnChange.Register( Validator.Validated<bool>( _onChange ) );
			if( _onTrueState != null ) _currentSource.OnTrueState.Register( Validator.Validated( _onTrueState ) );
			if( _onFalseState != null ) _currentSource.OnFalseState.Register( Validator.Validated( _onFalseState ) );
		}
	}

	public void Kill()
	{
		_killed = true;
		_validator?.Kill();
		_onChange?.Kill();
		_onTrueState?.Kill();
		_onFalseState?.Kill();
	}

	public override string ToString() { return string.Format( "BSR({0})", _currentSource.ToStringOrNull() ); }
}
