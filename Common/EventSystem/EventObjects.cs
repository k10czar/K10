using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class EventSlot : IEvent
{
	// private List<IEventTrigger> _listeners;
	private List<IEventTrigger> _listeners = new List<IEventTrigger>();

	public bool IsValid { get { return true; } }
	public int EventsCount => _listeners?.Count ?? 0;

	public void Trigger()
	{
		if( _listeners == null || _listeners.Count == 0 ) return;

		var listenersToTrigger = ObjectPool<List<IEventTrigger>>.Request();
		listenersToTrigger.AddRange( _listeners );

		for( int i = 0; i < listenersToTrigger.Count; i++ )
		{
			var listener = listenersToTrigger[i];
			if( listener.IsValid ) listener.Trigger();
			//NOT else Trigger can invalidate listener
			if( !listener.IsValid )
			{
				_listeners.Remove( listener );
				TryClearFullSignatureList();
			}
		}

		ObjectPool<List<IEventTrigger>>.Return( listenersToTrigger );
	}

	public void Clear() { _listeners?.Clear(); }
	private void TryClearFullSignatureList() 
	{
		// if( _listeners.Count == 0 ) _listeners = null;
	}

	public void Register( IEventTrigger listener ) 
	{
		if( listener == null ) return;
		if( _listeners == null ) _listeners = new List<IEventTrigger>();
		_listeners.Add( listener );
	}

	public bool Unregister( IEventTrigger listener )
	{
		if( _listeners == null ) return false;
		bool removed = _listeners.Remove( listener );
		if( removed ) TryClearFullSignatureList();
		return removed;
	}

	public override string ToString() { return $"[EventSlot:{EventsCount}]"; }
}

public class EventSlot<T> : IEvent<T>
{
	// private EventSlot _generic;
	// private List<IEventTrigger<T>> _listeners;
	private EventSlot _generic = new EventSlot();
	private List<IEventTrigger<T>> _listeners = new List<IEventTrigger<T>>();

	public bool IsValid { get { return true; } }
	public int EventsCount => ( ( _generic?.EventsCount ?? 0 ) + ( _listeners?.Count ?? 0 ) );

	public void Trigger( T t )
	{
		if( _listeners != null && _listeners.Count > 0 )
		{
			var listenersToTrigger = ObjectPool<List<IEventTrigger<T>>>.Request();
			listenersToTrigger.AddRange( _listeners );

			for( int i = 0; i < listenersToTrigger.Count; i++ )
			{
				var listener = listenersToTrigger[i];
				if( listener.IsValid ) listener.Trigger( t );
				//NOT else Trigger can invalidate listener
				if( !listener.IsValid )
				{
					_listeners.Remove( listener );
					TryClearFullSignatureList();
				}
			}
			ObjectPool<List<IEventTrigger<T>>>.Return( listenersToTrigger );
		}

		if( _generic != null )
		{
			_generic.Trigger();
			TryClearGeneric();
		}
	}

	public void Clear()
	{
		_listeners?.Clear();
		_generic?.Clear();
		_listeners = null;
		_generic = null;
	}

	private void TryClearGeneric() 
	{
		// if( _generic.EventsCount == 0 ) _generic = null;
	}
	private void TryClearFullSignatureList()
	{
		// if( _listeners.Count == 0 ) _listeners = null;
	}

	public void Register( IEventTrigger<T> listener ) 
	{
		if( listener == null ) return;
		if( _listeners == null ) _listeners = new List<IEventTrigger<T>>();
		_listeners.Add( listener );
	}

	public void Register( IEventTrigger listener )
	{ 
		if( listener == null ) return;
		if( _generic == null ) _generic = new EventSlot();
		_generic.Register( listener );
	}

	public bool Unregister( IEventTrigger<T> listener )
	{
		if( _listeners == null ) return false;
		bool removed = _listeners.Remove( listener );
		if( removed ) TryClearFullSignatureList();
		return removed;
	}

