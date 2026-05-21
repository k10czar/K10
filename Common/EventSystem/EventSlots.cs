using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Pool;

public class EventSlot : IEvent, ICustomDisposableKill
{
	private bool killed = false;
	private List<IEventTrigger> listeners;

	public bool IsValid => !killed;
	public int EventsCount => listeners?.Count ?? 0;
	public bool HasListeners => EventsCount > 0;

	public void Trigger()
	{
		if (killed)
		{
			Debug.LogError($"Error: Cannot Trigger dead EventSlot");
			return;
		}

		if (!HasListeners) return;

		var listenersCopy = ListPool<IEventTrigger>.Get();
		listenersCopy.AddRange(listeners);

		var removedEntries = false;
		foreach (var listener in listenersCopy)
		{
			try
			{
				if (listener.IsValid) listener.Trigger();

				if (!listener.IsValid) // Trigger can invalidate listener
				{
					listeners?.Remove(listener);
					removedEntries = true;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		if (removedEntries) TryReleaseListeners();
		ListPool<IEventTrigger>.Release(listenersCopy);
	}

	public void Kill()
	{
		killed = true;
		Clear();
	}

	public void Clear()
	{
		if (listeners == null) return;

		ListPool<IEventTrigger>.Release(listeners);
		listeners = null;
	}

	private void TryReleaseListeners()
	{
		if (listeners is not { Count: 0 }) return;
		Clear();
	}

	#region Register / Unregister Interface

	public void Register(IEventTrigger listener)
	{
		if (killed || listener == null) return;

		listeners ??= ListPool<IEventTrigger>.Get();
		listeners.Add(listener);
	}

	public bool Unregister(IEventTrigger listener)
	{
		if (killed || listeners == null) return false;

		var removed = listeners.Remove(listener);
		if (removed) TryReleaseListeners();

		return removed;
	}

	public void Register(Action act) => _ = new ActionCapsule(act, this);
	public bool Unregister(Action act) => Unregister(new ActionCapsule(act, true));

	#endregion

	public override string ToString() => $"[EventSlot:{EventsCount}]";
}

public class EventSlot<T> : IEvent<T>, ICustomDisposableKill
{
	private bool killed;

	private List<IEventTrigger<T>> listeners;

	public bool IsValid => !killed;
	public int EventsCount => listeners?.Count ?? 0;
	public bool HasListeners => EventsCount > 0;

	public void Trigger(T t)
	{
		if (killed)
		{
			Debug.LogError($"Error: Cannot Trigger dead EventSlot<{typeof(T)}>");
			return;
		}

		if (!HasListeners) return;

		var listenersCopy = ListPool<IEventTrigger<T>>.Get();
		listenersCopy.AddRange(listeners);

		var removedEntries = false;
		foreach (var listener in listenersCopy)
		{
			try
			{
				if (listener.IsValid) listener.Trigger(t);

				if (!listener.IsValid) // Trigger can invalidate listener
				{
					listeners?.Remove(listener);
					removedEntries = true;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		if (removedEntries) TryReleaseListeners();
		ListPool<IEventTrigger<T>>.Release(listenersCopy);
	}

	public void Kill()
	{
		killed = true;
		Clear();
	}

	public void Clear()
	{
		if (listeners == null) return;

		ListPool<IEventTrigger<T>>.Release(listeners);
		listeners = null;
	}

	private void TryReleaseListeners()
	{
		if (listeners is not { Count: 0 }) return;
		Clear();
	}

	#region Register / Unregister Interface

	public void Register(IEventTrigger<T> listener)
	{
		if (killed || listener == null) return;

		listeners ??= ListPool<IEventTrigger<T>>.Get();
		listeners.Add(listener);
	}

	public void Register(IEventTrigger listener)
	{
		if (killed || listener == null) return;
		Register(listener.Trigger);
	}

	public bool Unregister(IEventTrigger<T> listener)
	{
		if (killed || listeners == null) return false;

		var removed = listeners.Remove(listener);
		if (removed) TryReleaseListeners();

		return removed;
	}

	public bool Unregister(IEventTrigger listener) => !killed && Unregister(listener.Trigger);

	public void Register(Action act) => _ = new ActionCapsule<T>(act, this);
	public void Register(Action<T> act) => _ = new ActionCapsule<T>(act, this);

	public bool Unregister(Action act) => Unregister(new ActionCapsule<T>(act, true));
	public bool Unregister(Action<T> act) => Unregister(new ActionCapsule<T>(act, true));

	#endregion

	public override string ToString() => $"[EventSlot<{typeof(T)}>:{EventsCount}]";
}

public class EventSlot<T,K> : IEvent<T,K>, ICustomDisposableKill
{
	private bool killed;

	private List<IEventTrigger<T,K>> listeners;

	public bool IsValid => !killed;
	public int EventsCount => listeners?.Count ?? 0;
	public bool HasListeners => EventsCount > 0;

	public void Trigger(T t, K k)
	{
		if (killed)
		{
			Debug.LogError($"Error: Cannot Trigger dead EventSlot<{typeof(T)},{typeof(K)}>");
			return;
		}

		if (!HasListeners) return;

		var listenersCopy = ListPool<IEventTrigger<T,K>>.Get();
		listenersCopy.AddRange(listeners);

		var removedEntries = false;
		foreach (var listener in listenersCopy)
		{
			try
			{
				if (listener.IsValid) listener.Trigger(t, k);

				if (!listener.IsValid) // Trigger can invalidate listener
				{
					listeners?.Remove(listener);
					removedEntries = true;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		if (removedEntries) TryReleaseListeners();
		ListPool<IEventTrigger<T,K>>.Release(listenersCopy);
	}

	public void Kill()
	{
		killed = true;
		Clear();
	}

	public void Clear()
	{
		if (listeners == null) return;

		ListPool<IEventTrigger<T,K>>.Release(listeners);
		listeners = null;
	}

	private void TryReleaseListeners()
	{
		if (listeners is not { Count: 0 }) return;
		Clear();
	}

	#region Register / Unregister Interface

	public void Register(IEventTrigger<T,K> listener)
	{
		if (killed || listener == null) return;

		listeners ??= ListPool<IEventTrigger<T,K>>.Get();
		listeners.Add(listener);
	}

	public bool Unregister(IEventTrigger<T,K> listener)
	{
		if (killed || listeners == null) return false;

		var removed = listeners.Remove(listener);
		if (removed) TryReleaseListeners();

		return removed;
	}

	public void Register(IEventTrigger<T> listener)
	{
		if (killed || listener == null) return;
		Register(listener.Trigger);
	}

	public void Register(IEventTrigger listener)
	{
		if (killed || listener == null) return;
		Register(listener.Trigger);
	}

	public bool Unregister(IEventTrigger<T> listener) => !killed && Unregister(listener.Trigger);
	public bool Unregister(IEventTrigger listener) => !killed && Unregister(listener.Trigger);

	public void Register(Action act) => _ = new ActionCapsule<T,K>(act, this);
	public void Register(Action<T> act) => _ = new ActionCapsule<T,K>(act, this);
	public void Register(Action<K> act) => _ = new ActionCapsule<T,K>(act, this);
	public void Register(Action<T,K> act) => _ = new ActionCapsule<T,K>(act, this);

	public bool Unregister(Action act) => Unregister(new ActionCapsule<T,K>(act, true));
	public bool Unregister(Action<T> act) => Unregister(new ActionCapsule<T,K>(act, true));
	public bool Unregister(Action<K> act) => Unregister(new ActionCapsule<T,K>(act, true));
	public bool Unregister(Action<T,K> act) => Unregister(new ActionCapsule<T,K>(act, true));

	#endregion

	public override string ToString() => $"[EventSlot<{typeof(T)},{typeof(K)}>:{EventsCount}]";
}

public class EventSlot<T, K, L> : IEvent<T, K, L>, ICustomDisposableKill
{
	private bool killed;

	private List<IEventTrigger<T, K, L>> listeners;

	public bool IsValid => !killed;
	public int EventsCount => listeners?.Count ?? 0;
	public bool HasListeners => EventsCount > 0;

	public void Trigger(T t, K k, L l)
	{
		if (killed)
		{
			Debug.LogError($"Error: Cannot Trigger dead EventSlot<{typeof(T)},{typeof(K)},{typeof(L)}>");
			return;
		}

		if (!HasListeners) return;

		var listenersCopy = ListPool<IEventTrigger<T,K,L>>.Get();
		listenersCopy.AddRange(listeners);

		var removedEntries = false;
		foreach (var listener in listenersCopy)
		{
			try
			{
				if (listener.IsValid) listener.Trigger(t, k, l);

				if (!listener.IsValid) // Trigger can invalidate listener
				{
					listeners?.Remove(listener);
					removedEntries = true;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		if (removedEntries) TryReleaseListeners();
		ListPool<IEventTrigger<T,K,L>>.Release(listenersCopy);
	}

	public void Kill()
	{
		killed = true;
		Clear();
	}

	public void Clear()
	{
		if (listeners == null) return;

		ListPool<IEventTrigger<T,K,L>>.Release(listeners);
		listeners = null;
	}

	private void TryReleaseListeners()
	{
		if (listeners is not { Count: 0 }) return;
		Clear();
	}

	#region Register / Unregister Interface

	public void Register(IEventTrigger<T, K, L> listener)
	{
		if (killed || listener == null) return;

		listeners ??= ListPool<IEventTrigger<T,K,L>>.Get();
		listeners.Add(listener);
	}

	public bool Unregister(IEventTrigger<T, K, L> listener)
	{
		if (killed || listeners == null) return false;

		var removed = listeners.Remove(listener);
		if (removed) TryReleaseListeners();

		return removed;
	}

	public void Register(IEventTrigger<T,K> listener)
	{
		if (killed || listener == null) return;
		Register(listener.Trigger);
	}

	public void Register(IEventTrigger<T> listener)
	{
		if (killed || listener == null) return;
		Register(listener.Trigger);
	}

	public void Register(IEventTrigger listener)
	{
		if (killed || listener == null) return;
		Register(listener.Trigger);
	}

	public bool Unregister(IEventTrigger<T,K> listener) => !killed && Unregister(listener.Trigger);
	public bool Unregister(IEventTrigger<T> listener) => !killed && Unregister(listener.Trigger);
	public bool Unregister(IEventTrigger listener) => !killed && Unregister(listener.Trigger);

	public void Register(Action act) => _ = new ActionCapsule<T,K,L>(act, this);
	public void Register(Action<T> act) => _ = new ActionCapsule<T,K,L>(act, this);
	public void Register(Action<K> act) => _ = new ActionCapsule<T,K,L>(act, this);
	public void Register(Action<L> act) => _ = new ActionCapsule<T,K,L>(act, this);
	public void Register(Action<T,K> act) => _ = new ActionCapsule<T,K,L>(act, this);
	public void Register(Action<T,L> act) => _ = new ActionCapsule<T,K,L>(act, this);
	public void Register(Action<K,L> act) => _ = new ActionCapsule<T,K,L>(act, this);

	public bool Unregister(Action act) => Unregister(new ActionCapsule<T,K,L>(act, true));
	public bool Unregister(Action<T> act) => Unregister(new ActionCapsule<T,K,L>(act, true));
	public bool Unregister(Action<K> act) => Unregister(new ActionCapsule<T,K,L>(act, true));
	public bool Unregister(Action<L> act) => Unregister(new ActionCapsule<T,K,L>(act, true));
	public bool Unregister(Action<T,K> act) => Unregister(new ActionCapsule<T,K,L>(act, true));
	public bool Unregister(Action<T,L> act) => Unregister(new ActionCapsule<T,K,L>(act, true));
	public bool Unregister(Action<K,L> act) => Unregister(new ActionCapsule<T,K,L>(act, true));

	#endregion

	public override string ToString() => $"[EventSlot<{typeof(T)},{typeof(K)},{typeof(L)}>:{EventsCount}]";
}