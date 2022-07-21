using UnityEngine;

public interface IBoolState : IValueState<bool>, IBoolStateObserver
{
}

public interface IBoolStateObserver : IValueStateObserver<bool>
{
	IEventRegister OnTrueState { get; }
	IEventRegister OnFalseState { get; }
}

[System.Serializable]
public class BoolState : IBoolState, ISerializationCallbackReceiver
{
	public const string SET_METHOD_NAME = nameof( Setter );
	public const string ON_CHANGE_PROP_NAME = nameof( OnChange );
	[SerializeField] bool _value;

	[System.NonSerialized] private EventSlot<bool> _onChange;
	[System.NonSerialized] private EventSlot _onTrue;
	[System.NonSerialized] private EventSlot _onFalse;

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

	public void Clear()
	{
		_onChange?.Clear();
		_onTrue?.Clear();
		_onFalse?.Clear();
		_onChange = null;
		_onTrue = null;
		_onFalse = null;
	}
	public IEventRegister<bool> OnChange => _onChange ?? ( _onChange = new EventSlot<bool>() );
	public IEventRegister OnTrueState => _onTrue ?? ( _onTrue = new EventSlot() );
	public IEventRegister OnFalseState => _onFalse ?? ( _onFalse = new EventSlot() );

	public BoolState( bool initialValue = false ) { _value = initialValue; Init(); }

	void Init()
	{
		if( _onChange == null ) _onChange = new EventSlot<bool>();
		if( _onTrue == null ) _onTrue = new EventSlot();
		if( _onFalse == null ) _onFalse = new EventSlot();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
	void ISerializationCallbackReceiver.OnAfterDeserialize(){ Init(); }

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