	public bool Unregister( IEventTrigger listener )
	{
		if( _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}


	public override string ToString() { return $"[EventSlot<{typeof(T)}>:{_listeners?.Count ?? 0}, Generic:{_generic.ToStringOrNull()}]"; }
}

public class EventSlot<T, K> : IEvent<T, K>
{
	// private EventSlot<T> _generic;
	// private List<IEventTrigger<T, K>> _listeners;
	private EventSlot<T> _generic = new EventSlot<T>();
	private List<IEventTrigger<T, K>> _listeners = new List<IEventTrigger<T, K>>();

	public bool IsValid { get { return true; } }
	public int EventsCount => ( ( _generic?.EventsCount ?? 0 ) + ( _listeners?.Count ?? 0 ) );

	public void Trigger( T t, K k )
	{
		if( _listeners != null && _listeners.Count > 0 )
		{
			var listenersToTrigger = ObjectPool<List<IEventTrigger<T,K>>>.Request();
			listenersToTrigger.AddRange( _listeners );

			for( int i = 0; i < listenersToTrigger.Count; i++ )
			{
				var listener = listenersToTrigger[i];
				if( listener.IsValid ) listener.Trigger( t, k );
				//NOT else Trigger can invalidate listener
				if( !listener.IsValid )
				{
					_listeners.Remove( listener );
					TryClearFullSignatureList();
				}
			}
			ObjectPool<List<IEventTrigger<T,K>>>.Return( listenersToTrigger );
		}

		if( _generic != null )
		{
			_generic.Trigger( t );
			TryClearGeneric();
		}
	}

	private void TryClearGeneric() 
	{
		// if( _generic.EventsCount == 0 ) _generic = null;
	}
	private void TryClearFullSignatureList()
	{
		// if( _listeners.Count == 0 ) _listeners = null;
	}

	public void Clear()
	{
		_listeners?.Clear();
		_generic?.Clear();
		_listeners = null;
		_generic = null;
	}

	public void Register( IEventTrigger<T, K> listener )
	{
		if( listener == null ) return;
		if( _listeners == null ) _listeners = new List<IEventTrigger<T, K>>();
		_listeners.Add( listener );
	}

	public void Register( IEventTrigger<T> listener )
	{
		if( listener == null ) return;
		if( _generic == null ) _generic = new EventSlot<T>();
		_generic.Register( listener );
	}

	public void Register( IEventTrigger listener )
	{
		if( listener == null ) return;
		if( _generic == null ) _generic = new EventSlot<T>();
		_generic.Register( listener );
	}

	public bool Unregister( IEventTrigger<T, K> listener )
	{
		if( _listeners == null ) return false;
		bool removed = _listeners.Remove( listener );
		if( removed ) TryClearFullSignatureList();
		return removed;
	}

