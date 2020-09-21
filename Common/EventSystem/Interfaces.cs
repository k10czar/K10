using System;
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
	public static void Register( this IEventRegister register, Action act ) => register.Register( new ActionEventCapsule( act ) );
	public static void Unregister( this IEventRegister register, Action act ) => register.Unregister( new ActionEventCapsule( act ) );
	public static void Register<T>( this IEventRegister<T> register, Action<T> act ) => register.Register( new ActionEventCapsule<T>( act ) );
	public static void Unregister<T>( this IEventRegister<T> register, Action<T> act ) => register.Unregister( new ActionEventCapsule<T>( act ) );
	public static void Register<T,K>( this IEventRegister<T,K> register, Action<T,K> act ) => register.Register( new ActionEventCapsule<T,K>( act ) );
	public static void Unregister<T,K>( this IEventRegister<T,K> register, Action<T,K> act ) => register.Unregister( new ActionEventCapsule<T,K>( act ) );
	public static void Register<T,K,J>( this IEventRegister<T,K,J> register, Action<T,K,J> act ) => register.Register( new ActionEventCapsule<T,K,J>( act ) );
	public static void Unregister<T,K,J>( this IEventRegister<T,K,J> register, Action<T,K,J> act ) => register.Unregister( new ActionEventCapsule<T,K,J>( act ) );

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
