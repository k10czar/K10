using System;
using UnityEngine;

public interface IBoolState : IValueState<bool>, IBoolStateObserver
{
}

public interface IBoolStateObserver : IValueStateObserver<bool>
{
	IEventRegister OnTrueState { get; }
	IEventRegister OnFalseState { get; }
	IBoolStateObserver Not { get; }
}

[Serializable]
public class BoolState : IBoolState, ICustomDisposableKill
{
	public const string SET_METHOD_NAME = nameof( Setter );
	public const string ON_CHANGE_PROP_NAME = nameof( OnChange );

	[NonSerialized] private bool _killed;
	[SerializeField] private bool _value;

	[NonSerialized] private EventSlot<bool> _onChange;
	[NonSerialized] private EventSlot _onTrue;
	[NonSerialized] private EventSlot _onFalse;
	[NonSerialized] private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();

	public IBoolStateObserver Not => _not.Request( this );

	public static implicit operator bool( BoolState v ) => v._value;

	public bool Value { get => _value; set => Setter(value); }
	public bool Get() => _value;

	public void Setter(bool value)
	{
		if (_value == value) return;
		_value = value;

		if (_killed) return;

		_onChange?.Trigger(value);

		if (value) _onTrue?.Trigger();
		else _onFalse?.Trigger();
	}

	public void Kill()
	{
		_killed = true;
		_onChange?.Kill();
		_onTrue?.Kill();
		_onFalse?.Kill();
		_not.Kill();
		_onChange = null;
		_onTrue = null;
		_onFalse = null;
	}

	public IEventRegister<bool> OnChange => _killed ? _onChange : _onChange ??= new();
	public IEventRegister OnTrueState => _killed ? _onTrue : _onTrue ??= new();
	public IEventRegister OnFalseState => _killed ? _onFalse : _onFalse ??= new();

	public BoolState() : this(false) {}
	public BoolState(bool initialValue) => _value = initialValue;

	public override string ToString() => $"BS({_value})";
}

public static class BoolStateExtensions
{
	public static void SetTrue(this IValueState<bool> boolState) => boolState.Setter(true);
	public static void SetFalse(this IValueState<bool> boolState) => boolState.Setter(false);
	public static void Flip(this IValueState<bool> boolState) => boolState.Setter(!boolState.Value);

	public static void RegisterOnTrue(this IBoolStateObserver state, IEventTrigger listener, bool triggerIfValid = true)
	{
		state.OnTrueState.Register(listener);
		if (triggerIfValid && state.Value) listener.Trigger();
	}

	public static ActionCapsule RegisterOnTrue(this IBoolStateObserver state, Action listener, bool triggerIfValid = true)
	{
		var capsule = state.OnTrueState.Register(listener);
		if (triggerIfValid && state.Value) listener();

		return capsule;
	}

	public static void RegisterOnFalse(this IBoolStateObserver state, IEventTrigger listener, bool triggerIfValid = true)
	{
		state.OnFalseState.Register(listener);
		if (triggerIfValid && !state.Value) listener.Trigger();
	}

	public static ActionCapsule RegisterOnFalse(this IBoolStateObserver state, Action listener, bool triggerIfValid = true)
	{
		var capsule = state.OnFalseState.Register(listener);
		if (triggerIfValid && !state.Value) listener();

		return capsule;
	}

	public static void RegisterOn(this IBoolStateObserver state, bool value, IEventTrigger listener, bool triggerIfValid = true)
	{
		if (value) state.RegisterOnTrue(listener, triggerIfValid);
		else state.RegisterOnFalse(listener, triggerIfValid);
	}

	public static ActionCapsule RegisterOn(this IBoolStateObserver state, bool value, Action listener, bool triggerIfValid = true)
	{
		return value ? state.RegisterOnTrue(listener, triggerIfValid) : state.RegisterOnFalse(listener, triggerIfValid);
	}
}