using System;
using K10.EventSystem;
using UnityEngine;

public class ActionCapsule : ActionCapsuleBase, IEventTrigger
{
	private readonly Action callback;
	private readonly IEventRegister observed;

	[HideInCallstack]
	public void Trigger() => callback();

	public override void Void()
	{
		if (IsValid) observed?.Unregister(this);
		IsValid = false;
	}

	#region Constructors

	[Obsolete("Use ActionCapsule(callback, observed) instead")]
	public ActionCapsule(Action callback) : base(callback)
	{
		this.callback = callback;
		this.observed = null;
	}

	public ActionCapsule(Action callback, IEventRegister observed) : base(callback)
	{
		this.callback = callback;
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	// Used only to unregister
	internal ActionCapsule(object callback, bool killOnly) : base(callback)
	{
		this.callback = null;
		this.observed = null;
		IsValid = false;
	}

	#endregion
}

public class ActionCapsule<T> : ActionCapsuleBase, IEventTrigger<T>
{
	private readonly Action<T> callback;
	private readonly IEventRegister<T> observed;

	[HideInCallstack]
	public void Trigger(T t) => callback(t);

	public override void Void()
	{
		if (IsValid) observed?.Unregister(this);
		IsValid = false;
	}

	#region Constructors

	[Obsolete("Use ActionCapsule<T>(callback, observed) instead")]
	public ActionCapsule(Action<T> callback) : base(callback)
	{
		this.callback = callback;
		this.observed = null;
	}

	public ActionCapsule(Action<T> callback, IEventRegister<T> observed) : base(callback)
	{
		this.callback = callback;
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action callback, IEventRegister<T> observed)
		: base(callback)
	{
		this.callback = callback.Wrap<T>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	// Used only to unregister
	internal ActionCapsule(object callback, bool killOnly) : base(callback)
	{
		this.callback = null;
		this.observed = null;
		IsValid = false;
	}

	#endregion
}

public class ActionCapsule<T,K> : ActionCapsuleBase, IEventTrigger<T,K>
{
	private readonly Action<T,K> callback;
	private readonly IEventRegister<T,K> observed;

	[HideInCallstack]
	public void Trigger(T t, K k) => callback(t, k);

	public override void Void()
	{
		if (IsValid) observed?.Unregister(this);
		IsValid = false;
	}

	#region Constructors

	[Obsolete("Use ActionCapsule<T,K>(callback, observed) instead")]
	public ActionCapsule(Action<T,K> callback) : base(callback)
	{
		this.callback = callback;
		this.observed = null;
	}

	public ActionCapsule(Action<T,K> callback, IEventRegister<T,K> observed) : base(callback)
	{
		this.callback = callback;
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action<T> callback, IEventRegister<T,K> observed) : base(callback)
	{
		this.callback = callback.Wrap<T,K>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action<K> callback, IEventRegister<T,K> observed) : base(callback)
	{
		this.callback = callback.Wrap<T,K>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action callback, IEventRegister<T,K> observed) : base(callback)
	{
		this.callback = callback.Wrap<T,K>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	// Used only to unregister
	internal ActionCapsule(object callback, bool killOnly) : base(callback)
	{
		this.callback = null;
		this.observed = null;
		IsValid = false;
	}

	#endregion
}

public class ActionCapsule<T,K,L> : ActionCapsuleBase, IEventTrigger<T,K,L>
{
	private readonly Action<T,K,L> callback;
	private readonly IEventRegister<T,K,L> observed;

	[HideInCallstack]
	public void Trigger(T t, K k, L l) => callback(t, k, l);

	public override void Void()
	{
		if (IsValid) observed?.Unregister(this);
		IsValid = false;
	}

	#region Constructors

	[Obsolete("Use ActionCapsule<T,K>(callback, observed) instead")]
	public ActionCapsule(Action<T,K,L> callback) : base(callback)
	{
		this.callback = callback;
		this.observed = null;
	}

	public ActionCapsule(Action<T,K,L> callback, IEventRegister<T,K,L> observed) : base(callback)
	{
		this.callback = callback;
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action<T> callback, IEventRegister<T,K,L> observed) : base(callback)
	{
		this.callback = callback.Wrap<T,K,L>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action<K> callback, IEventRegister<T,K,L> observed) : base(callback)
	{
		this.callback = callback.Wrap<T,K,L>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action<L> callback, IEventRegister<T,K,L> observed) : base(callback)
	{
		this.callback = callback.Wrap<T,K,L>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action<T,K> callback, IEventRegister<T,K,L> observed) : base(callback)
	{
		this.callback = callback.Wrap<T,K,L>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action<T,L> callback, IEventRegister<T,K,L> observed) : base(callback)
	{
		this.callback = callback.Wrap<T,K,L>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action<K,L> callback, IEventRegister<T,K,L> observed) : base(callback)
	{
		this.callback = callback.Wrap<T,K,L>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	public ActionCapsule(Action callback, IEventRegister<T,K,L> observed) : base(callback)
	{
		this.callback = callback.Wrap<T,K,L>();
		this.observed = observed;

		if (IsValid) observed.Register(this);
	}

	// Used only to unregister
	internal ActionCapsule(object callback, bool killOnly) : base(callback)
	{
		this.callback = null;
		this.observed = null;
		IsValid = false;
	}

	#endregion
}