	public bool Unregister( IEventTrigger<T> listener )
	{
		if( _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}

	public bool Unregister( IEventTrigger listener )
	{
		if( _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}

	public override string ToString() { return $"[EventSlot<{typeof(T)},{typeof(K)}>:{_listeners?.Count ?? 0}, Generic:{_generic.ToStringOrNull()}]"; }
}

public class EventSlot<T, K, L> : IEvent<T, K, L>
{
	// private EventSlot<T, K> _generic;
	// private List<IEventTrigger<T, K, L>> _listeners;
	private EventSlot<T, K> _generic = new EventSlot<T, K>();
	private List<IEventTrigger<T, K, L>> _listeners = new List<IEventTrigger<T, K, L>>();

	public bool IsValid { get { return true; } }
	public int EventsCount => ( _generic.EventsCount + _listeners.Count );

	public void Trigger( T t, K k, L l )
	{
		if( _listeners != null && _listeners.Count > 0 )
		{
			var listenersToTrigger = ObjectPool<List<IEventTrigger<T, K, L>>>.Request();
			listenersToTrigger.AddRange( _listeners );

			for( int i = 0; i < listenersToTrigger.Count; i++ )
			{
				var listener = listenersToTrigger[i];
				if( listener.IsValid ) listener.Trigger( t, k, l );
				//NOT else Trigger can invalidate listener
				if( !listener.IsValid )
				{
					_listeners.Remove( listener );
					TryClearFullSignatureList();
				}
			}
			ObjectPool<List<IEventTrigger<T, K, L>>>.Return( listenersToTrigger );
		}

		if( _generic != null )
		{
			_generic.Trigger( t, k );
			TryClearGeneric();
		}
	}

	private void TryClearGeneric() 
	{
		// if( _generic.EventsCount == 0 ) _generic = null;
	}
	
	private void TryClearFullSignatureList()
	{
		// if( _listeners.Count == 0 ) _listeners = null;
	}

	public void Clear()
	{
		_listeners?.Clear();
		_generic?.Clear();
		_listeners = null;
		_generic = null;
	}

	public void Register( IEventTrigger<T, K, L> listener )
	{
		if( listener == null ) return;
		if( _listeners == null ) _listeners = new List<IEventTrigger<T, K, L>>();
		_listeners.Add( listener );
	}

	public void Register( IEventTrigger<T, K> listener )
	{
		if( listener == null ) return;
		if( _generic == null ) _generic = new EventSlot<T, K>();
		_generic.Register( listener );
	}

	public void Register( IEventTrigger<T> listener )
	{
		if( listener == null ) return;
		if( _generic == null ) _generic = new EventSlot<T, K>();
		_generic.Register( listener );
	}

	public void Register( IEventTrigger listener )
	{
		if( listener == null ) return;
		if( _generic == null ) _generic = new EventSlot<T,K>();
		_generic.Register( listener );
	}

	public bool Unregister( IEventTrigger<T, K, L> listener )
	{
		if( _listeners == null ) return false;
		bool removed = _listeners.Remove( listener );
		if( removed ) TryClearFullSignatureList();
		return removed;
	}

	public bool Unregister( IEventTrigger<T, K> listener ) 
	{
		if( _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}

	public bool Unregister( IEventTrigger<T> listener ) 
	{
		if( _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}

	public bool Unregister( IEventTrigger listener )
	{
		if( _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}

	public override string ToString() { return $"[EventSlot<{typeof(T)},{typeof(K)},{typeof(L)}>:{_listeners?.Count ?? 0}, Generic:{_generic.ToStringOrNull()}]"; }
}

public class VoidableEventTrigger : IEventTrigger
{
	IEventTrigger _trigger;
	bool _voided;

	public VoidableEventTrigger( IEventTrigger trigger ) { _trigger = trigger; }
	public VoidableEventTrigger( Action act ) : this( new ActionEventCapsule( act ) ) { }
	public bool IsValid { get { return !_voided && _trigger.IsValid; } }
	public void Trigger() { if( _trigger.IsValid ) _trigger.Trigger(); }
	public void Void() { _voided = true; }
}

public class VoidableEventTrigger<T> : IEventTrigger<T>
{
	IEventTrigger<T> _trigger;
	bool _voided;

	public VoidableEventTrigger( IEventTrigger<T> trigger ) { _trigger = trigger; }
	public VoidableEventTrigger( Action<T> act ) : this( new ActionEventCapsule<T>( act ) ) { }
	public bool IsValid { get { return !_voided && _trigger.IsValid; } }
	public void Trigger( T t ) { if( _trigger.IsValid ) _trigger.Trigger( t ); }
	public void Void() { _voided = true; }
}

public class VoidableEventTrigger<T, K> : IEventTrigger<T, K>
{
	IEventTrigger<T, K> _trigger;
	bool _voided;

	public VoidableEventTrigger( IEventTrigger<T, K> trigger ) { _trigger = trigger; }
	public VoidableEventTrigger( Action<T, K> act ) : this( new ActionEventCapsule<T, K>( act ) ) { }
	public bool IsValid { get { return !_voided && _trigger.IsValid; } }
	public void Trigger( T t, K k ) { if( _trigger.IsValid ) _trigger.Trigger( t, k ); }
	public void Void() { _voided = true; }
}