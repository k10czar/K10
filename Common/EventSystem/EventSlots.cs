using UnityEngine;
using System.Collections.Generic;
using System;
using K10;

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

		var listenersCopy = ObjectPool<List<IEventTrigger>>.Request();
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
		ObjectPool<List<IEventTrigger>>.Return(listenersCopy);
	}

	public void Kill()
	{
		killed = true;
		Clear();
	}

	public void Clear()
	{
		ObjectPool<List<IEventTrigger>>.Return(listeners);
		listeners = null;
	}

	private void TryReleaseListeners()
	{
		if (listeners == null || listeners.Count != 0) return;
		Clear();
	}

	public void Register(IEventTrigger listener)
	{
		if (killed || listener == null) return;

		listeners ??= ObjectPool<List<IEventTrigger>>.Request();
		listeners.Add(listener);
	}

	public bool Unregister(IEventTrigger listener)
	{
		if (killed || listeners == null) return false;

		var removed = listeners.Remove(listener);
		if (removed) TryReleaseListeners();

		return removed;
	}

	public override string ToString() => $"[EventSlot:{EventsCount}]";
}

public class EventSlot<T> : IEvent<T>, ICustomDisposableKill
{
	private bool killed;

	private EventSlot generic;
	private List<IEventTrigger<T>> listeners;

	public bool IsValid => !killed;
	public int EventsCount => (generic?.EventsCount ?? 0) + (listeners?.Count ?? 0);
	public bool HasListeners => EventsCount > 0;

	private bool ThisHasListeners => listeners != null && listeners.Count > 0;

	public static implicit operator EventSlot(EventSlot<T> v) => Lazy.Request(ref v.generic);

	public void Trigger(T t)
	{
		if (killed)
		{
			Debug.LogError($"Error: Cannot Trigger dead EventSlot<{typeof(T)}>");
			return;
		}

		if (ThisHasListeners)
		{
			var listenersCopy = ObjectPool<List<IEventTrigger<T>>>.Request();
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
			ObjectPool<List<IEventTrigger<T>>>.Return(listenersCopy);
		}

		if (generic != null)
		{
			generic.Trigger();
			TryClearGeneric();
		}
	}

	public void Kill()
	{
		killed = true;
		Clear();
	}

	public void Clear()
	{
		generic?.Clear();
		generic = null;

		ReleaseListeners();
	}

	private void TryReleaseListeners()
	{
		if (listeners == null || listeners.Count != 0) return;
		ReleaseListeners();
	}

	private void ReleaseListeners()
	{
		if (listeners == null) return;

		ObjectPool<List<IEventTrigger<T>>>.Return(listeners);
		listeners = null;
	}

	private void TryClearGeneric()
	{
		if (generic == null) return;
		if (generic.EventsCount == 0) generic = null;
	}

	#region Register Interface

	public void Register(IEventTrigger<T> listener)
	{
		if (killed || listener == null) return;

		listeners ??= ObjectPool<List<IEventTrigger<T>>>.Request();
		listeners.Add(listener);
	}

	public void Register(IEventTrigger listener)
	{
		if (killed || listener == null) return;
		Lazy.Request(ref generic).Register(listener);
	}

	public bool Unregister(IEventTrigger<T> listener)
	{
		if (killed || listeners == null) return false;

		var removed = listeners.Remove(listener);
		if (removed) TryReleaseListeners();

		return removed;
	}

	public bool Unregister(IEventTrigger listener)
	{
		if (killed || generic == null) return false;

		var removed = generic.Unregister(listener);
		if (removed) TryClearGeneric();

		return removed;
	}

	#endregion

	public override string ToString() => $"[EventSlot<{typeof(T)}>:{listeners?.Count ?? 0}, Generic:{generic.ToStringOrNull()}]";
}

public class EventSlot<T, K> : IEvent<T, K>, ICustomDisposableKill
{
	private bool killed;

	private EventSlot<T> generic;
	private List<IEventTrigger<T, K>> listeners;

	public bool IsValid => !killed;
	public int EventsCount => (generic?.EventsCount ?? 0) + (listeners?.Count ?? 0);
	public bool HasListeners => EventsCount > 0;

	private bool ThisHasListeners => listeners != null && listeners.Count > 0;

	public static implicit operator EventSlot<T>(EventSlot<T, K> v) => Lazy.Request(ref v.generic);

	public void Trigger(T t, K k)
	{
		if (killed)
		{
			Debug.LogError($"Error: Cannot Trigger dead EventSlot<{typeof(T)},{typeof(K)}>");
			return;
		}

		if (ThisHasListeners)
		{
			var listenersCopy = ObjectPool<List<IEventTrigger<T, K>>>.Request();
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
			ObjectPool<List<IEventTrigger<T, K>>>.Return(listenersCopy);
		}

		if (generic != null)
		{
			generic.Trigger(t);
			TryClearGeneric();
		}
	}

	public void Kill()
	{
		killed = true;
		Clear();
	}

	public void Clear()
	{
		generic?.Clear();
		generic = null;

		ReleaseListeners();
	}

	private void TryReleaseListeners()
	{
		if (listeners == null || listeners.Count != 0) return;
		ReleaseListeners();
	}

