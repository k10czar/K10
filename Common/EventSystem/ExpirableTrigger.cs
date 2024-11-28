


public interface IExpirableTrigger { void Expire(); }

public abstract class BaseExpirableEvent : IExpirableTrigger
{
	bool _valid = true;

	public bool IsValid { get { return _valid; } }
	public void Expire() { _valid = false; }
}

[UnityEngine.HideInInspector]
public class ExpirableTrigger : BaseExpirableEvent, IEventTrigger, IExpirableTrigger
{
	System.Action _callback;

	public ExpirableTrigger( System.Action callback ) { _callback = callback; }
	public void Trigger() { _callback(); }
}

[UnityEngine.HideInInspector]
public class ExpirableTrigger<T> : BaseExpirableEvent, IEventTrigger<T>, IExpirableTrigger
{
	System.Action<T> _callback;

	public ExpirableTrigger( System.Action<T> callback ) { _callback = callback; }
	public void Trigger( T t ) { _callback( t ); }
}

[UnityEngine.HideInInspector]
public class ExpirableTrigger<T,K> : BaseExpirableEvent, IEventTrigger<T,K>, IExpirableTrigger
{
	System.Action<T, K> _callback;
	bool _valid = true;

	public ExpirableTrigger( System.Action<T,K> callback ) { _callback = callback; }
	public void Trigger( T t, K k ) { if( _valid ) _callback( t, k ); }
}