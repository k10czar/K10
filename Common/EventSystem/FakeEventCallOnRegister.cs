
public class FakeEventCallOnRegister : IEventRegister
{
	private static readonly FakeEventCallOnRegister _instance = new FakeEventCallOnRegister();
	public static IEventRegister Instance => _instance;

	public void Register( IEventTrigger listener ) { listener.TriggerIfValid(); }
	public bool Unregister( IEventTrigger listener ) => true;

	private FakeEventCallOnRegister() { }
}