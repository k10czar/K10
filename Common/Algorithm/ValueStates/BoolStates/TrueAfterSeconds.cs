using System.Collections;
using UnityEngine;

public class TrueAfterSeconds : IBoolStateObserver
{
	bool _value = false;

	private EventSlot _onTrueState;
	private EventSlot<bool> _onChange;
	private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();

	public IBoolStateObserver Not => _not.Request( this );
	public IEventRegister OnTrueState => _value ? FakeEvent.Instance : Lazy.Request( ref _onTrueState );
    public IEventRegister OnFalseState => FakeEvent.Instance;
    public IEventRegister<bool> OnChange => _value ? FakeEvent<bool>.Instance : Lazy.Request( ref _onChange );

    public bool Value => _value;
    public bool Get() { return _value; }

    public TrueAfterSeconds( float defaultBubbleTime )
    {
        ExternalCoroutine.StartCoroutine( DelayedExpiration( defaultBubbleTime ) );
    }

    private IEnumerator DelayedExpiration( float defaultBubbleTime )
    {
        yield return new WaitForSeconds( defaultBubbleTime );
		_value = true;
		_onTrueState?.Trigger();
		_onChange?.Trigger( true );
		_onTrueState = null;
		_onChange = null;
	}
}