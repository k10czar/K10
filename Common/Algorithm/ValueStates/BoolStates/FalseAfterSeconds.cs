using System.Collections;
using K10;
using UnityEngine;

public class FalseAfterSeconds : IBoolStateObserver
{
	bool _value = true;

	private EventSlot _onFalseState;
	private EventSlot<bool> _onChange;
	private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();

	public IBoolStateObserver Not => _not.Request( this );
	public IEventRegister OnTrueState => FakeEvent.Instance;
	public IEventRegister OnFalseState => _value ? Lazy.Request( ref _onFalseState ) : FakeEvent.Instance;
	public IEventRegister<bool> OnChange => _value ? Lazy.Request( ref _onChange ) : FakeEvent<bool>.Instance;

	public bool Value => _value;
	public bool Get() { return _value; }

	public FalseAfterSeconds( float defaultBubbleTime )
	{
		ExternalCoroutine.Play( DelayedExpiration( defaultBubbleTime ) );
	}

	private IEnumerator DelayedExpiration( float defaultBubbleTime )
	{
		yield return new WaitForSeconds( defaultBubbleTime );
		_value = false;
		_onFalseState?.Trigger();
		_onChange?.Trigger( true );
		_onFalseState = null;
		_onChange = null;
	}
}