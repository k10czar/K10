

using System;
using K10;

public interface IVoidable
{
	bool IsValid { get; }
	void Void();
	IEventRegister OnVoid { get; }
}

public static class VoidableExtensions
{
	public static void VoidWhenTrue(this IVoidable voidable, System.Func<bool> validationQuery)
	{
		ExternalCoroutine.StartCoroutine(UntilCoroutine(voidable, () => !validationQuery()));
	}

	public static void VoidWhenFalse(this IVoidable voidable, System.Func<bool> validationQuery)
	{
		ExternalCoroutine.StartCoroutine(UntilCoroutine(voidable, validationQuery));
	}

	private static System.Collections.IEnumerator UntilCoroutine(IVoidable voidable, System.Func<bool> validationQuery)
	{
		while (voidable.IsValid && validationQuery()) yield return null;
		voidable.Void();
	}
}

public class CallEventOnce : IEventTrigger
{
	IEventTrigger _eventToCall;

	bool _preVoided;
	public bool IsValid => !_preVoided && _eventToCall != null && _eventToCall.IsValid;
	
	public CallEventOnce(IEventTrigger callback) { _eventToCall = callback; _preVoided = false; }
    public void Trigger() { if( !IsValid ) return; _preVoided = true; _eventToCall.Trigger(); _eventToCall = null; }
}

[UnityEngine.HideInInspector]
public class CallOnce : IEventTrigger
{
	Action _actionToCall;

	bool _preVoided;
	public bool IsValid => !_preVoided && _actionToCall != null;

	public CallOnce( Action act ) { _actionToCall = act; _preVoided = false; }

    public void Trigger() { if( !IsValid ) return; _preVoided = true; _actionToCall(); _actionToCall = null; }
	public void Void() { _preVoided = true; _actionToCall = null; }
}

[UnityEngine.HideInInspector]
public class VoidableCallOnce : IEventTrigger, IVoidable
{
	Action _actionToCall;

	bool _preVoided;
	public bool IsValid => !_preVoided && _actionToCall != null;
	
	private EventSlot _onVoid;
	public IEventRegister OnVoid => _onVoid ??= new EventSlot();

    public VoidableCallOnce( Action act ) { _actionToCall = act; _preVoided = false; }

    public void Trigger() { if( !IsValid ) return; _preVoided = true; _actionToCall(); _actionToCall = null; }

    public void Void()
    {
        if( _actionToCall == null ) return; _onVoid?.Trigger(); _actionToCall = null; _onVoid?.Kill();
    }
}

[UnityEngine.HideInInspector]
public class CallOnceValidated : IEventTrigger
{
	System.Action _actionToCall;
	IEventValidator _validator;

	bool _preVoided;
	public bool IsValid => !_preVoided && _actionToCall != null && _validator != null && _validator.CurrentValidationCheck();

	public CallOnceValidated( IEventValidator validator, System.Action act ) { _actionToCall = act; _validator = validator; _preVoided = false; }

    public void Trigger() { if( !IsValid ) return; _preVoided = true; _actionToCall(); _actionToCall = null; _validator = null; }
}

[UnityEngine.HideInInspector]
public class CallOnce<T> : IEventTrigger<T>, IVoidable
{
	Voidable<T> _voidable;

	bool _preVoided;
	public bool IsValid => !_preVoided && _voidable.IsValid;

    public CallOnce( IEventTrigger<T> callback ) { _voidable = new Voidable<T>( callback ); _preVoided = false; }
	public CallOnce( System.Action<T> act ) { _voidable = new Voidable<T>( act ); _preVoided = false; }
	public CallOnce( IEventValidator validator, IEventTrigger<T> callback ) { _voidable = new Voidable<T>( validator.Validated( callback ) ); _preVoided = false; }
	public CallOnce( IEventValidator validator, System.Action<T> act ) { _voidable = new Voidable<T>( validator.Validated( act ) ); _preVoided = false; }

    public void Trigger( T t ) { if( !IsValid ) return; _preVoided = true; _voidable.Trigger( t ); _voidable.Void(); }

	public IEventRegister OnVoid => _voidable.OnVoid;
	public void Void() { _voidable.Void(); }
}

[UnityEngine.HideInInspector]
public class Voidable : IEventTrigger, IVoidable
{
    private IEventTrigger _callback;

	private EventSlot _onVoid;
	public IEventRegister OnVoid => _onVoid ??= new EventSlot();

	public Voidable( IEventTrigger callback ) { _callback = callback; _onVoid = null; }
	public Voidable( System.Action act ) { _callback = new ActionEventCapsule( act ); _onVoid = null; }

    public void Trigger() { if( !IsValid ) return; _callback.Trigger(); }
    public bool IsValid => _callback?.IsValid ?? false;
    public void Void() { if( _callback == null ) return; _onVoid?.Trigger(); _callback = null; _onVoid?.Kill(); }
}

[UnityEngine.HideInInspector]
public class Voidable<T> : IEventTrigger<T>, IVoidable
{
	private IEventTrigger<T> _callback;

	private EventSlot _onVoid;
	public IEventRegister OnVoid => _onVoid ??= new EventSlot();

    public Voidable( IEventTrigger<T> callback ) { _callback = callback; _onVoid = null; }
	public Voidable( System.Action<T> act ) { _callback = new ActionEventCapsule<T>( act ); _onVoid = null; }

    public void Trigger( T t ) { if( !IsValid ) return; _callback.Trigger( t ); }
    public bool IsValid => _callback?.IsValid ?? false;
	public void Void() { if( _callback == null ) return; _onVoid?.Trigger(); _callback = null; _onVoid?.Kill(); }
}