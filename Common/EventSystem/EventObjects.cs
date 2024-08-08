using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using K10;


public class EventSlot : IEvent, ICustomDisposableKill
{
	bool _killed = false;
	// TODO: LazyOptimization
	private List<IEventTrigger> _listeners;
	// private List<IEventTrigger> _listeners = new List<IEventTrigger>();

	public bool IsValid => !_killed;
	public int EventsCount => _listeners?.Count ?? 0;
	public int CountValidEvents => _listeners.Count( ( et ) => et.IsValid );
	public bool HasListeners => EventsCount > 0;

	public void Trigger()
	{
		if( _killed )
		{
			Debug.LogError( $"Error: Cannot Trigger dead EventSlot" );
			return;
		}

		if( _listeners == null || _listeners.Count == 0 ) return;

		var listenersToTrigger = ObjectPool<List<IEventTrigger>>.Request();
		listenersToTrigger.AddRange( _listeners );

		var count = listenersToTrigger.Count;
		for( int i = 0; i < count; i++ )
		{
			try
			{
				var listener = listenersToTrigger[i];
				if (listener.IsValid) listener.Trigger();
				//NOT else Trigger can invalidate listener
				if (!listener.IsValid)
				{
					_listeners?.Remove(listener);
					TryClearFullSignatureList();
				}
			}
			catch (Exception exception)
			{
				Debug.LogError($"{exception.Message}\n{exception.StackTrace}");
			}
		}

		ObjectPool<List<IEventTrigger>>.Return( listenersToTrigger );
	}

	public void Kill()
	{
		_killed = true;
		Clear();
		_listeners = null;
	}

	public void Clear() => _listeners?.Clear();

	private void TryClearFullSignatureList()
	{
		if( _listeners == null || _listeners.Count == 0 ) _listeners = null;
	}

	public void Register( IEventTrigger listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _listeners ).Add( listener );
	}

	public bool Unregister( IEventTrigger listener )
	{
		if( _killed || _listeners == null ) return false;
		bool removed = _listeners.Remove( listener );
		if( removed ) TryClearFullSignatureList();
		return removed;
	}

	public override string ToString() { return $"[EventSlot:{EventsCount}]"; }
}

public class EventSlot<T> : IEvent<T>, ICustomDisposableKill
{
	bool _killed = false;
	// TODO: LazyOptimization
	private EventSlot _generic;
	private List<IEventTrigger<T>> _listeners;
	// private EventSlot _generic = new EventSlot();
	// private List<IEventTrigger<T>> _listeners = new List<IEventTrigger<T>>();

	public bool IsValid => !_killed;
	public int EventsCount => ( ( _generic?.EventsCount ?? 0 ) + ( _listeners?.Count ?? 0 ) );
	public int CountValidEvents => ( _generic?.CountValidEvents ?? 0 ) + _listeners.Count( ( et ) => et.IsValid );
	public bool HasListeners => EventsCount > 0;
	
	public static implicit operator EventSlot( EventSlot<T> v ) => Lazy.Request( ref v._generic );

