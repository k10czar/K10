using System;
using UnityEngine;

public class ActionCapsule : IEventTrigger, IEquatable<ActionCapsule>, IVoidable
{
	private readonly Action callback;
	private readonly IEventRegister observed;

	public ActionCapsule(Action callback)
	{
		this.callback = callback;
		this.observed = null;

		IsValid = callback != null;
	}

	public ActionCapsule(Action callback, IEventRegister observed)
	{
		this.callback = callback;
		this.observed = observed;

		IsValid = callback != null;
		if (IsValid) observed.Register(this);
	}

	[HideInCallstack]
	public void Trigger() => callback();
	public bool IsValid { get; private set; }

	public void Void()
	{
		if (IsValid) observed?.Unregister(this);
		IsValid = false;
	}

	public override bool Equals(object other)
	{
		if (other == null) return false;
		if (GetHashCode() != other.GetHashCode()) return false;

		if (other is ActionCapsule cap)
			return callback?.Equals(cap.callback) ?? cap.callback == null;

		return false;
	}

	public bool Equals(ActionCapsule other)
	{
		if (GetHashCode() != other?.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode() => callback?.GetHashCode() ?? 0;
}

public class ActionCapsule<T> : IEventTrigger<T>, IEquatable<ActionCapsule<T>>, IVoidable
{
	private readonly Action<T> callback;
	private readonly IEventRegister<T> observed;

	public ActionCapsule(Action<T> callback)
	{
		this.callback = callback;
		this.observed = null;

		IsValid = callback != null;
	}

	public ActionCapsule(Action<T> callback, IEventRegister<T> observed)
	{
		this.callback = callback;
		this.observed = observed;

		IsValid = callback != null;
		if (IsValid) observed.Register(this);
	}

	[HideInCallstack]
	public void Trigger(T t) => callback(t);
	public bool IsValid { get; private set; }

	public void Void()
	{
		if (IsValid) observed?.Unregister(this);
		IsValid = false;
	}

	public override bool Equals(object other)
	{
		if (other == null) return false;
		if (GetHashCode() != other.GetHashCode()) return false;

		if (other is ActionCapsule<T> cap)
			return callback?.Equals(cap.callback) ?? cap.callback == null;

		return false;
	}

	public bool Equals(ActionCapsule<T> other)
	{
		if (GetHashCode() != other?.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode() => callback?.GetHashCode() ?? 0;
}

public class ActionCapsule<T, K> : IEventTrigger<T, K>, IEquatable<ActionCapsule<T, K>>, IVoidable
{
	private readonly Action<T, K> callback;
	private readonly IEventRegister<T, K> observed;

	public ActionCapsule(Action<T, K> callback)
	{
		this.callback = callback;
		this.observed = null;

		IsValid = callback != null;
	}

	public ActionCapsule(Action<T, K> callback, IEventRegister<T, K> observed)
	{
		this.callback = callback;
		this.observed = observed;

		IsValid = callback != null;
		if (IsValid) observed.Register(this);
	}

	[HideInCallstack]
	public void Trigger(T t, K k) => callback(t, k);
	public bool IsValid { get; private set; }

	public void Void()
	{
		if (IsValid) observed?.Unregister(this);
		IsValid = false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null) return false;
		if (GetHashCode() != obj.GetHashCode()) return false;
		if (obj is ActionCapsule<T, K> cap)
		{
			if (callback != null) return callback.Equals(cap.callback);
			return cap.callback.Equals(null);
		}

		return false;
	}

	public bool Equals(ActionCapsule<T, K> other)
	{
		if (GetHashCode() != other?.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode()
	{
		return callback?.GetHashCode() ?? 0;
	}
}

public class ActionCapsule<T, K, L> : IEventTrigger<T, K, L>, IEquatable<ActionCapsule<T, K, L>>, IVoidable
{
	private readonly Action<T, K, L> callback;
	private readonly IEventRegister<T, K, L> observed;

	public ActionCapsule(Action<T, K, L> callback)
	{
		this.callback = callback;
		this.observed = null;

		IsValid = callback != null;
	}

	public ActionCapsule(Action<T, K, L> callback, IEventRegister<T, K, L> observed)
	{
		this.callback = callback;
		this.observed = observed;

		IsValid = callback != null;
		if (IsValid) observed.Register(this);
	}

	[HideInCallstack]
	public void Trigger(T t, K k, L l) => callback(t, k, l);
	public bool IsValid { get; private set; }

	public void Void()
	{
		if (IsValid) observed?.Unregister(this);
		IsValid = false;
	}

	public override bool Equals(object other)
	{
		if (other == null) return false;
		if (GetHashCode() != other.GetHashCode()) return false;

		if (other is ActionCapsule<T, K, L> cap)
			return callback?.Equals(cap.callback) ?? cap.callback == null;

		return false;
	}

	public bool Equals(ActionCapsule<T, K, L> other)
	{
		if (GetHashCode() != other?.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode() => callback?.GetHashCode() ?? 0;
}