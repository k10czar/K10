

public class FalseState : IBoolStateObserver
{
	private static readonly FalseState _instance = new FalseState();
	public static IBoolStateObserver Instance => _instance;

	public bool Value => false;
	public IEventRegister OnTrueState => FakeEvent.Instance;
	public IEventRegister OnFalseState => FakeEvent.Instance;
	public IEventRegister<bool> OnChange => FakeEvent<bool>.Instance;
	public IBoolStateObserver Not => TrueState.Instance;

	public bool Get() => false;

	private FalseState() { }
}
