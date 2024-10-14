public interface IService
{
}

public interface IReadyService : IService
{
    InitializationEvent IsReady { get; }
}