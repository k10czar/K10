
public class OneTimeValidator : IVoidableEventValidator, ICustomDisposableKill
{
	bool _killed = false;

	public System.Func<bool> CurrentValidationCheck => CheckIsValid;

	EventSlot _onVoid;

	public IEventRegister OnVoid => Lazy.Request( ref _onVoid );

	public void Kill()
	{
		_killed = true;
		Clear.AfterKill( ref _onVoid );
	}

	public void Void()
	{
		_killed = true;
		_onVoid?.Trigger();
		Kill();
	}

	bool CheckIsValid() => !_killed;
}
