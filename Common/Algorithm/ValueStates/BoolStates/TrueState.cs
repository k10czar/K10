

public class TrueState : IBoolStateObserver
{
	private static readonly TrueState _instance = new TrueState();
	public static IBoolStateObserver Instance => _instance;

	public bool Value => true;
	public IEventRegister OnTrueState => FakeEvent.Instance;
	public IEventRegister OnFalseState => FakeEvent.Instance;
	public IEventRegister<bool> OnChange => FakeEvent<bool>.Instance;

	public bool Get() => true;

	private TrueState() { }
}