	public void Trigger( T t )
	{
		if( _killed )
		{
			Debug.LogError( $"Error: Cannot Trigger dead EventSlot<{typeof( T )}>" );
			return;
		}

		if( _listeners != null && _listeners.Count > 0 )
		{
			var listenersToTrigger = ObjectPool<List<IEventTrigger<T>>>.Request();
			listenersToTrigger.AddRange( _listeners );

			var count = listenersToTrigger.Count;
			for( int i = 0; i < count; i++ )
			{
				try
				{
					var listener = listenersToTrigger[i];
					if (listener.IsValid) listener.Trigger(t);
					//NOT else Trigger can invalidate listener
					if (!listener.IsValid)
					{
						_listeners?.Remove(listener);
						TryClearFullSignatureList();
					}
				}
				catch (Exception exception)
				{
					Debug.LogError($"{exception.Message}\n{exception.StackTrace}");
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

	public void Kill()
	{
		_killed = true;
		_listeners?.Clear();
		_generic?.Kill();
		_listeners = null;
		_generic = null;
	}

	public void Clear()
	{
		_generic.Clear();
		_listeners?.Clear();
	}

	private void TryClearGeneric()
	{
		if( _generic == null ) return;
		if( _generic.EventsCount == 0 ) _generic = null;
	}
	private void TryClearFullSignatureList()
	{
		if( _listeners == null ) return;
		if( _listeners.Count == 0 ) _listeners = null;
	}

	public void Register( IEventTrigger<T> listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _listeners ).Add( listener );
	}

	public void Register( IEventTrigger listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _generic ).Register( listener );
	}

	public bool Unregister( IEventTrigger<T> listener )
	{
		if( _killed || _listeners == null ) return false;
		bool removed = _listeners.Remove( listener );
		if( removed ) TryClearFullSignatureList();
		return removed;
	}

	public bool Unregister( IEventTrigger listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}


	public override string ToString() { return $"[EventSlot<{typeof(T)}>:{_listeners?.Count ?? 0}, Generic:{_generic.ToStringOrNull()}]"; }
}

public class EventSlot<T, K> : IEvent<T, K>, ICustomDisposableKill
{
	bool _killed = false;
	// TODO: LazyOptimization
	private EventSlot<T> _generic;
	private List<IEventTrigger<T, K>> _listeners;
	// private EventSlot<T> _generic = new EventSlot<T>();
	// private List<IEventTrigger<T, K>> _listeners = new List<IEventTrigger<T, K>>();

	public bool IsValid => !_killed;
	public int EventsCount => ( ( _generic?.EventsCount ?? 0 ) + ( _listeners?.Count ?? 0 ) );
	public int CountValidEvents => ( _generic?.CountValidEvents ?? 0 ) + _listeners.Count( ( et ) => et.IsValid );
	public bool HasListeners => EventsCount > 0;

	public static implicit operator EventSlot<T>( EventSlot<T, K> v ) => Lazy.Request( ref v._generic );
	
	public void Trigger( T t, K k )
	{
		if( _killed )
		{
			Debug.LogError( $"Error: Cannot Trigger dead EventSlot<{typeof( T )},{typeof( K )}>" );
			return;
		}

		if( _listeners != null && _listeners.Count > 0 )
		{
			var listenersToTrigger = ObjectPool<List<IEventTrigger<T,K>>>.Request();
			listenersToTrigger.AddRange( _listeners );

			var count = listenersToTrigger.Count;
			for( int i = 0; i < count; i++ )
			{
				try
				{
					var listener = listenersToTrigger[i];
					if (listener.IsValid) listener.Trigger(t, k);
					//NOT else Trigger can invalidate listener
					if (!listener.IsValid)
					{
						_listeners?.Remove(listener);
						TryClearFullSignatureList();
					}
				}
				catch (Exception exception)
				{
					Debug.LogError($"{exception.Message}\n{exception.StackTrace}");
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
		if( _generic == null ) return;
		if( _generic.EventsCount == 0 ) _generic = null;
	}
	private void TryClearFullSignatureList()
	{
		if( _listeners == null ) return;
		if( _listeners.Count == 0 ) _listeners = null;
	}

	public void Kill()
	{
		_killed = true;
		_listeners?.Clear();
		_generic?.Kill();
		_listeners = null;
		_generic = null;
	}

	public void Clear()
	{
		_generic.Clear();
		_listeners?.Clear();
	}

	public void Register( IEventTrigger<T, K> listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _listeners ).Add( listener );
	}

	public void Register( IEventTrigger<T> listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _generic ).Register( listener );
	}

	public void Register( IEventTrigger listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _generic ).Register( listener );
	}

	public bool Unregister( IEventTrigger<T, K> listener )
	{
		if( _killed || _listeners == null ) return false;
		bool removed = _listeners.Remove( listener );
		if( removed ) TryClearFullSignatureList();
		return removed;
	}

	public bool Unregister( IEventTrigger<T> listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}

	public bool Unregister( IEventTrigger listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}

	public override string ToString() { return $"[EventSlot<{typeof(T)},{typeof(K)}>:{_listeners?.Count ?? 0}, Generic:{_generic.ToStringOrNull()}]"; }
}

public class EventSlot<T, K, L> : IEvent<T, K, L>, ICustomDisposableKill
{
	bool _killed = false;
	// TODO: LazyOptimization
	private EventSlot<T, K> _generic;
	private List<IEventTrigger<T, K, L>> _listeners;
	// private EventSlot<T, K> _generic = new EventSlot<T, K>();
	// private List<IEventTrigger<T, K, L>> _listeners = new List<IEventTrigger<T, K, L>>();

	public bool IsValid => !_killed;
	public int EventsCount => ( _generic.EventsCount + _listeners.Count );
	public int CountValidEvents => ( _generic?.CountValidEvents ?? 0 ) + _listeners.Count( ( et ) => et.IsValid );
	public bool HasListeners => EventsCount > 0;
	
	public static implicit operator EventSlot<T, K>( EventSlot<T, K, L> v ) => Lazy.Request( ref v._generic );

	public void Trigger( T t, K k, L l )
	{
		if( _killed )
		{
			Debug.LogError( $"Error: Cannot Trigger dead EventSlot<{typeof( T )},{typeof( K )},{typeof( L )}>" );
			return;
		}

		if( _listeners != null && _listeners.Count > 0 )
		{
			var listenersToTrigger = ObjectPool<List<IEventTrigger<T, K, L>>>.Request();
			listenersToTrigger.AddRange( _listeners );

			var count = listenersToTrigger.Count;
			for( int i = 0; i < count; i++ )
			{
				try
				{
					var listener = listenersToTrigger[i];
					if (listener.IsValid) listener.Trigger(t, k, l);
					//NOT else Trigger can invalidate listener
					if (!listener.IsValid)
					{
						_listeners?.Remove(listener);
						TryClearFullSignatureList();
					}
				}
				catch (Exception exception)
				{
					Debug.LogError($"{exception.Message}\n{exception.StackTrace}");
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
		if( _generic == null || _generic.EventsCount == 0 ) _generic = null;
	}

	private void TryClearFullSignatureList()
	{
		if( _listeners == null || _listeners.Count == 0 ) _listeners = null;
	}

	public void Kill()
	{
		_killed = true;
		_listeners?.Clear();
		_generic?.Kill();
		_listeners = null;
		_generic = null;
	}

	public void Clear()
	{
		_generic.Clear();
		_listeners?.Clear();
	}

	public void Register( IEventTrigger<T, K, L> listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _listeners ).Add( listener );
	}

	public void Register( IEventTrigger<T, K> listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _generic ).Register( listener );
	}

	public void Register( IEventTrigger<T> listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _generic ).Register( listener );
	}

	public void Register( IEventTrigger listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _generic ).Register( listener );
	}

	public bool Unregister( IEventTrigger<T, K, L> listener )
	{
		if( _killed || _listeners == null ) return false;
		bool removed = _listeners.Remove( listener );
		if( removed ) TryClearFullSignatureList();
		return removed;
	}

	public bool Unregister( IEventTrigger<T, K> listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}

	public bool Unregister( IEventTrigger<T> listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}

	public bool Unregister( IEventTrigger listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		if( removed ) TryClearGeneric();
		return removed;
	}

	public override string ToString() { return $"[EventSlot<{typeof(T)},{typeof(K)},{typeof(L)}>:{_listeners?.Count ?? 0}, Generic:{_generic.ToStringOrNull()}]"; }
}

public class VoidableEventTrigger : IEventTrigger, ICustomDisposableKill
{
	IEventTrigger _trigger;

