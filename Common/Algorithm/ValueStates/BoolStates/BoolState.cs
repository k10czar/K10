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

[System.Serializable]
public class BoolState : IBoolState, ICustomDisposableKill
{
	public const string SET_METHOD_NAME = nameof( Setter );
	public const string ON_CHANGE_PROP_NAME = nameof( OnChange );
	bool _killed;
	[SerializeField] bool _value;

	[System.NonSerialized] private EventSlot<bool> _onChange;
	[System.NonSerialized] private EventSlot _onTrue;
	[System.NonSerialized] private EventSlot _onFalse;
	[System.NonSerialized] private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();
	public IBoolStateObserver Not => _not.Request( this );

	public static implicit operator bool( BoolState v ) => v._value;

	public bool Value { get => _value; set { Setter( value ); } }
	public bool Get() => _value;

	public void Setter( bool value )
	{
		if( _value == value ) return;
		_value = value;

		_onChange?.Trigger( value );
		if( value ) _onTrue?.Trigger();
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

	public IEventRegister<bool> OnChange => _killed ? _onChange : _onChange ??= new();// Lazy.Request( ref _onChange, _killed );
	public IEventRegister OnTrueState => _killed ? _onTrue : _onTrue ??= new();//Lazy.Request( ref _onTrue, _killed );
	public IEventRegister OnFalseState => _killed ? _onFalse : _onFalse ??= new();//Lazy.Request( ref _onFalse, _killed );

	public BoolState() : this( false ) { }
	public BoolState( bool initialValue ) { _value = initialValue; }

	public override string ToString() { return string.Format( "BS({0})", _value ); }
}

public static class BoolStateExtention
{
	public static void SetTrue( this IValueState<bool> boolState ) { boolState.Setter( true ); }
	public static void SetFalse( this IValueState<bool> boolState ) { boolState.Setter( false ); }
	public static void Flip( this IValueState<bool> boolState ) { boolState.Setter( !boolState.Value ); }

	public static void RegisterOnTrue( this IBoolStateObserver state, IEventTrigger evnt, bool triggerIfValid = true ) { state.OnTrueState.Register( evnt ); if( triggerIfValid && state.Value ) evnt.Trigger(); }
	public static void RegisterOnTrue( this IBoolStateObserver state, System.Action evnt, bool triggerIfValid = true ) { state.OnTrueState.Register( evnt ); if( triggerIfValid && state.Value ) evnt(); }

	public static void RegisterOnFalse( this IBoolStateObserver state, IEventTrigger evnt, bool triggerIfValid = true ) { state.OnFalseState.Register( evnt ); if( triggerIfValid && !state.Value ) evnt.Trigger(); }
	public static void RegisterOnFalse( this IBoolStateObserver state, System.Action evnt, bool triggerIfValid = true ) { state.OnFalseState.Register( evnt ); if( triggerIfValid && !state.Value ) evnt(); }

	public static void RegisterOn( this IBoolStateObserver state, bool value, IEventTrigger evnt, bool triggerIfValid = true ) { if( value ) state.RegisterOnTrue( evnt, triggerIfValid ); else state.RegisterOnFalse( evnt, triggerIfValid ); }
	public static void RegisterOn( this IBoolStateObserver state, bool value, System.Action evnt, bool triggerIfValid = true ) { if( value ) state.RegisterOnTrue( evnt, triggerIfValid ); else state.RegisterOnFalse( evnt, triggerIfValid ); }
}
