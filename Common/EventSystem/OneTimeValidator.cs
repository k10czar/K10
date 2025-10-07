
public class OneTimeValidator : IVoidableEventValidator, ICustomDisposableKill
{
	bool _killed = false;
	System.Func<bool> _cachedValidation;

	public System.Func<bool> CurrentValidationCheck => _cachedValidation ??= CheckIsValid;

	EventSlot _onVoid;
	public IEventRegister OnVoid => _onVoid ??= new();

	public void Kill()
	{
		_killed = true;
		_onVoid?.Trigger();
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
