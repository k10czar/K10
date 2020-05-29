

public interface IVoidable
{
	void Expire();
}


public class CallOnce : Voidable
{
    public CallOnce( IEventTrigger callback ) : base( callback ) { }
	public CallOnce( System.Action act ) : base( act ) { }
    public override void Trigger() { if( !IsValid ) return; Expire(); _callback.Trigger(); }
}


public class CallOnce<T> : Voidable<T>
{
    public CallOnce( IEventTrigger<T> callback ) : base( callback ) { }
	public CallOnce( System.Action<T> act ) : base( act ) { }
    public override void Trigger( T t ) { if( !IsValid ) return; Expire(); _callback.Trigger( t ); }
}

public class Voidable : IEventTrigger, IVoidable
{
    protected IEventTrigger _callback;
    bool _void;

    public Voidable( IEventTrigger callback ) { _callback = callback; }
	public Voidable( System.Action act ) { _callback = new ActionEventCapsule( act ); }
    public virtual void Trigger() { if( !IsValid ) return; _callback.Trigger(); }
    public bool IsValid { get { return !_void && _callback.IsValid; } }
    public void Expire() { _void = true; }
}

public class Voidable<T> : IEventTrigger<T>, IVoidable
{
	protected IEventTrigger<T> _callback;
	bool _void;

    public Voidable( IEventTrigger<T> callback ) { _callback = callback; }
	public Voidable( System.Action<T> act ) { _callback = new ActionEventCapsule<T>( act ); }
    public virtual void Trigger( T t ) { if( !IsValid ) return; _callback.Trigger( t ); }
    public bool IsValid { get { return !_void && _callback.IsValid; } }
    public void Expire() { _void = true; }
}