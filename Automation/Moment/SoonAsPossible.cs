public class SoonAsPossible : IMoment
{
	public IEventRegister GetEvent() => InstantTriggerOnce.Instance;
}
