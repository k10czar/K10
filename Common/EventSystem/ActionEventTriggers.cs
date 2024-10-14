

[UnityEngine.HideInInspector]
public struct ActionEventCapsule : IEventTrigger//, ICustomDisposableKill
{
	private System.Action _callback;

	// public void Kill()
	// {
	// 	_callback = null;
	// }

	public ActionEventCapsule( System.Action callback ) { _callback = callback; }
	public void Trigger() { _callback(); }
	bool IValidatedObject.IsValid { get { return _callback != null; } }

	public override bool Equals( object obj )
	{
		if( obj == null ) return false;
		if( GetHashCode() != obj.GetHashCode() ) return false;
		if( obj is ActionEventCapsule cap )
		{
			if( _callback != null ) return _callback.Equals( cap._callback );
			else cap._callback.Equals( null );
		}
		return base.Equals( obj );
	}
	public override int GetHashCode() { return _callback?.GetHashCode() ?? 0; }
}

[UnityEngine.HideInInspector]
public struct ActionEventCapsule<T> : IEventTrigger<T>//, ICustomDisposableKill
{
	private System.Action<T> _callback;

	// public void Kill()
	// {
	// 	_callback = null;
	// }

	public ActionEventCapsule( System.Action<T> callback ) { _callback = callback; }
	public void Trigger( T t ) { _callback( t ); }
	public bool IsValid { get { return _callback != null; } }

	public override bool Equals( object obj )
	{
		if( obj == null ) return false;
		if( GetHashCode() != obj.GetHashCode() ) return false;
		if( obj is ActionEventCapsule<T> cap )
		{
			if( _callback != null ) return _callback.Equals( cap._callback );
			else cap._callback.Equals( null );
		}
		return base.Equals( obj );
	}
	public override int GetHashCode() { return _callback?.GetHashCode() ?? 0; }
}

[UnityEngine.HideInInspector]
public struct ActionEventCapsule<T, K> : IEventTrigger<T, K>//, ICustomDisposableKill
{
	private System.Action<T, K> _callback;

	// public void Kill()
	// {
	// 	_callback = null;
	// }

	public ActionEventCapsule( System.Action<T, K> callback ) { _callback = callback; }
	public void Trigger( T t, K k ) { _callback( t, k ); }
	public bool IsValid { get { return _callback != null; } }

	public override bool Equals( object obj )
	{
		if( obj == null ) return false;
		if( GetHashCode() != obj.GetHashCode() ) return false;
		if( obj is ActionEventCapsule<T,K> cap )
		{
			if( _callback != null ) return _callback.Equals( cap._callback );
			else cap._callback.Equals( null );
		}
		return base.Equals( obj );
	}
	public override int GetHashCode() { return _callback?.GetHashCode() ?? 0; }
}


[UnityEngine.HideInInspector]
public struct ActionEventCapsule<T, K, L> : IEventTrigger<T, K, L>//, ICustomDisposableKill
{
	private System.Action<T, K, L> _callback;

	// public void Kill()
	// {
	// 	_callback = null;
	// }

	public ActionEventCapsule( System.Action<T, K, L> callback ) { _callback = callback; }
	public void Trigger( T t, K k, L l ) { _callback( t, k, l ); }
	public bool IsValid { get { return _callback != null; } }

	public override bool Equals( object obj )
	{
		if( obj == null ) return false;
		if( GetHashCode() != obj.GetHashCode() ) return false;
		if( obj is ActionEventCapsule<T,K,L> cap )
		{
			if( _callback != null ) return _callback.Equals( cap._callback );
			else cap._callback.Equals( null );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() { return _callback?.GetHashCode() ?? 0; }
}