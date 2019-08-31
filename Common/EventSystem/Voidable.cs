


public class CallOnce : Voidable
{
    public CallOnce( System.Action callback ) : base( callback ) { }
    public override void Trigger() { base.Trigger(); Expire(); }
}


public class CallOnce<T> : Voidable<T>
{
    public CallOnce( System.Action<T> callback ) : base( callback ) { }
    public override void Trigger( T t ) { base.Trigger( t ); Expire(); }
}

public class Voidable : IEventTrigger
{
    System.Action m_callback;
    bool _void;

    public Voidable( System.Action callback ) { m_callback = callback; }
    public virtual void Trigger() { m_callback(); }
    public bool IsValid { get { return !_void; } }
    public void Expire() { _void = true; }
}

public class Voidable<T> : IEventTrigger<T>
{
    System.Action<T> m_callback;
	bool _void;

    public Voidable( System.Action<T> callback ) { m_callback = callback; }
    public virtual void Trigger( T t ) { m_callback( t ); }
    public bool IsValid { get { return !_void; } }
    public void Expire() { _void = true; }
}