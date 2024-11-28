

public interface IActivatable
{
    IBoolStateObserver IsActive { get; }
}

public interface IActiveSetter : IActivatable
{
    void SetActive( bool active );
}
