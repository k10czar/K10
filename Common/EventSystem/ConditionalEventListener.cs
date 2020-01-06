

public abstract class BaseConditionalEventListener
{
	protected System.Func<bool> _condition;

	public BaseConditionalEventListener( System.Func<bool> condition ) { _condition = condition; }
	public virtual bool IsValid { get { return _condition != null && _condition(); } }
}

public sealed class ConditionalEventListener : BaseConditionalEventListener, IEventTrigger
{
	IEventTrigger _evnt;

	public override bool IsValid => base.IsValid && _evnt != null && _evnt.IsValid;

	public ConditionalEventListener( IEventTrigger evnt, System.Func<bool> condition ) : base( condition ) { _evnt = evnt; }
	public ConditionalEventListener( System.Action action, System.Func<bool> condition ) : this( new ActionEventCapsule( action ), condition ) { }
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

public sealed class ConditionalEventListener<T> : BaseConditionalEventListener, IEventTrigger<T>
{
	IEventTrigger<T> _evnt;

	public override bool IsValid => base.IsValid && _evnt != null && _evnt.IsValid;

	public ConditionalEventListener( IEventTrigger<T> evnt, System.Func<bool> condition ) : base( condition ) { _evnt = evnt; }
	public ConditionalEventListener( System.Action<T> action, System.Func<bool> condition ) : this( new ActionEventCapsule<T>( action ), condition ) { }

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

public sealed class ConditionalEventListener<T, K> : BaseConditionalEventListener, IEventTrigger<T, K>
{
	IEventTrigger<T, K> _evnt;

	public override bool IsValid => base.IsValid && _evnt != null && _evnt.IsValid;

	public ConditionalEventListener( IEventTrigger<T, K> evnt, System.Func<bool> condition ) : base( condition ) { _evnt = evnt; }
	public ConditionalEventListener( System.Action<T, K> action, System.Func<bool> condition ) : this( new ActionEventCapsule<T, K>( action ), condition ) { }
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

public sealed class ConditionalEventListener<T, K, J> : BaseConditionalEventListener, IEventTrigger<T, K, J>
{
	IEventTrigger<T, K, J> _evnt;

	public override bool IsValid => base.IsValid && _evnt != null && _evnt.IsValid;

	public ConditionalEventListener( IEventTrigger<T, K, J> evnt, System.Func<bool> condition ) : base( condition ) { _evnt = evnt; }
	public ConditionalEventListener( System.Action<T, K, J> action, System.Func<bool> condition ) : this( new ActionEventCapsule<T, K, J>( action ), condition ) { }
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
