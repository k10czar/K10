using System;


public class ActionEventCapsule : IEventTrigger
{
	private readonly System.Action _callback;

	public ActionEventCapsule( System.Action callback ) { _callback = callback; }
	public void Trigger() { _callback(); }
	bool IValidatedObject.IsValid { get { return _callback != null; } }

	public override bool Equals( object obj )
	{
		if( obj is ActionEventCapsule && _callback != null )
		{
			var del = (ActionEventCapsule)obj;
			return _callback.Equals( del._callback );
		}
		return base.Equals( obj );
	}
	public override int GetHashCode() { return _callback.GetHashCode(); }
}

public class ActionEventCapsule<T> : IEventTrigger<T>
{
	private readonly System.Action<T> _callback;

	public ActionEventCapsule( System.Action<T> callback ) { _callback = callback; }
	public void Trigger( T t ) { _callback( t ); }
	public bool IsValid { get { return _callback != null; } }

	public override bool Equals( object obj )
	{
		if( obj is ActionEventCapsule<T> && _callback != null )
		{
			var del = (ActionEventCapsule<T>)obj;
			return _callback.Equals( del._callback );
		}
		return base.Equals( obj );
	}
	public override int GetHashCode() { return _callback.GetHashCode(); }
}

public class ActionEventCapsule<T, K> : IEventTrigger<T, K>
{
	private readonly System.Action<T, K> _callback;

	public ActionEventCapsule( System.Action<T, K> callback ) { _callback = callback; }
	public void Trigger( T t, K k ) { _callback( t, k ); }
	public bool IsValid { get { return _callback != null; } }

	public override bool Equals( object obj )
	{
		if( obj is ActionEventCapsule<T, K> && _callback != null )
		{
			var del = (ActionEventCapsule<T, K>)obj;
			return _callback.Equals( del._callback );
		}
		return base.Equals( obj );
	}
	public override int GetHashCode() { return _callback.GetHashCode(); }
}


public class ActionEventCapsule<T, K, L> : IEventTrigger<T, K, L>
{
	private readonly System.Action<T, K, L> _callback;

	public ActionEventCapsule( System.Action<T, K, L> callback ) { _callback = callback; }
	public void Trigger( T t, K k, L l ) { _callback( t, k, l ); }
	public bool IsValid { get { return _callback != null; } }

	public override bool Equals( object obj )
	{
		if( obj is ActionEventCapsule<T, K, L> && _callback != null )
		{
			var del = (ActionEventCapsule<T, K, L>)obj;
			return _callback.Equals( del._callback );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() { return _callback.GetHashCode(); }
}