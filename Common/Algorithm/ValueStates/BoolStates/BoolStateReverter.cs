public struct LazyBoolStateReverterHolder
{
	private bool _killed;
	private BoolStateReverter _not;

	public LazyBoolStateReverterHolder( BoolStateReverter not )
	{
		_killed = false;
		_not = not;
	}

	public void Kill()
	{
		_not?.Kill();
		_not = null;
	}

	public IBoolStateObserver Request( IBoolStateObserver originalState )
	{
		if( _killed ) return ( _not?.Value ?? originalState.Value ) ? FalseState.Instance : TrueState.Instance;
		if( _not == null ) _not = new BoolStateReverter( originalState );
		return _not;
	}
}

public class BoolStateReverter : IBoolStateObserver, ICustomDisposableKill
{
	bool _killed;
	IBoolStateObserver _original;
	EventSlot<bool> _onChange;

	public bool Value => !_original.Value;
	public bool Get() => !_original.Value;
	public IEventRegister<bool> OnChange
	{
		get
		{
			if( _killed ) return FakeEvent<bool>.Instance;
			if( _onChange == null )
			{
				_onChange = new EventSlot<bool>();
				_original.OnChange.Register( EventReverter );
			}
			return _onChange;
		}
	}

	public IEventRegister OnTrueState => _original.OnFalseState;
	public IEventRegister OnFalseState => _original.OnTrueState;
	public IBoolStateObserver Not => _original;

	public BoolStateReverter( IBoolStateObserver parent )
	{
		_original = parent;
	}

	public void Kill()
	{
		_killed = true;
		_onChange?.Kill();
		_onChange = null;
	}

	void EventReverter( bool value ) { _onChange?.Trigger( !value ); }

	public override string ToString() { return $"( {Value} => !{_original.ToStringOrNull()} )"; }
}
