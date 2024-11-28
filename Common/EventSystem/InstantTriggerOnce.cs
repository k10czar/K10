public class InstantTriggerOnce : IEventRegister
{
	private static InstantTriggerOnce _instance = null;
    public static IEventRegister Instance => _instance ??= new InstantTriggerOnce();

	private InstantTriggerOnce() { }

    public void Register( IEventTrigger listener )
    {
		listener.Trigger();
    }

    public bool Unregister( IEventTrigger listener ) { return true; }
}