using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using K10;
using System.Runtime.CompilerServices;

public interface IActionEvent : IActionEventRegister, IEventTrigger { }
public interface IActionEventRegister
{
	void Register( Action action );
	bool Unregister( Action action );
}

public interface IActionEvent<T> : IActionEventRegister<T>, IEventTrigger<T> { }
public interface IActionEventRegister<T> : IActionEventRegister
{
	void Register( Action<T> action );
	bool Unregister( Action<T> action );
}

public interface IActionEvent<T,K> : IActionEventRegister<T,K>, IEventTrigger<T,K> { }
public interface IActionEventRegister<T,K> : IActionEventRegister<T>
{
	void Register( Action<T,K> action );
	bool Unregister( Action<T,K> action );
}

public interface IActionEvent<T,K,L> : IActionEventRegister<T,K,L>, IEventTrigger<T,K,L> { }
public interface IActionEventRegister<T,K,L> : IActionEventRegister<T,K>
{
	void Register( Action<T,K,L> action );
	bool Unregister( Action<T,K,L> action );
}

[HideInInspector]
public class ActionEventSlot : IActionEvent
{
	bool _killed = false;
	private List<Action> _actions;

	public bool IsValid => !_killed;

	public ActionEventSlot() { }

	public ActionEventSlot(int provision)
	{
		if (provision > 0) _actions = new(provision);
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Trigger()
	{
		if( _killed )
		{
#if UNITY_EDITOR
			Debug.LogError( $"Error: Cannot Trigger dead ActionEventSlot" );
#endif
			return;
		}

		if (_actions == null) return;
		
		var count = _actions.Count;
		for (int i = 0; i < count; i++)
		{
			try
			{
				_actions[i].Invoke();
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
		_actions?.Clear();
    }

    public void Register(Action listener)
	{
		if (_killed || listener == null) return;
		if (_actions == null) _actions = new();
		_actions.Add(listener);
	}

	public bool Unregister( Action listener )
	{
		if( _killed || _actions == null ) return false;
		bool removed = _actions.Remove( listener );
		return removed;
	}

	public override string ToString() { return $"[ActionEventSlot:{_actions.ToStringOrNull()}]"; }
	
#if UNITY_EDITOR
	public string EDITOR_LogProvision( string identifier )
	{
		return $"<color=yellow>{identifier}:</color>._listeners:{_actions?.Count.ToString() ?? "NULL"}";
	}
#endif
}

[UnityEngine.HideInInspector]
public class ActionEventSlot<T> : IActionEvent<T>
{
	bool _killed = false;
	private ActionEventSlot _generic;
	private List<Action<T>> _actions;
	
	public bool IsValid => !_killed;


	public static implicit operator ActionEventSlot(ActionEventSlot<T> v) => v._generic ??= new();// Lazy.Request( ref v._generic );

	public ActionEventSlot() { }

	public ActionEventSlot(int provision, int genericProvision)
	{
		if (provision > 0) _actions = new(provision);
		if (genericProvision > 0) _generic = new(genericProvision);
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Trigger(T t)
	{
		if (_killed)
		{
#if UNITY_EDITOR
			Debug.LogError($"Error: Cannot Trigger dead ActionEventSlot<{typeof(T)}>");
#endif
			return;
		}

		if (_actions != null)
		{
			var count = _actions.Count;
			for (int i = 0; i < count; i++)
			{
				try
				{
					_actions[i].Invoke( t );
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		if (_generic != null)
		{
			_generic.Trigger();
		}
	}

	public void Kill()
	{
		_killed = true;
		Clear();
		_generic?.Kill();
	}

	public void Clear()
	{
		_actions?.Clear();
		_generic?.Clear();
	}

	public void Register(Action<T> listener)
	{
		if (_killed || listener == null) return;
		if (_actions == null) _actions = new();
		_actions.Add(listener);
	}

	public void Register(Action listener)
	{
		if (_killed || listener == null) return;
		if (_generic == null) _generic = new();
		_generic.Register(listener);
	}

	public bool Unregister(Action<T> listener)
	{
		if (_killed || _actions == null) return false;
		bool removed = _actions.Remove(listener);
		return removed;
	}

	public bool Unregister(Action listener)
	{
		if (_killed || _generic == null) return false;
		bool removed = _generic.Unregister(listener);
		return removed;
	}

	public override string ToString() { return $"[ActionEventSlot<{typeof(T)}>:{_actions.ToStringOrNull()}, Generic:{_generic.ToStringOrNull()}]"; }

#if UNITY_EDITOR
	public string EDITOR_LogProvision( string identifier )
	{
		return $"<color=magenta>{identifier}:</color>._listeners:{_actions?.Count.ToString() ?? "NULL"}|{_generic?.EDITOR_LogProvision("_generic") ?? "NULL"}";
	}
#endif
}

[UnityEngine.HideInInspector]
public class ActionEventSlot<T, K> : IActionEvent<T,K>
{
	bool _killed = false;
	private ActionEventSlot<T> _generic;
	private List<Action<T, K>> _actions;
	
	public bool IsValid => !_killed;

	public static implicit operator ActionEventSlot<T>(ActionEventSlot<T, K> v) => v._generic ??= new();

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Trigger( T t, K k )
    {
        if (_killed)
        {
#if UNITY_EDITOR
            Debug.LogError($"Error: Cannot Trigger dead ActionEventSlot<{typeof(T)},{typeof(K)}>");
#endif
            return;
        }

		if (_actions != null)
		{
			var count = _actions.Count;
			for (int i = 0; i < count; i++)
			{
				try
				{
					_actions[i].Invoke( t, k );
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		if (_generic != null)
		{
			_generic.Trigger( t );
		}
    }

	public void Kill()
	{
		_killed = true;
		Clear();
		_generic?.Kill();
	}

	public void Clear()
	{
		_actions?.Clear();
		_generic?.Clear();
	}

	public void Register( Action<T, K> listener )
	{
		if( _killed || listener == null ) return;
		if (_actions == null) _actions = new();
		_actions.Add( listener );
	}

	public void Register( Action<T> listener )
	{
		if( _killed || listener == null ) return;
		if (_generic == null) _generic = new();
		_generic.Register(listener);
	}

	public void Register( Action listener )
	{
		if( _killed || listener == null ) return;
		if (_generic == null) _generic = new();
		_generic.Register(listener);
	}

	public bool Unregister( Action<T, K> listener )
	{
		if( _killed || _actions == null ) return false;
		bool removed = _actions.Remove( listener );
		return removed;
	}

	public bool Unregister( Action<T> listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		return removed;
	}

	public bool Unregister( Action listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		return removed;
	}

	public override string ToString() { return $"[ActionEventSlot<{typeof(T)},{typeof(K)}>:{_actions.ToStringOrNull()}, Generic:{_generic.ToStringOrNull()}]"; }

#if UNITY_EDITOR
	public string EDITOR_LogSizeData( string identifier )
	{
		return $"<color=magenta>{identifier}:</color> _listeners:{_actions?.Count.ToString() ?? "NULL"}|{_generic?.EDITOR_LogProvision("generic<{nameof(T)}>") ?? "NULL"}";
	}
#endif
}

[UnityEngine.HideInInspector]
public class ActionEventSlot<T, K, L>: IActionEvent<T,K,L>
{
	bool _killed = false;
	private ActionEventSlot<T, K> _generic;
	private List<Action<T, K, L>> _actions;
	
	public bool IsValid => !_killed;

	public static implicit operator ActionEventSlot<T, K>(ActionEventSlot<T, K, L> v) => v._generic ??= new();

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Trigger( T t, K k, L l )
	{
		if( _killed )
		{
#if UNITY_EDITOR
			Debug.LogError( $"Error: Cannot Trigger dead ActionEventSlot<{typeof( T )},{typeof( K )},{typeof( L )}>" );
#endif
			return;
		}

		if (_actions != null)
		{
			var count = _actions.Count;
			for (int i = 0; i < count; i++)
			{
				try
				{
					_actions[i].Invoke( t, k, l );
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		if (_generic != null)
		{
			_generic.Trigger( t, k );
		}
	}

	public void Kill()
	{
		_killed = true;
		Clear();
		_generic?.Kill();
	}

	public void Clear()
	{
		_actions?.Clear();
		_generic?.Clear();
	}

	public void Register( Action<T, K, L> listener )
	{
		if( _killed || listener == null ) return;
		if (_actions == null) _actions = new();
		_actions.Add( listener );
	}

	public void Register( Action<T, K> listener )
	{
		if( _killed || listener == null ) return;
		if (_generic == null) _generic = new();
		_generic.Register(listener);
	}

	public void Register( Action<T> listener )
	{
		if( _killed || listener == null ) return;
		if (_generic == null) _generic = new();
		_generic.Register(listener);
	}

	public void Register( Action listener )
	{
		if( _killed || listener == null ) return;
		if (_generic == null) _generic = new();
		_generic.Register(listener);
	}

	public bool Unregister( Action<T, K, L> listener )
	{
		if( _killed || _actions == null ) return false;
		bool removed = _actions.Remove( listener );
		return removed;
	}

	public bool Unregister( Action<T, K> listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		return removed;
	}

	public bool Unregister( Action<T> listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		return removed;
	}

	public bool Unregister( Action listener )
	{
		if( _killed || _generic == null ) return false;
		bool removed = _generic.Unregister( listener );
		return removed;
	}

	public override string ToString() { return $"[ActionEventSlot<{typeof(T)},{typeof(K)},{typeof(L)}>:{_actions.ToStringOrNull()}, Generic:{_generic.ToStringOrNull()}]"; }
}