

public interface IEventRequest : ICustomDisposableKill
{
    IEventRegister OnEvent { get; }
    void Request();
}