public interface IService
{
}

public interface IReadyService : IService
{
    InitializationEvent<IService> IsReady { get; }
}