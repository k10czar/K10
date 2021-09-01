

public interface IVoidable
{
	bool IsValid { get; }
	void Void();
	IEventRegister OnVoid { get; }
}

public static class VoidableExtensions
{
	public static void VoidWhenTrue( this IVoidable voidable, System.Func<bool> validationQuery )
	{
		ExternalCoroutine.StartCoroutine( UntilCoroutine( voidable, () => !validationQuery() ) );
	}

	public static void VoidWhenFalse( this IVoidable voidable, System.Func<bool> validationQuery )
	{
		ExternalCoroutine.StartCoroutine( UntilCoroutine( voidable, validationQuery ) );
	}

	private static System.Collections.IEnumerator UntilCoroutine( IVoidable voidable, System.Func<bool> validationQuery )
	{
		while( voidable.IsValid && validationQuery() ) yield return null;
		voidable.Void();
	}
}


public class ValidatedCallOnce : Voidable
{
	private readonly System.Func<bool> _validation;
	public override bool IsValid { get { return base.IsValid && _validation(); } }
	public ValidatedCallOnce( System.Func<bool> validation, IEventTrigger callback ) : base( callback ) { _validation = validation; }
	public ValidatedCallOnce( System.Func<bool> validation, System.Action act ) : base( act ) { _validation = validation; }
	public ValidatedCallOnce( IEventValidator validator, IEventTrigger callback ) : this( validator.CurrentValidationCheck, callback ) { }
	public ValidatedCallOnce( IEventValidator validator, System.Action act ) : this( validator.CurrentValidationCheck, act ) { }
	public override void Trigger() { if( !IsValid ) return; Void(); _callback.Trigger(); }
}


public class CallOnce : Voidable
{
    public CallOnce( IEventTrigger callback ) : base( callback ) { }
	public CallOnce( System.Action act ) : base( act ) { }
    public override void Trigger() { if( !IsValid ) return; Void(); _callback.Trigger(); }
}


public class CallOnce<T> : Voidable<T>
{
    public CallOnce( IEventTrigger<T> callback ) : base( callback ) { }
	public CallOnce( System.Action<T> act ) : base( act ) { }
    public override void Trigger( T t ) { if( !IsValid ) return; Void(); _callback.Trigger( t ); }
}

public class Voidable : IEventTrigger, IVoidable
{
    protected IEventTrigger _callback;
    bool _void;

	EventSlot _onVoid;
	public IEventRegister OnVoid => _onVoid ??= new EventSlot();

	public Voidable( IEventTrigger callback ) { _callback = callback; }
	public Voidable( System.Action act ) { _callback = new ActionEventCapsule( act ); }
    public virtual void Trigger() { if( !IsValid ) return; _callback.Trigger(); }
    public virtual bool IsValid { get { return !_void && _callback.IsValid; } }
    public void Void() { if( _void ) return; _void = true; _onVoid?.Trigger(); }
}

public class Voidable<T> : IEventTrigger<T>, IVoidable
{
	protected IEventTrigger<T> _callback;
	bool _void;

	EventSlot _onVoid;
	public IEventRegister OnVoid => _onVoid ??= new EventSlot();

    public Voidable( IEventTrigger<T> callback ) { _callback = callback; }
	public Voidable( System.Action<T> act ) { _callback = new ActionEventCapsule<T>( act ); }
    public virtual void Trigger( T t ) { if( !IsValid ) return; _callback.Trigger( t ); }
    public bool IsValid { get { return !_void && _callback.IsValid; } }
	public void Void() { if( _void ) return; _void = true; _onVoid?.Trigger(); }
}