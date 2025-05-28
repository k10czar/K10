public interface IReadyService : IService
{
    InitializationEvent<IService> IsReady { get; }
}