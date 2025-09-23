using System;
using UnityEngine;

public struct CallOnceCapsule : IEventTrigger, IEquatable<CallOnceCapsule>, IVoidable
{
	private readonly Action callback;

	public CallOnceCapsule(Action callback)
	{
		this.callback = callback;
		IsValid = callback != null;
	}

	[HideInCallstack]
	public void Trigger()
	{
		if (!IsValid) return;

		IsValid = false;
		callback();
	}

	public bool IsValid { get; private set; }
	public void Void() => IsValid = false;

	public override bool Equals(object other)
	{
		if (other == null) return false;
		if (GetHashCode() != other.GetHashCode()) return false;

		if (other is CallOnceCapsule cap)
			return callback?.Equals(cap.callback) ?? cap.callback == null;

		return base.Equals(other);
	}

	public bool Equals(CallOnceCapsule other)
	{
		if (GetHashCode() != other.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode() => callback?.GetHashCode() ?? 0;
}

public struct CallOnceCapsule<T> : IEventTrigger<T>, IEquatable<CallOnceCapsule<T>>, IVoidable
{
	private readonly Action<T> callback;

	public CallOnceCapsule(Action<T> callback)
	{
		this.callback = callback;
		IsValid = callback != null;
	}

	[HideInCallstack]
	public void Trigger(T t)
	{
		IsValid = false;
		callback(t);
	}

	public bool IsValid { get; private set; }
	public void Void() => IsValid = false;

	public override bool Equals(object other)
	{
		if (other == null) return false;
		if (GetHashCode() != other.GetHashCode()) return false;

		if (other is CallOnceCapsule<T> cap)
			return callback?.Equals(cap.callback) ?? cap.callback == null;

		return base.Equals(other);
	}

	public bool Equals(CallOnceCapsule<T> other)
	{
		if (GetHashCode() != other.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode() => callback?.GetHashCode() ?? 0;
}

public struct CallOnceCapsule<T, K> : IEventTrigger<T, K>, IEquatable<CallOnceCapsule<T, K>>, IVoidable
{
	private readonly Action<T, K> callback;

	public CallOnceCapsule(Action<T, K> callback)
	{
		this.callback = callback;
		IsValid = callback != null;
	}

	[HideInCallstack]
	public void Trigger(T t, K k)
	{
		IsValid = false;
		callback(t, k);
	}

	public bool IsValid { get; private set; }
	public void Void() => IsValid = false;

	public override bool Equals(object obj)
	{
		if (obj == null) return false;
		if (GetHashCode() != obj.GetHashCode()) return false;
		if (obj is CallOnceCapsule<T, K> cap)
		{
			if (callback != null) return callback.Equals(cap.callback);
			return cap.callback.Equals(null);
		}

		return base.Equals(obj);
	}

	public bool Equals(CallOnceCapsule<T, K> other)
	{
		if (GetHashCode() != other.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode()
	{
		return callback?.GetHashCode() ?? 0;
	}
}

public struct CallOnceCapsule<T, K, L> : IEventTrigger<T, K, L>, IEquatable<CallOnceCapsule<T, K, L>>, IVoidable
{
	private readonly Action<T, K, L> callback;

	public CallOnceCapsule(Action<T, K, L> callback)
	{
		this.callback = callback;
		IsValid = callback != null;
	}

	[HideInCallstack]
	public void Trigger(T t, K k, L l)
	{
		IsValid = false;
		callback(t, k, l);
	}

	public bool IsValid { get; private set; }
	public void Void() => IsValid = false;

	public override bool Equals(object other)
	{
		if (other == null) return false;
		if (GetHashCode() != other.GetHashCode()) return false;

		if (other is CallOnceCapsule<T, K, L> cap)
			return callback?.Equals(cap.callback) ?? cap.callback == null;

		return base.Equals(other);
	}

	public bool Equals(CallOnceCapsule<T, K, L> other)
	{
		if (GetHashCode() != other.GetHashCode()) return false;
		return callback?.Equals(other.callback) ?? other.callback == null;
	}

	public override int GetHashCode() => callback?.GetHashCode() ?? 0;
}