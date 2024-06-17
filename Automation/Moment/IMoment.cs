public interface IMoment
{
	IEventRegister GetEvent();
}

public static class MomentExtension
{
	public static IEventRegister GetEventOrInstant( this IMoment moment ) => moment.GetEvent() ?? InstantTriggerOnce.Instance;
}
