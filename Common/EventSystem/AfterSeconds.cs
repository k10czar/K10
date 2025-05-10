using System.Collections;

[UnityEngine.HideInInspector]
public class AfterSeconds : IEventTrigger, IVoidable
{
	Voidable _voidable;

	bool _preVoided;
	float _seconds;
	public bool IsValid => !_preVoided && _voidable.IsValid;

	public AfterSeconds( float seconds, IEventTrigger callback ) { _seconds = seconds; _voidable = new Voidable( callback ); _preVoided = false; }
	public AfterSeconds( float seconds, System.Action act ) { _seconds = seconds; _voidable = new Voidable( act ); _preVoided = false; }
	public AfterSeconds( float seconds, IEventValidator validator, IEventTrigger callback ) { _seconds = seconds; _voidable = new Voidable( validator.Validated( callback ) ); _preVoided = false; }
	public AfterSeconds( float seconds, IEventValidator validator, System.Action act ) { _seconds = seconds; _voidable = new Voidable( validator.Validated( act ) ); _preVoided = false; }

    public void Trigger() { if( !IsValid ) return; _preVoided = true; ExternalCoroutine.StartCoroutine( DelayedTrigger() ); }

	IEnumerator DelayedTrigger()
	{
		yield return new UnityEngine.WaitForSeconds( _seconds );
		_voidable.Trigger(); _voidable.Void();
	}

	public IEventRegister OnVoid => _voidable.OnVoid;
	public void Void() { _voidable.Void(); }
}
