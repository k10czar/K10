using System;
using UnityEngine;

public struct ActionCapsule : IEventTrigger, IEquatable<ActionCapsule>
{
	private readonly Action callback;

	public ActionCapsule(Action callback)
	{
		this.callback = callback;
		IsValid = callback != null;
	}

	[HideInCallstack]
	public void Trigger() => callback();
	public bool IsValid { get; private set; }
	public void Void() => IsValid = false;

	public override bool Equals(object other)
	{
		if (other == null) return false;
		if (GetHashCode() != other.GetHashCode()) return false;

		if (other is ActionCapsule cap)
			return callback?.Equals(cap.callback) ?? cap.callback == null;

		return base.Equals(other);
	}

	public bool Equals(ActionCapsule other)
	{
		if (GetHashCode() != other.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode() => callback?.GetHashCode() ?? 0;
}

public struct ActionCapsule<T> : IEventTrigger<T>, IEquatable<ActionCapsule<T>>
{
	private readonly Action<T> callback;

	public ActionCapsule(Action<T> callback)
	{
		this.callback = callback;
		IsValid = callback != null;
	}

	[HideInCallstack]
	public void Trigger(T t) => callback(t);
	public bool IsValid { get; private set; }
	public void Void() => IsValid = false;

	public override bool Equals(object other)
	{
		if (other == null) return false;
		if (GetHashCode() != other.GetHashCode()) return false;

		if (other is ActionCapsule<T> cap)
			return callback?.Equals(cap.callback) ?? cap.callback == null;

		return base.Equals(other);
	}

	public bool Equals(ActionCapsule<T> other)
	{
		if (GetHashCode() != other.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode() => callback?.GetHashCode() ?? 0;
}

public struct ActionCapsule<T, K> : IEventTrigger<T, K>, IEquatable<ActionCapsule<T, K>>
{
	private readonly Action<T, K> callback;

	public ActionCapsule(Action<T, K> callback)
	{
		this.callback = callback;
		IsValid = callback != null;
	}

	[HideInCallstack]
	public void Trigger(T t, K k) => callback(t, k);
	public bool IsValid { get; private set; }
	public void Void() => IsValid = false;

	public override bool Equals(object obj)
	{
		if (obj == null) return false;
		if (GetHashCode() != obj.GetHashCode()) return false;
		if (obj is ActionCapsule<T, K> cap)
		{
			if (callback != null) return callback.Equals(cap.callback);
			return cap.callback.Equals(null);
		}

		return base.Equals(obj);
	}

	public bool Equals(ActionCapsule<T, K> other)
	{
		if (GetHashCode() != other.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode()
	{
		return callback?.GetHashCode() ?? 0;
	}
}

public struct ActionCapsule<T, K, L> : IEventTrigger<T, K, L>, IEquatable<ActionCapsule<T, K, L>>
{
	private readonly Action<T, K, L> callback;

	public ActionCapsule(Action<T, K, L> callback)
	{
		this.callback = callback;
		IsValid = callback != null;
	}

	[HideInCallstack]
	public void Trigger(T t, K k, L l) => callback(t, k, l);
	public bool IsValid { get; private set; }
	public void Void() => IsValid = false;

	public override bool Equals(object other)
	{
		if (other == null) return false;
		if (GetHashCode() != other.GetHashCode()) return false;

		if (other is ActionCapsule<T, K, L> cap)
			return callback?.Equals(cap.callback) ?? cap.callback == null;

		return base.Equals(other);
	}

	public bool Equals(ActionCapsule<T, K, L> other)
	{
		if (GetHashCode() != other.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode() => callback?.GetHashCode() ?? 0;
}