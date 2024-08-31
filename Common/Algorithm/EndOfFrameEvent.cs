using K10;
using UnityEngine;

public class EndOfFrameEvent : IEventRequest
{
    private bool _requested = false;
    private Coroutine _coroutine;
    private static readonly WaitForEndOfFrame COROUTINE_WAIT = new WaitForEndOfFrame();

    EventSlot _onEvent = new EventSlot();
    public IEventRegister OnEvent => _onEvent;

    public void Request()
    {
        if( _requested ) return;
        _requested = true;
        if( _coroutine == null ) _coroutine = ExternalCoroutine.Play( RequestCoroutine() );
    }

    System.Collections.IEnumerator RequestCoroutine()
    {
        yield return COROUTINE_WAIT;
        if( _requested )
        {
            _onEvent.Trigger();
            _requested = false;
        }
        _coroutine = null;
    }

    public void Kill()
    {
        _requested = false;
        if( _coroutine != null ) ExternalCoroutine.Stop( _coroutine );
        _onEvent?.Kill();
        _onEvent = null;
        _coroutine = null;
    }
}