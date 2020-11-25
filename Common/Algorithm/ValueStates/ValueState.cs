using UnityEngine;

public interface INumericValueState<T> : IValueState<T>, INumericValueStateSetter<T> where T : struct { }

public interface INumericValueStateSetter<T> : IValueStateSetter<T> where T : struct
{
	void Increment( T t );
}

public interface IValueState<T> : IValueStateObserver<T>, IValueStateSetter<T> where T : struct { }

public interface IValueStateSetter<T> where T : struct
{
	void Setter( T t );
}

public interface IValueStateObserver<T> where T : struct
{
	T Value { get; }
	T Get();
	IEventRegister<T> OnChange { get; }
}

[System.Serializable]
public class ValueState<T> : IValueState<T>, ISerializationCallbackReceiver where T : struct
{
	[SerializeField] T _value;

	[System.NonSerialized] EventSlot<T> _onChange = new EventSlot<T>();

	public T Value { get { return _value; } set { Setter( value ); } }
	public T Get() { return _value; }

	public void Setter( T value )
	{
		if( _value.Equals( value ) ) return;
		_value = value;
		_onChange.Trigger( value );
	}

	public IEventRegister<T> OnChange { get { return _onChange; } }

	public ValueState( T initialValue = default( T ) ) { _value = initialValue; }

	public override string ToString() { return string.Format( $"VS<{typeof( T )}>({_value})" ); }
	void Init()
	{
		if( _onChange == null ) _onChange = new EventSlot<T>();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
	void ISerializationCallbackReceiver.OnAfterDeserialize() { Init(); }
}

public static class ValueStateExtention
{
	public static void Synchronize<T>( this IValueStateObserver<T> state, System.Action evnt ) where T : struct { Synchronize( state, new ActionEventCapsule( evnt ) ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, System.Action evnt, IEventValidator validation ) where T : struct { Synchronize( state, validation.Validated( evnt ) ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, System.Action evnt, System.Func<bool> validation ) where T : struct { Synchronize( state, new ConditionalEventListener( evnt, validation ) ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, System.Action evnt, params IEventValidator[] validators ) where T : struct { Synchronize( state, new ActionEventCapsule( evnt ), validators ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, IEventTrigger evnt ) where T : struct { state.OnChange.Register( evnt ); if( evnt.IsValid ) evnt.Trigger(); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, IEventTrigger evnt, IEventValidator validation ) where T : struct { Synchronize( state, validation.Validated( evnt ) ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, IEventTrigger evnt, System.Func<bool> validation ) where T : struct { Synchronize( state, new ConditionalEventListener( evnt, validation ) ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, IEventTrigger evnt, params IEventValidator[] validators ) where T : struct
	{
		var vals = validators.GetCurrentValidators();
		Synchronize( state, new ConditionalEventListener( evnt, () => vals.And() && evnt.IsValid ) );
	}

	public static void Synchronize<T>( this IValueStateObserver<T> state, System.Action<T> evnt ) where T : struct { Synchronize<T>( state, new ActionEventCapsule<T>( evnt ) ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, System.Action<T> evnt, IEventValidator validation ) where T : struct { Synchronize<T>( state, validation.Validated<T>( evnt ) ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, System.Action<T> evnt, System.Func<bool> validation ) where T : struct { Synchronize<T>( state, new ConditionalEventListener<T>( evnt, validation ) ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, System.Action<T> evnt, params IEventValidator[] validators ) where T : struct { Synchronize( state, new ActionEventCapsule<T>( evnt ), validators ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, IEventTrigger<T> evnt ) where T : struct { state.OnChange.Register( evnt ); if( evnt.IsValid ) evnt.Trigger( state.Value ); }
	public static void Synchronize<T>( this IValueStateObserver<T> source, IValueStateSetter<T> valueState ) where T : struct { Synchronize<T>( source, valueState.Setter ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, IEventTrigger<T> evnt, IEventValidator validation ) where T : struct { Synchronize<T>( state, validation.Validated<T>( evnt ) ); }
	public static void Synchronize<T>( this IValueStateObserver<T> source, IValueStateSetter<T> valueState, IEventValidator validation ) where T : struct { Synchronize<T>( source, valueState.Setter, validation ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, IEventTrigger<T> evnt, System.Func<bool> validation ) where T : struct { Synchronize<T>( state, new ConditionalEventListener<T>( evnt, () => validation() && evnt.IsValid ) ); }
	public static void Synchronize<T>( this IValueStateObserver<T> state, IEventTrigger<T> evnt, params IEventValidator[] validators ) where T : struct
	{
		var vals = validators.GetCurrentValidators();
		Synchronize( state, new ConditionalEventListener<T>( evnt, () => vals.And() && evnt.IsValid ) );
	}
}