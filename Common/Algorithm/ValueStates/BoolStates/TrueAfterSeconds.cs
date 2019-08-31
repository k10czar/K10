using System.Collections;
using UnityEngine;

public class TrueAfterSeconds : IBoolStateObserver
{
    private readonly BoolState _isValid = new BoolState( false );

    public IEventRegister OnTrueState => _isValid.OnTrueState;
    public IEventRegister OnFalseState => _isValid.OnFalseState;
    public bool Value => _isValid.Value;
    public IEventRegister<bool> OnChange => _isValid.OnChange;
    public bool Get() { return _isValid.Get(); }

    public TrueAfterSeconds( float defaultBubbleTime )
    {
        ExternalCoroutine.StartCoroutine( DelayedExpiration( defaultBubbleTime ) );
    }

    private IEnumerator DelayedExpiration( float defaultBubbleTime )
    {
        yield return new WaitForSeconds( defaultBubbleTime );
        _isValid.SetTrue();
    }
}