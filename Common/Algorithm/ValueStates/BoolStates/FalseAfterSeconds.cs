using BoolStateOperations;

public class FalseAfterSeconds : IBoolStateObserver
{
	private readonly IBoolStateObserver _isValid;

	public IEventRegister OnTrueState => _isValid.OnTrueState;
	public IEventRegister OnFalseState => _isValid.OnFalseState;
	public bool Value => _isValid.Value;
	public IEventRegister<bool> OnChange => _isValid.OnChange;
	public bool Get() { return _isValid.Get(); }

	public FalseAfterSeconds( float defaultBubbleTime )
	{
		_isValid = new Not( new TrueAfterSeconds( defaultBubbleTime ) );
	}
}