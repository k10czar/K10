


public class CallOnce : Voidable
{
    public CallOnce( IEventTrigger callback ) : base( callback ) { }
	public CallOnce( System.Action act ) : base( act ) { }
    public override void Trigger() { base.Trigger(); Expire(); }
}


public class CallOnce<T> : Voidable<T>
{
    public CallOnce( IEventTrigger<T> callback ) : base( callback ) { }
	public CallOnce( System.Action<T> act ) : base( act ) { }
    public override void Trigger( T t ) { base.Trigger( t ); Expire(); }
}

public class Voidable : IEventTrigger
{
    IEventTrigger _callback;
    bool _void;

    public Voidable( IEventTrigger callback ) { _callback = callback; }
	public Voidable( System.Action act ) { _callback = new ActionEventCapsule( act ); }
    public virtual void Trigger() { _callback.Trigger(); }
    public bool IsValid { get { return !_void && _callback.IsValid; } }
    public void Expire() { _void = true; }
}

public class Voidable<T> : IEventTrigger<T>
{
    IEventTrigger<T> _callback;
	bool _void;

    public Voidable( IEventTrigger<T> callback ) { _callback = callback; }
	public Voidable( System.Action<T> act ) { _callback = new ActionEventCapsule<T>( act ); }
    public virtual void Trigger( T t ) { _callback.Trigger( t ); }
    public bool IsValid { get { return !_void && _callback.IsValid; } }
    public void Expire() { _void = true; }
}