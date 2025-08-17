

[UnityEngine.HideInInspector]
public abstract class BaseConditionalEventListener : ICustomDisposableKill
{
	protected System.Func<bool> _condition;

	public BaseConditionalEventListener( System.Func<bool> condition ) { _condition = condition; }
	public virtual bool IsValid => _condition != null && _condition();

	public virtual void Kill() { _condition = null; }
}

[UnityEngine.HideInInspector]
public sealed class ConditionalEventListener : BaseConditionalEventListener, IEventTrigger
{
	EventTriggerCapsule _evnt;

	public override bool IsValid => base.IsValid && _evnt != null && _evnt.IsValid;

	private ConditionalEventListener( EventTriggerCapsule evnt, System.Func<bool> condition ) : base( condition ) { _evnt = evnt; }
	public ConditionalEventListener( IEventTrigger evnt, System.Func<bool> condition ) : this( new EventTriggerCapsule( evnt ), condition ) { }
	public ConditionalEventListener( System.Action action, System.Func<bool> condition ) : this( new EventTriggerCapsule( action ), condition ) { }

	public override void Kill() { base.Kill(); _evnt?.Void(); }

	public void Trigger() { _evnt.Trigger(); }

	public override bool Equals( object obj )
	{
		if( obj is ConditionalEventListener && _evnt != null && _condition != null )
		{
			var del = (ConditionalEventListener)obj;
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() { return _evnt.GetHashCode() + _condition.GetHashCode(); }
}

[UnityEngine.HideInInspector]
public sealed class ConditionalEventListener<T> : BaseConditionalEventListener, IEventTrigger<T>
{
	EventTriggerCapsule<T> _evnt;

	public override bool IsValid => base.IsValid && _evnt != null && _evnt.IsValid;

	private ConditionalEventListener( EventTriggerCapsule<T> evnt, System.Func<bool> condition ) : base( condition ) { _evnt = evnt; }
	public ConditionalEventListener( IEventTrigger<T> evnt, System.Func<bool> condition ) : this( new EventTriggerCapsule<T>( evnt ), condition ) { }
	public ConditionalEventListener( System.Action<T> action, System.Func<bool> condition ) : this( new EventTriggerCapsule<T>( action ), condition ) { }

	public override void Kill() { base.Kill(); _evnt?.Void(); }

	public void Trigger( T t ) { _evnt.Trigger( t ); }

	public override bool Equals( object obj )
	{
		if( obj is ConditionalEventListener<T> && _evnt != null && _condition != null )
		{
			var del = (ConditionalEventListener<T>)obj;
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() { return _evnt.GetHashCode() + _condition.GetHashCode(); }
}

[UnityEngine.HideInInspector]
public sealed class ConditionalEventListener<T, K> : BaseConditionalEventListener, IEventTrigger<T, K>
{
	EventTriggerCapsule<T, K> _evnt;

	public override bool IsValid => base.IsValid && _evnt != null && _evnt.IsValid;

	private ConditionalEventListener( EventTriggerCapsule<T, K> evnt, System.Func<bool> condition ) : base( condition ) { _evnt = evnt; }
	public ConditionalEventListener( IEventTrigger<T, K> evnt, System.Func<bool> condition ) : this( new EventTriggerCapsule<T, K>( evnt ), condition ) {}
	public ConditionalEventListener( System.Action<T, K> action, System.Func<bool> condition ) : this( new EventTriggerCapsule<T, K>( action ), condition ) { }

	public override void Kill() { base.Kill(); _evnt?.Void(); }

	public void Trigger( T t, K k ) { _evnt.Trigger( t, k ); }

	public override bool Equals( object obj )
	{
		if( obj is ConditionalEventListener<T, K> && _evnt != null && _condition != null )
		{
			var del = (ConditionalEventListener<T, K>)obj;
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() { return _evnt.GetHashCode() + _condition.GetHashCode(); }
}

[UnityEngine.HideInInspector]
public sealed class ConditionalEventListener<T, K, J> : BaseConditionalEventListener, IEventTrigger<T, K, J>
{
	EventTriggerCapsule<T, K, J> _evnt;

	public override bool IsValid => base.IsValid && _evnt != null && _evnt.IsValid;

	private ConditionalEventListener( EventTriggerCapsule<T, K, J> evnt, System.Func<bool> condition ) : base( condition ) { _evnt = evnt; }
	public ConditionalEventListener( IEventTrigger<T, K, J> evnt, System.Func<bool> condition ) : this( new EventTriggerCapsule<T, K, J>( evnt ), condition ) { }
	public ConditionalEventListener( System.Action<T, K, J> action, System.Func<bool> condition ) : this( new EventTriggerCapsule<T, K, J>( action ), condition ) { }

	public override void Kill() { base.Kill(); _evnt?.Void(); }

	public void Trigger( T t, K k, J j ) { _evnt.Trigger( t, k, j ); }

	public override bool Equals( object obj )
	{
		if( obj is ConditionalEventListener<T, K, J> && _evnt != null && _condition != null )
		{
			var del = (ConditionalEventListener<T, K, J>)obj;
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() { return _evnt.GetHashCode() + _condition.GetHashCode(); }
}