	private void ReleaseListeners()
	{
		if (listeners == null) return;

		ObjectPool<List<IEventTrigger<T,K>>>.Return(listeners);
		listeners = null;
	}

	private void TryClearGeneric()
	{
		if (generic == null) return;
		if (generic.EventsCount == 0) generic = null;
	}

	#region Register Interface

	public void Register(IEventTrigger<T, K> listener)
	{
		if (killed || listener == null) return;

		listeners ??= ObjectPool<List<IEventTrigger<T, K>>>.Request();
		listeners.Add(listener);
	}

	public void Register(IEventTrigger<T> listener)
	{
		if (killed || listener == null) return;
		Lazy.Request(ref generic).Register(listener);
	}

	public void Register(IEventTrigger listener)
	{
		if (killed || listener == null) return;
		Lazy.Request(ref generic).Register(listener);
	}

	public bool Unregister(IEventTrigger<T, K> listener)
	{
		if (killed || listeners == null) return false;
		bool removed = listeners.Remove(listener);
		if (removed) TryReleaseListeners();
		return removed;
	}

	public bool Unregister(IEventTrigger<T> listener)
	{
		if (killed || generic == null) return false;
		bool removed = generic.Unregister(listener);
		if (removed) TryClearGeneric();
		return removed;
	}

	public bool Unregister(IEventTrigger listener)
	{
		if (killed || generic == null) return false;
		bool removed = generic.Unregister(listener);
		if (removed) TryClearGeneric();
		return removed;
	}

	#endregion

	public override string ToString() => $"[EventSlot<{typeof(T)},{typeof(K)}>:{listeners?.Count ?? 0}, Generic:{generic.ToStringOrNull()}]";
}

public class EventSlot<T, K, L> : IEvent<T, K, L>, ICustomDisposableKill
{
	private bool killed;

	private EventSlot<T, K> generic;
	private List<IEventTrigger<T, K, L>> listeners;

	public bool IsValid => !killed;
	public int EventsCount => (generic.EventsCount + listeners.Count);
	public bool HasListeners => EventsCount > 0;

	private bool ThisHasListeners => listeners != null && listeners.Count > 0;

	public static implicit operator EventSlot<T, K>(EventSlot<T, K, L> v) => Lazy.Request(ref v.generic);

	public void Trigger(T t, K k, L l)
	{
		if (killed)
		{
			Debug.LogError($"Error: Cannot Trigger dead EventSlot<{typeof(T)},{typeof(K)},{typeof(L)}>");
			return;
		}

		if (ThisHasListeners)
		{
			var listenersCopy = ObjectPool<List<IEventTrigger<T, K, L>>>.Request();
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
			ObjectPool<List<IEventTrigger<T, K, L>>>.Return(listenersCopy);
		}

		if (generic != null)
		{
			generic.Trigger(t, k);
			TryClearGeneric();
		}
	}

	public void Kill()
	{
		killed = true;
		Clear();
	}

	public void Clear()
	{
		generic?.Clear();
		generic = null;

		ReleaseListeners();
	}

	private void TryReleaseListeners()
	{
		if (listeners == null || listeners.Count != 0) return;
		ReleaseListeners();
	}

	private void ReleaseListeners()
	{
		if (listeners == null) return;

		ObjectPool<List<IEventTrigger<T,K,L>>>.Return(listeners);
		listeners = null;
	}

	private void TryClearGeneric()
	{
		if (generic == null) return;
		if (generic.EventsCount == 0) generic = null;
	}

	#region Register Interface

	public void Register(IEventTrigger<T, K, L> listener)
	{
		if (killed || listener == null) return;

		listeners ??= ObjectPool<List<IEventTrigger<T, K, L>>>.Request();
		listeners.Add(listener);
	}

	public void Register(IEventTrigger<T, K> listener)
	{
		if (killed || listener == null) return;
		Lazy.Request(ref generic).Register(listener);
	}

	public void Register(IEventTrigger<T> listener)
	{
		if (killed || listener == null) return;
		Lazy.Request(ref generic).Register(listener);
	}

	public void Register(IEventTrigger listener)
	{
		if (killed || listener == null) return;
		Lazy.Request(ref generic).Register(listener);
	}

	public bool Unregister(IEventTrigger<T, K, L> listener)
	{
		if (killed || listeners == null) return false;

		var removed = listeners.Remove(listener);
		if (removed) TryReleaseListeners();

		return removed;
	}

	public bool Unregister(IEventTrigger<T, K> listener)
	{
		if (killed || generic == null) return false;

		var removed = generic.Unregister(listener);
		if (removed) TryClearGeneric();

		return removed;
	}

	public bool Unregister(IEventTrigger<T> listener)
	{
		if (killed || generic == null) return false;

		var removed = generic.Unregister(listener);
		if (removed) TryClearGeneric();

		return removed;
	}

	public bool Unregister(IEventTrigger listener)
	{
		if (killed || generic == null) return false;

		var removed = generic.Unregister(listener);
		if (removed) TryClearGeneric();

		return removed;
	}

	#endregion

	public override string ToString() => $"[EventSlot<{typeof(T)},{typeof(K)},{typeof(L)}>:{listeners?.Count ?? 0}, Generic:{generic.ToStringOrNull()}]";
}