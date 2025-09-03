using System;
using UnityEngine;

public class EventTriggerCapsule : IEventTrigger, IVoidable
{
	private IEventTrigger trigger;

	public EventTriggerCapsule(Action act) : this(new ActionCapsule(act)) {}
	public EventTriggerCapsule(IEventTrigger trigger) => this.trigger = trigger;

	[HideInCallstack]
	public void Trigger() { if (IsValid) trigger.Trigger(); }
	public bool IsValid => trigger?.IsValid ?? false;
	public void Void() => trigger = null;
}

public class EventTriggerCapsule<T> : IEventTrigger<T>, IVoidable
{
	private IEventTrigger<T> trigger;

	public EventTriggerCapsule(Action<T> act) : this(new ActionCapsule<T>(act)) {}
	public EventTriggerCapsule(IEventTrigger<T> trigger) => this.trigger = trigger;

	[HideInCallstack]
	public void Trigger(T t) { if (IsValid) trigger.Trigger(t); }
	public bool IsValid => trigger?.IsValid ?? false;
	public void Void() => trigger = null;
}

public class EventTriggerCapsule<T, K> : IEventTrigger<T, K>, IVoidable
{
	private IEventTrigger<T, K> trigger;

	public EventTriggerCapsule(Action<T, K> act) : this(new ActionCapsule<T, K>(act)) {}
	public EventTriggerCapsule(IEventTrigger<T, K> trigger) => this.trigger = trigger;

	[HideInCallstack]
	public void Trigger(T t, K k) { if (IsValid) trigger.Trigger(t, k); }
	public bool IsValid => trigger?.IsValid ?? false;
	public void Void() => trigger = null;
}

public class EventTriggerCapsule<T, K, J> : IEventTrigger<T, K, J>, IVoidable
{
	private IEventTrigger<T, K, J> trigger;

	public EventTriggerCapsule(Action<T, K, J> act) : this(new ActionCapsule<T, K, J>(act)) {}
	public EventTriggerCapsule(IEventTrigger<T, K, J> trigger) => this.trigger = trigger;

	[HideInCallstack]
	public void Trigger(T t, K k, J j) { if (IsValid) trigger.Trigger(t, k, j); }
	public bool IsValid => trigger?.IsValid ?? false;
	public void Void() => trigger = null;
}