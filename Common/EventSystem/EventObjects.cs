using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class EventSlot : IEvent
{
	List<IEventTrigger> _listeners = new List<IEventTrigger>();

	public bool IsValid { get { return true; } }
	public int EventsCount => _listeners.Count;

	public void Trigger()
	{
		for( int i = 0; i < _listeners.Count; i++ )
		{
			var listener = _listeners[i];
			if( listener.IsValid ) listener.Trigger();
			//NOT else Trigger can invalidate listener
			if( !listener.IsValid ) _listeners.RemoveAt( i-- );
		}
	}

	public void Register( IEventTrigger listener ) { if( listener != null ) _listeners.Add( listener ); }
	public bool Unregister( IEventTrigger listener ) { return _listeners.Remove( listener ); }

	public override string ToString() { return string.Format( "[EventSlot: Count={0}]", _listeners.Count ); }
}

public class EventSlot<T> : IEvent<T>
{
	EventSlot _generic = new EventSlot();
	List<IEventTrigger<T>> _listeners = new List<IEventTrigger<T>>();

	public bool IsValid { get { return true; } }
	public int EventsCount => ( _generic.EventsCount + _listeners.Count );

	public void Trigger( T t )
	{
		for( int i = 0; i < _listeners.Count; i++ )
		{
			var listener = _listeners[i];
			if( listener.IsValid ) listener.Trigger( t );
			//NOT else Trigger can invalidate listener
			if( !listener.IsValid ) _listeners.RemoveAt( i-- );
		}
		_generic.Trigger();
	}

	public void Register( IEventTrigger<T> listener ) { _listeners.Add( listener ); }
	public void Register( IEventTrigger listener ) { _generic.Register( listener ); }

	public bool Unregister( IEventTrigger<T> listener ) { return _listeners.Remove( listener ); }
	public bool Unregister( IEventTrigger listener ) { return _generic.Unregister( listener ); }

	public override string ToString() { return string.Format( "[EventSlot<T>: Count={0}, Generic:{1}]", _listeners.Count, _generic ); }
}

public class EventSlot<T, K> : IEvent<T, K>
{
	EventSlot<T> _generic = new EventSlot<T>();
	List<IEventTrigger<T, K>> _listeners = new List<IEventTrigger<T, K>>();

	public bool IsValid { get { return true; } }
	public int EventsCount => ( _generic.EventsCount + _listeners.Count );

	public void Trigger( T t, K k )
	{
		for( int i = 0; i < _listeners.Count; i++ )
		{
			var listener = _listeners[i];
			if( listener.IsValid ) listener.Trigger( t, k );
			//NOT else Trigger can invalidate listener
			if( !listener.IsValid ) _listeners.RemoveAt( i-- );
		}
		_generic.Trigger( t );
	}

	public void Register( IEventTrigger<T, K> listener ) { _listeners.Add( listener ); }
	public void Register( IEventTrigger<T> listener ) { _generic.Register( listener ); }
	public void Register( IEventTrigger listener ) { _generic.Register( listener ); }

	public bool Unregister( IEventTrigger<T, K> listener ) { return _listeners.Remove( listener ); }
	public bool Unregister( IEventTrigger<T> listener ) { return _generic.Unregister( listener ); }
	public bool Unregister( IEventTrigger listener ) { return _generic.Unregister( listener ); }

	public override string ToString() { return string.Format( "[EventSlot<T,K>: Count={0}, Generic:{1}]", _listeners.Count, _generic ); }
}

public class EventSlot<T, K, L> : IEvent<T, K, L>
{
	EventSlot<T, K> _generic = new EventSlot<T, K>();
	List<IEventTrigger<T, K, L>> _listeners = new List<IEventTrigger<T, K, L>>();

	public bool IsValid { get { return true; } }
	public int EventsCount => ( _generic.EventsCount + _listeners.Count );

	public void Trigger( T t, K k, L l )
	{
		for( int i = _listeners.Count - 1; i >= 0; i-- )
		{
			var listener = _listeners[i];
			if( listener.IsValid ) listener.Trigger( t, k, l );
			if( !listener.IsValid ) _listeners.RemoveAt( i );
		}
		_generic.Trigger( t, k );
	}

	public void Register( IEventTrigger<T, K, L> listener ) { _listeners.Add( listener ); }
	public void Register( IEventTrigger<T, K> listener ) { _generic.Register( listener ); }
	public void Register( IEventTrigger<T> listener ) { _generic.Register( listener ); }
	public void Register( IEventTrigger listener ) { _generic.Register( listener ); }

	public bool Unregister( IEventTrigger<T, K, L> listener ) { return _listeners.Remove( listener ); }
	public bool Unregister( IEventTrigger<T, K> listener ) { return _generic.Unregister( listener ); }
	public bool Unregister( IEventTrigger<T> listener ) { return _generic.Unregister( listener ); }
	public bool Unregister( IEventTrigger listener ) { return _generic.Unregister( listener ); }
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