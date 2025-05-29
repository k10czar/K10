using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using K10;
using System.Runtime.CompilerServices;


[UnityEngine.HideInInspector]
public class EventSlot : IEvent, ICustomDisposableKill
{
	bool _killed = false;
	private List<IEventTrigger> _listeners;
	private List<IEventTrigger> _callList;
	bool _callListIsDirty = false;

	public bool IsValid => !_killed;
	public int EventsCount => _listeners?.Count ?? 0;
	public int CountValidEvents => _listeners.Count( ( et ) => et.IsValid );
	public bool HasListeners => EventsCount > 0;

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Trigger()
	{
		if( _killed )
		{
			Debug.LogError( $"Error: Cannot Trigger dead EventSlot" );
			return;
		}

		if (_listeners == null) return;
		
		var count = _listeners.Count;
		if ( count <= 0 ) return;

		if (count == 1) // Only one element Events does not need callList
		{
			try
			{
				var listener = _listeners[0];
				if (listener.IsValid) listener.Trigger();
				//NOT else Trigger can invalidate listener
				if (!listener.IsValid)
				{
					//Trigger could add another element or remove itself, so we could not assume that we could clear the list or the element is in the same spot
					_listeners?.Remove(listener);
					_callListIsDirty = true;
					TryClearFullSignatureList();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return;
		}

		if (_callListIsDirty)
		{
			Lazy.RequestPoolable(ref _callList);
			for (int i = 0; i < count; i++)
			{
				if (i < _callList.Count) _callList[i] = _listeners[i];
				else _callList.Add(_listeners[i]);
			}
			for (int i = _callList.Count - 1; i >= count; i--) _callList.RemoveAt(i);
			_callListIsDirty = false;
		}

		for( int i = 0; i < count; i++ )
		{
			try
			{
				var listener = _callList[i];
				if (listener.IsValid) listener.Trigger();
				//NOT else Trigger can invalidate listener
				if (!listener.IsValid)
				{
					_listeners?.Remove(listener);
					_callListIsDirty = true;
					TryClearFullSignatureList();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void Kill()
	{
		_killed = true;
		Clear();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Clear()
	{
		_callListIsDirty = false;
		if (_listeners != null)
		{
			_listeners.Clear();
			ObjectPool.ReturnAndClearRef(ref _listeners);
		}
		if (_callList != null)
		{
			_callList.Clear();
			ObjectPool.ReturnAndClearRef(ref _callList);
		}
    }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
    private void TryClearFullSignatureList()
	{
		if (_listeners != null && _listeners.Count == 0 )
		{
			ObjectPool.ReturnAndClearRef( ref _listeners );
			if (_callList != null)
			{
				_callList.Clear();
				ObjectPool.ReturnAndClearRef( ref _callList );
			}
			_callListIsDirty = false;
		}
	}

	public void Register(IEventTrigger listener)
	{
		if (_killed || listener == null) return;
		Lazy.RequestPoolable(ref _listeners).Add(listener);
		_callListIsDirty = true;
	}

	public bool Unregister( IEventTrigger listener )
	{
		if( _killed || _listeners == null ) return false;
		bool removed = _listeners.Remove( listener );
		if (removed)
		{
			TryClearFullSignatureList();
			_callListIsDirty = true;
		}
		return removed;
	}

	public override string ToString() { return $"[EventSlot:{EventsCount}]"; }
}

[UnityEngine.HideInInspector]
public class EventSlot<T> : IEvent<T>, ICustomDisposableKill
{
	bool _killed = false;
	private EventSlot _generic;
	private List<IEventTrigger<T>> _listeners;
	private List<IEventTrigger<T>> _callList;
	bool _callListIsDirty = false;

	public bool IsValid => !_killed;
	public int EventsCount => ( ( _generic?.EventsCount ?? 0 ) + ( _listeners?.Count ?? 0 ) );
	public int CountValidEvents => ( _generic?.CountValidEvents ?? 0 ) + _listeners.Count( ( et ) => et.IsValid );
	public bool HasListeners => EventsCount > 0;

	public static implicit operator EventSlot( EventSlot<T> v ) => Lazy.Request( ref v._generic );

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Trigger( T t )
    {
        if (_killed)
        {
            Debug.LogError($"Error: Cannot Trigger dead EventSlot<{typeof(T)}>");
            return;
        }

        if (_listeners != null)
        {
            var count = _listeners.Count;
            if (count > 0)
            {
				if (count == 1) // Only one element Events does not need callList
				{
					try
					{
						var listener = _listeners[0];
						if (listener.IsValid) listener.Trigger(t);
						//NOT else Trigger can invalidate listener
						if (!listener.IsValid)
						{
							//Trigger could add another element or remove itself, so we could not assume that we could clear the list or the element is in the same spot
							_listeners?.Remove(listener);
							_callListIsDirty = true;
							TryClearFullSignatureList();
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				else
				{
					if (_callListIsDirty)
					{
						Lazy.RequestPoolable(ref _callList);
						for (int i = 0; i < count; i++)
						{
							if (i < _callList.Count) _callList[i] = _listeners[i];
							else _callList.Add(_listeners[i]);
						}
						for (int i = _callList.Count - 1; i >= count; i--) _callList.RemoveAt(i);
						_callListIsDirty = false;
					}

					for (int i = 0; i < count; i++)
					{
						try
						{
							var listener = _callList[i];
							if (listener.IsValid) listener.Trigger(t);
							//NOT else Trigger can invalidate listener
							if (!listener.IsValid)
							{
								_listeners?.Remove(listener);
								_callListIsDirty = true;
								TryClearFullSignatureList();
							}
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
            }
        }

        if (_generic != null)
        {
            _generic.Trigger();
            TryClearGeneric();
        }
    }

    public void Kill()
	{
		_killed = true;
		Clear();
	}

	public void Clear()
	{
		_callListIsDirty = false;
		if (_listeners != null)
		{
			_listeners.Clear();
			ObjectPool.ReturnAndClearRef( ref _listeners);
		}
		if (_generic != null)
		{
			_generic.Clear();
			ObjectPool.ReturnAndClearRef(ref _generic);
		}
		if (_callList != null)
		{
			_callList.Clear();
			ObjectPool.ReturnAndClearRef(ref _callList);
		}
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	private void TryClearGeneric()
	{
		if( _generic == null ) return;
		if (_generic.EventsCount == 0) ObjectPool.ReturnAndClearRef( ref _generic);
	}
	
	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	private void TryClearFullSignatureList()
	{
		if (_listeners == null) return;
		if (_listeners.Count == 0)
		{
			ObjectPool.ReturnAndClearRef( ref _listeners );
			if (_callList != null)
			{
				_callList.Clear();
				ObjectPool.ReturnAndClearRef( ref _callList);
			}
		}
	}

	public void Register( IEventTrigger<T> listener )
	{
		if( _killed || listener == null ) return;
		Lazy.RequestPoolable( ref _listeners ).Add( listener );
		_callListIsDirty = true;
	}

	public void Register( IEventTrigger listener )
	{
		if( _killed || listener == null ) return;
		Lazy.RequestPoolable( ref _generic ).Register( listener );
	}

	public bool Unregister( IEventTrigger<T> listener )
	{
		if( _killed || _listeners == null ) return false;
		bool removed = _listeners.Remove( listener );
		if (removed)
		{
			_callListIsDirty = true;
			TryClearFullSignatureList();
		}
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

[UnityEngine.HideInInspector]
public class EventSlot<T, K> : IEvent<T, K>, ICustomDisposableKill
{
	bool _killed = false;
	private EventSlot<T> _generic;
	private List<IEventTrigger<T, K>> _listeners;
	private List<IEventTrigger<T, K>> _callList;
	bool _callListIsDirty = false;

	public bool IsValid => !_killed;
	public int EventsCount => ( ( _generic?.EventsCount ?? 0 ) + ( _listeners?.Count ?? 0 ) );
	public int CountValidEvents => ( _generic?.CountValidEvents ?? 0 ) + _listeners.Count( ( et ) => et.IsValid );
	public bool HasListeners => EventsCount > 0;

	public static implicit operator EventSlot<T>( EventSlot<T, K> v ) => Lazy.Request( ref v._generic );

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Trigger( T t, K k )
    {
        if (_killed)
        {
            Debug.LogError($"Error: Cannot Trigger dead EventSlot<{typeof(T)},{typeof(K)}>");
            return;
        }

        if (_listeners != null)
        {
            var count = _listeners.Count;
            if (count > 0)
            {
				if (count == 1) // Only one element Events does not need callList
				{
					try
					{
						var listener = _listeners[0];
						if (listener.IsValid) listener.Trigger(t, k);
						//NOT else Trigger can invalidate listener
						if (!listener.IsValid)
						{
							_listeners?.Remove(listener);
							_callListIsDirty = true;
							TryClearFullSignatureList();
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				else
				{
					if (_callListIsDirty)
					{
						Lazy.RequestPoolable(ref _callList);
						for (int i = 0; i < count; i++)
						{
							if (i < _callList.Count) _callList[i] = _listeners[i];
							else _callList.Add(_listeners[i]);
						}
						for (int i = _callList.Count - 1; i >= count; i--) _callList.RemoveAt(i);
						_callListIsDirty = false;
					}

					for (int i = 0; i < count; i++)
					{
						try
						{
							var listener = _callList[i];
							if (listener.IsValid) listener.Trigger(t, k);
							//NOT else Trigger can invalidate listener
							if (!listener.IsValid)
							{
								_listeners?.Remove(listener);
								_callListIsDirty = true;
								TryClearFullSignatureList();
							}
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
            }
        }

        if (_generic != null)
        {
            _generic.Trigger(t);
            TryClearGeneric();
        }
    }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	private void TryClearGeneric()
	{
		if( _generic == null ) return;
		if (_generic.EventsCount == 0) ObjectPool.ReturnAndClearRef( ref _generic);
	}
	
	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	private void TryClearFullSignatureList()
	{
		if (_listeners == null) return;
		if (_listeners.Count == 0)
		{
			ObjectPool.ReturnAndClearRef( ref _listeners );
			if (_callList != null)
			{
				_callList.Clear();
				ObjectPool.ReturnAndClearRef( ref _callList);
			}
			_callListIsDirty = false;
		}
	}

	public void Kill()
	{
		_killed = true;
		Clear();
	}

	public void Clear()
	{
		_callListIsDirty = false;
		if (_listeners != null)
		{
			_listeners.Clear();
			ObjectPool.ReturnAndClearRef( ref _listeners);
		}
		if (_generic != null)
		{
			_generic.Clear();
			ObjectPool.ReturnAndClearRef(ref _generic);
		}
		if (_callList != null)
		{
			_callList.Clear();
			ObjectPool.ReturnAndClearRef(ref _callList);
		}
	}

	public void Register( IEventTrigger<T, K> listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _listeners ).Add( listener );
		_callListIsDirty = true;
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
		if (removed)
		{
			_callListIsDirty = true;
			TryClearFullSignatureList();
		}
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

[UnityEngine.HideInInspector]
public class EventSlot<T, K, L> : IEvent<T, K, L>, ICustomDisposableKill
{
	bool _killed = false;
	private EventSlot<T, K> _generic;
	private List<IEventTrigger<T, K, L>> _listeners;
	private List<IEventTrigger<T, K, L>> _callList;
	bool _callListIsDirty = false;

	public bool IsValid => !_killed;
	public int EventsCount => ( _generic.EventsCount + _listeners.Count );
	public int CountValidEvents => ( _generic?.CountValidEvents ?? 0 ) + _listeners.Count( ( et ) => et.IsValid );
	public bool HasListeners => EventsCount > 0;

	public static implicit operator EventSlot<T, K>( EventSlot<T, K, L> v ) => Lazy.Request( ref v._generic );

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Trigger( T t, K k, L l )
	{
		if( _killed )
		{
			Debug.LogError( $"Error: Cannot Trigger dead EventSlot<{typeof( T )},{typeof( K )},{typeof( L )}>" );
			return;
		}

		if( _listeners != null )
		{
			var count = _listeners.Count;
			if (count > 0)
			{
				if (count == 1) // Only one element Events does not need callList
				{
					try
					{
						var listener = _listeners[0];
						if (listener.IsValid) listener.Trigger(t, k, l);
						//NOT else Trigger can invalidate listener
						if (!listener.IsValid)
						{
							_listeners?.Remove(listener);
							_callListIsDirty = true;
							TryClearFullSignatureList();
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				else
				{
					if (_callListIsDirty)
					{
						Lazy.RequestPoolable(ref _callList);
						for (int i = 0; i < count; i++)
						{
							if (i < _callList.Count) _callList[i] = _listeners[i];
							else _callList.Add(_listeners[i]);
						}
						for (int i = _callList.Count - 1; i >= count; i--) _callList.RemoveAt(i);
						_callListIsDirty = false;
					}	

					for (int i = 0; i < count; i++)
					{
						try
						{
							var listener = _callList[i];
							if (listener.IsValid) listener.Trigger(t, k, l);
							//NOT else Trigger can invalidate listener
							if (!listener.IsValid)
							{
								_listeners?.Remove(listener);
								_callListIsDirty = true;
								TryClearFullSignatureList();
							}
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
			}
		}

		if( _generic != null )
		{
			_generic.Trigger( t, k );
			TryClearGeneric();
		}
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	private void TryClearGeneric()
	{
		if( _generic == null ) return;
		if (_generic.EventsCount == 0) ObjectPool.ReturnAndClearRef( ref _generic);
	}
	
	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	private void TryClearFullSignatureList()
	{
		if (_listeners == null) return;
		if (_listeners.Count == 0)
		{
			ObjectPool.ReturnAndClearRef( ref _listeners );
			if (_callList != null)
			{
				_callList.Clear();
				ObjectPool.ReturnAndClearRef( ref _callList);
			}
		}
	}

	public void Kill()
	{
		_killed = true;
		Clear();
	}

	public void Clear()
	{
		_callListIsDirty = false;
		if (_listeners != null)
		{
			_listeners.Clear();
			ObjectPool.ReturnAndClearRef( ref _listeners);
		}
		if (_generic != null)
		{
			_generic.Clear();
			ObjectPool.ReturnAndClearRef(ref _generic);
		}
		if (_callList != null)
		{
			_callList.Clear();
			ObjectPool.ReturnAndClearRef(ref _callList);
		}
	}

	public void Register( IEventTrigger<T, K, L> listener )
	{
		if( _killed || listener == null ) return;
		Lazy.Request( ref _listeners ).Add( listener );
		_callListIsDirty = true;
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
		if (removed)
		{
			_callListIsDirty = true;
			TryClearFullSignatureList();
		}
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

[UnityEngine.HideInInspector]
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

[UnityEngine.HideInInspector]
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

[UnityEngine.HideInInspector]
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

[UnityEngine.HideInInspector]
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