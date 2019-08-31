

public interface IObjectLifeState
{
    IBoolStateObserver IsValid { get; }
}

public interface IVoidableObjectLifeState : IObjectLifeState
{
    void Expire();
}

public class VoidableObjectLifeState : IVoidableObjectLifeState
{
    private readonly BoolState _isValid = new BoolState( true );
    public IBoolStateObserver IsValid { get { return _isValid; } }
    public void Expire() { _isValid.SetFalse(); }
}