using System;

public interface IEvent : IEventRegister, IEventTrigger { }
public interface IEvent<T> : IEventRegister<T>, IEventTrigger<T> { }
public interface IEvent<T, K> : IEventRegister<T, K>, IEventTrigger<T, K> { }
public interface IEvent<T, K, L> : IEventRegister<T, K, L>, IEventTrigger<T, K, L> { }

public interface IEventRegister
{
	void Register( IEventTrigger listener );
	bool Unregister( IEventTrigger listener );
	void Register( Action listener );
	bool Unregister( Action listener );
}

public interface IEventRegister<T> : IEventRegister
{
	void Register( IEventTrigger<T> listener );
	bool Unregister( IEventTrigger<T> listener );
	void Register( Action<T> listener );
	bool Unregister( Action<T> listener );
}

public interface IEventRegister<T, K> : IEventRegister<T>
{
	void Register( IEventTrigger<T, K> listener );
	bool Unregister( IEventTrigger<T, K> listener );
	void Register( Action<T, K> listener );
	bool Unregister( Action<T, K> listener );

}
public interface IEventRegister<T, K, L> : IEventRegister<T, K>
{
	void Register( IEventTrigger<T, K, L> listener );
	bool Unregister( IEventTrigger<T, K, L> listener );
	void Register( Action<T, K, L> listener );
	bool Unregister( Action<T, K, L> listener );
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

public interface IEventTrigger<T, K, L> : IValidatedObject
{
	void Trigger( T t, K k, L l );
}
