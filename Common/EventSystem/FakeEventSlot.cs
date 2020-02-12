using System;

public class FakeEvent : IEventRegister
{
	private static readonly FakeEvent _instance = new FakeEvent();
	public static IEventRegister Instance => _instance;

	public void Register( IEventTrigger listener ) { }
	public bool Unregister( IEventTrigger listener ) => true;

	private FakeEvent() { }
}


public class FakeEvent<T> : IEventRegister<T>
{
	private static readonly FakeEvent<T> _instance = new FakeEvent<T>();
	public static IEventRegister<T> Instance => _instance;

	public void Register( IEventTrigger<T> listener ) { }
	public void Register( IEventTrigger listener ) { }
	public bool Unregister( IEventTrigger<T> listener ) => true;
	public bool Unregister( IEventTrigger listener ) => true;

	private FakeEvent() { }
}


public class FakeEvent<T, K> : IEventRegister<T, K>
{
	private static readonly FakeEvent<T, K> _instance = new FakeEvent<T, K>();
	public static IEventRegister<T, K> Instance => _instance;

	public void Register( IEventTrigger<T, K> listener ) { }
	public void Register( IEventTrigger<T> listener ) { }
	public void Register( IEventTrigger listener ) { }
	public bool Unregister( IEventTrigger<T, K> listener ) => true;
	public bool Unregister( IEventTrigger<T> listener ) => true;
	public bool Unregister( IEventTrigger listener ) => true;

	private FakeEvent() { }
}


public class FakeEvent<T, K, J> : IEventRegister<T, K, J>
{
	private static readonly FakeEvent<T,K,J> _instance = new FakeEvent<T,K,J>();
	public static IEventRegister<T,K,J> Instance => _instance;

	public void Register( IEventTrigger<T, K, J> listener ) { }
	public void Register( IEventTrigger<T, K> listener ) { }
	public void Register( IEventTrigger<T> listener ) { }
	public void Register( IEventTrigger listener ) { }
	public bool Unregister( IEventTrigger<T, K, J> listener ) => true;
	public bool Unregister( IEventTrigger<T, K> listener ) => true;
	public bool Unregister( IEventTrigger<T> listener ) => true;
	public bool Unregister( IEventTrigger listener ) => true;

	private FakeEvent() { }
}