	public VoidableEventTrigger( IEventTrigger trigger ) { _trigger = trigger; }
	public VoidableEventTrigger( Action act ) : this( new ActionEventCapsule( act ) ) { }
	public bool IsValid => _trigger?.IsValid ?? false;
	public void Trigger() { if( IsValid ) _trigger.Trigger(); }
	public void Void() { _trigger = null; }
	public void Kill() { _trigger = null; }
}

public class VoidableEventTrigger<T> : IEventTrigger<T>, ICustomDisposableKill
{
	IEventTrigger<T> _trigger;

	public VoidableEventTrigger( IEventTrigger<T> trigger ) { _trigger = trigger; }
	public VoidableEventTrigger( Action<T> act ) : this( new ActionEventCapsule<T>( act ) ) { }
	public bool IsValid => _trigger?.IsValid ?? false;
	public void Trigger( T t ) { if( IsValid ) _trigger.Trigger( t ); }
	public void Void() { _trigger = null; }
	public void Kill() { _trigger = null; }
}

public class VoidableEventTrigger<T, K> : IEventTrigger<T, K>, ICustomDisposableKill
{
	IEventTrigger<T, K> _trigger;

	public VoidableEventTrigger( IEventTrigger<T, K> trigger ) { _trigger = trigger; }
	public VoidableEventTrigger( Action<T, K> act ) : this( new ActionEventCapsule<T, K>( act ) ) { }
	public bool IsValid => _trigger?.IsValid ?? false;
	public void Trigger( T t, K k ) { if( IsValid ) _trigger.Trigger( t, k ); }
	public void Void() { _trigger = null; }
	public void Kill() { _trigger = null; }
}

public class VoidableEventTrigger<T, K, J> : IEventTrigger<T, K, J>, ICustomDisposableKill
{
	IEventTrigger<T, K, J> _trigger;

	public VoidableEventTrigger( IEventTrigger<T, K, J> trigger ) { _trigger = trigger; }
	public VoidableEventTrigger( Action<T, K, J> act ) : this( new ActionEventCapsule<T, K, J>( act ) ) { }
	public bool IsValid => _trigger?.IsValid ?? false;
	public void Trigger( T t, K k, J j ) { if( IsValid ) _trigger.Trigger( t, k, j ); }
	public void Void() { _trigger = null; }
	public void Kill() { _trigger = null; }
}
