using System;
using K10.EventSystem;
using UnityEngine;

public class CallOnceCapsule : ActionCapsuleBase, IEventTrigger
{
	private readonly Action callback;

	public CallOnceCapsule(Action callback) : base(callback)
	{
		this.callback = callback;
		IsValid = callback != null;
	}

	[HideInCallstack]
	public void Trigger()
	{
		IsValid = false;
		callback();
	}
}

public class CallOnceCapsule<T> : ActionCapsuleBase, IEventTrigger<T>
{
	private readonly Action<T> callback;

	public CallOnceCapsule(Action<T> callback) : base(callback)
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
}

public class CallOnceCapsule<T, K> : ActionCapsuleBase, IEventTrigger<T, K>
{
	private readonly Action<T, K> callback;

	public CallOnceCapsule(Action<T, K> callback) : base(callback)
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
}

public class CallOnceCapsule<T, K, L> : ActionCapsuleBase, IEventTrigger<T, K, L>
{
	private readonly Action<T, K, L> callback;

	public CallOnceCapsule(Action<T, K, L> callback) : base(callback)
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
}