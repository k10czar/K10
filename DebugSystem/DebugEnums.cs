namespace K10.DebugSystem
{
    public enum EDebugOwnerBehaviour
    {
        Ignore,
        AnyOwnerListed,
        AllOwnersListed,
        AnyListedAndSelected,
    }

    public enum EDebugType
    {
        Default,
        Verbose,
        Visual,
        InGame,
    }

    public enum LogSeverity
    {
        Info,
        Warning,
        Error
    }
}