﻿using System;
using System.Collections.Generic;

public interface IEvent : IEventRegister, IEventTrigger { }
public interface IEvent<T> : IEventRegister<T>, IEventTrigger<T> { }
public interface IEvent<T, K> : IEventRegister<T, K>, IEventTrigger<T, K> { }
public interface IEvent<T, K, L> : IEventRegister<T, K, L>, IEventTrigger<T, K, L> { }

public interface IEventRegister
{
	void Register( IEventTrigger listener );
	bool Unregister( IEventTrigger listener );
}

public interface IEventRegister<T> : IEventRegister
{
	void Register( IEventTrigger<T> listener );
	bool Unregister( IEventTrigger<T> listener );
}

public interface IEventRegister<T, K> : IEventRegister<T>
{
	void Register( IEventTrigger<T, K> listener );
	bool Unregister( IEventTrigger<T, K> listener );

}
public interface IEventRegister<T, K, J> : IEventRegister<T, K>
{
	void Register( IEventTrigger<T, K, J> listener );
	bool Unregister( IEventTrigger<T, K, J> listener );
}

public static class EventExtensions
{
	public static void TriggerIfValid( this IEventTrigger trigger ) { if( trigger != null && trigger.IsValid ) trigger.Trigger(); }
	public static void TriggerIfValid<T>( this IEventTrigger<T> trigger, T t ) { if( trigger != null && trigger.IsValid ) trigger.Trigger( t ); }
	public static void TriggerIfValid<T,K>( this IEventTrigger<T,K> trigger, T t, K k ) { if( trigger != null && trigger.IsValid ) trigger.Trigger( t, k ); }
	public static void TriggerIfValid<T,K,J>( this IEventTrigger<T,K,J> trigger, T t, K k, J j ) { if( trigger != null && trigger.IsValid ) trigger.Trigger( t, k, j ); }
	public static void TriggerIfValid<T,K,J,L>( this IEventTrigger<T,K,J,L> trigger, T t, K k, J j, L l ) { if( trigger != null && trigger.IsValid ) trigger.Trigger( t, k, j, l ); }

	public static void Register( this IEventRegister register, Action act ) => register.Register( new ActionEventCapsule( act ) );
	public static void Unregister( this IEventRegister register, Action act ) => register.Unregister( new ActionEventCapsule( act ) );
	public static void Register<T>( this IEventRegister<T> register, Action<T> act ) => register.Register( new ActionEventCapsule<T>( act ) );
	public static void Unregister<T>( this IEventRegister<T> register, Action<T> act ) => register.Unregister( new ActionEventCapsule<T>( act ) );
	public static void Register<T,K>( this IEventRegister<T,K> register, Action<T,K> act ) => register.Register( new ActionEventCapsule<T,K>( act ) );
	public static void Unregister<T,K>( this IEventRegister<T,K> register, Action<T,K> act ) => register.Unregister( new ActionEventCapsule<T,K>( act ) );
	public static void Register<T,K,J>( this IEventRegister<T,K,J> register, Action<T,K,J> act ) => register.Register( new ActionEventCapsule<T,K,J>( act ) );
	public static void Unregister<T,K,J>( this IEventRegister<T,K,J> register, Action<T,K,J> act ) => register.Unregister( new ActionEventCapsule<T,K,J>( act ) );

	public static void RegisterValidated( this IEventRegister register, IEventValidator validator, Action act ) => register.Register( validator.Validated( act ) );
	public static void RegisterValidated<T>( this IEventRegister<T> register, IEventValidator validator, Action<T> act ) => register.Register( validator.Validated( act ) );
	public static void RegisterValidated<T, K>( this IEventRegister<T, K> register, IEventValidator validator, Action<T, K> act ) => register.Register( validator.Validated( act ) );
	public static void RegisterValidated<T, K, J>( this IEventRegister<T, K, J> register, IEventValidator validator, Action<T, K, J> act ) => register.Register( validator.Validated( act ) );

	public static void RegisterValidated( this IEventRegister register, IEventValidator validator, IEventTrigger act ) => register.Register( validator.Validated( act ) );
	public static void RegisterValidated<T>( this IEventRegister<T> register, IEventValidator validator, IEventTrigger<T> act ) => register.Register( validator.Validated( act ) );
	public static void RegisterValidated<T, K>( this IEventRegister<T, K> register, IEventValidator validator, IEventTrigger<T, K> act ) => register.Register( validator.Validated( act ) );
	public static void RegisterValidated<T, K, J>( this IEventRegister<T, K, J> register, IEventValidator validator, IEventTrigger<T, K, J> act ) => register.Register( validator.Validated( act ) );


	#region Enumerables
	public static void Register( this IEnumerable<IEventRegister> registers, Action act ) => registers.Register( new ActionEventCapsule( act ) );
	public static void Register( this IEnumerable<IEventRegister> registers, IEventTrigger listener )
	{
		foreach( var register in registers ) { register.Register( listener ); }
	}
	#endregion //Enumerables

	public static IEventTrigger<T> Filtered<T, K>( this IEventTrigger<K> register, Func<T, K> filter ) => new EventFilter<T,K>( register, filter );
}

public interface IValidatedObject
{
	bool IsValid { get; }
}

public static class ValidatedObjectExtensions
{
	public static bool IsValidAndNotNull( this IValidatedObject obj ) => obj != null && obj.IsValid;
}


public interface IEventTrigger : IValidatedObject
{
	void Trigger();
}

public interface IEventTrigger<T> : IValidatedObject
{
	void Trigger( T t );
}

public interface IEventTrigger<T, K> : IValidatedObject
{
	void Trigger( T t, K k );
}

public interface IEventTrigger<T, K, J> : IValidatedObject
{
	void Trigger( T t, K k, J j );
}

public interface IEventTrigger<T, K, J, L> : IValidatedObject
{
	void Trigger( T t, K k, J j, L l );
}
