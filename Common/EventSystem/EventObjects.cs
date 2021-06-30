using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class EventSlot : IEvent
{
	private List<IEventTrigger> _listeners;

	public bool IsValid => true;
	public int EventsCount => _listeners?.Count ?? 0;


	public void Trigger()
	{
		if( _listeners == null ) return;

		var executionQueue = Temp<List<IEventTrigger>>.Request();
		executionQueue.AddRange( _listeners );

		for( int i = 0; i < executionQueue.Count; i++ )
		{
			var listener = executionQueue[i];
			if( listener.IsValid ) listener.Trigger();
			//NOT else Trigger can invalidate listener
			if( !listener.IsValid ) _listeners.Remove( listener );
		}

		Temp<List<IEventTrigger>>.Return( executionQueue );
	}

	public void Register( IEventTrigger listener )
	{
		if( listener == null ) return;
		if( _listeners == null ) _listeners = new List<IEventTrigger>();
		_listeners.Add( listener );
	}

	public bool Unregister( IEventTrigger listener ) { return _listeners?.Remove( listener ) ?? false; }

	public override string ToString() { return $"[EventSlot:{EventsCount}]"; }
}

public class EventSlot<T> : IEvent<T>
{
	private EventSlot _generic;
	private List<IEventTrigger<T>> _listeners;

	public bool IsValid => true;
	public int EventsCount => ( ( _generic?.EventsCount ?? 0 ) + ( _listeners?.Count ?? 0 ) );

	public void Trigger( T t )
	{
		TryTriggerSpecific( t );
		TryTriggerGeneric();
	}

	private void TryTriggerSpecific( T t )
	{
		if( _listeners == null ) return;
		var executionQueue = Temp<List<IEventTrigger<T>>>.Request();
		executionQueue.AddRange( _listeners );

		for( int i = 0; i < executionQueue.Count; i++ )
		{
			var listener = executionQueue[i];
			if( listener.IsValid ) listener.Trigger( t );
			//NOT else Trigger can invalidate listener
			if( !listener.IsValid ) _listeners.Remove( listener );
		}
		Temp<List<IEventTrigger<T>>>.Return( executionQueue );
	}

	private void TryTriggerGeneric()
	{
		if( _generic == null ) return;
		_generic.Trigger();
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

	public bool Unregister( IEventTrigger<T> listener ) { return _listeners?.Remove( listener ) ?? false; }
	public bool Unregister( IEventTrigger listener ) { return _generic?.Unregister( listener ) ?? false; }

	public override string ToString() { return $"[EventSlot<{typeof(T)}>:{_listeners?.Count ?? 0}, Generic:{_generic.ToStringOrNull()}]"; }
}

public class EventSlot<T, K> : IEvent<T, K>
{
	private EventSlot<T> _generic;
	private List<IEventTrigger<T, K>> _listeners;

	public bool IsValid => true;
	public int EventsCount => ( ( _generic?.EventsCount ?? 0 ) + ( _listeners?.Count ?? 0 ) );

	public void Trigger( T t, K k )
	{
		TryTriggerSpecific( t, k );
		TryTriggerGeneric( t );
	}

	private void TryTriggerSpecific( T t, K k )
	{
		if( _listeners == null ) return;
		var executionQueue = Temp<List<IEventTrigger<T,K>>>.Request();
		executionQueue.AddRange( _listeners );

		for( int i = 0; i < executionQueue.Count; i++ )
		{
			var listener = executionQueue[i];
			if( listener.IsValid ) listener.Trigger( t, k );
			//NOT else Trigger can invalidate listener
			if( !listener.IsValid ) _listeners.Remove( listener );
		}
		Temp<List<IEventTrigger<T,K>>>.Return( executionQueue );
	}

	private void TryTriggerGeneric( T t )
	{
		if( _generic == null ) return;
		_generic.Trigger( t );
	}

	public void Register( IEventTrigger<T, K> listener )
	{
		if( _listeners == null ) _listeners = new List<IEventTrigger<T,K>>();
		_listeners.Add( listener );
	}

	public void Register( IEventTrigger<T> listener ) { RequestGenericEvent().Register( listener ); }
	public void Register( IEventTrigger listener ) { RequestGenericEvent().Register( listener ); }

	private EventSlot<T> RequestGenericEvent()
	{
		if( _generic == null ) _generic = new EventSlot<T>();
		return _generic;
	}

	public bool Unregister( IEventTrigger<T, K> listener ) { return _listeners?.Remove( listener ) ?? false; }
	public bool Unregister( IEventTrigger<T> listener ) { return _generic?.Unregister( listener ) ?? false; }
	public bool Unregister( IEventTrigger listener ) { return _generic?.Unregister( listener ) ?? false; }

	public override string ToString() { return $"[EventSlot<{typeof(T)},{typeof(K)}>:{_listeners?.Count ?? 0}, Generic:{_generic.ToStringOrNull()}]"; }
}

public class EventSlot<T, K, L> : IEvent<T, K, L>
{
	private EventSlot<T, K> _generic;
	private List<IEventTrigger<T, K, L>> _listeners;

	public bool IsValid => true;
	public int EventsCount => ( ( _generic?.EventsCount ?? 0 ) + ( _listeners?.Count ?? 0 ) );

	public void Trigger( T t, K k, L l )
	{
		TryTriggerSpecific( t, k, l );
		TryTriggerGeneric( t, k );
	}

	private void TryTriggerSpecific( T t, K k, L l )
	{
		if( _listeners == null ) return;
		var executionQueue = Temp<List<IEventTrigger<T, K, L>>>.Request();
		executionQueue.AddRange( _listeners );

		for( int i = 0; i < executionQueue.Count; i++ )
		{
			var listener = executionQueue[i];
			if( listener.IsValid ) listener.Trigger( t, k, l );
			//NOT else Trigger can invalidate listener
			if( !listener.IsValid ) _listeners.Remove( listener );
		}
		Temp<List<IEventTrigger<T, K, L>>>.Return( executionQueue );
	}

	private void TryTriggerGeneric( T t, K k )
	{
		if( _generic == null ) return;
		_generic.Trigger( t, k );
	}

	public void Register( IEventTrigger<T, K, L> listener ) 
	{ 
		if( _listeners == null ) _listeners = new List<IEventTrigger<T, K, L>>();
		_listeners.Add( listener ); 
	}
	public void Register( IEventTrigger<T, K> listener ) { RequestGenericEvent().Register( listener ); }
	public void Register( IEventTrigger<T> listener ) { RequestGenericEvent().Register( listener ); }
	public void Register( IEventTrigger listener ) { RequestGenericEvent().Register( listener ); }

	private EventSlot<T, K> RequestGenericEvent()
	{
		if( _generic == null ) _generic = new EventSlot<T, K>();
		return _generic;
	}

	public bool Unregister( IEventTrigger<T, K, L> listener ) { return _listeners?.Remove( listener ) ?? false; }
	public bool Unregister( IEventTrigger<T, K> listener ) { return _generic?.Unregister( listener ) ?? false; }
	public bool Unregister( IEventTrigger<T> listener ) { return _generic?.Unregister( listener ) ?? false; }
	public bool Unregister( IEventTrigger listener ) { return _generic?.Unregister( listener ) ?? false; }

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