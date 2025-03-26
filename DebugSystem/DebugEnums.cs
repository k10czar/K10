namespace K10.DebugSystem
{
    public enum EDebugOwnerBehaviour
    {
        Ignore,
        AnyOwnerListed,
        AllOwnersListed,
        AnyListedAndSelected,
    }

    public enum ELogPrefix
    {
        None,
        Name,
        ToString,
    }

    public enum EDebugType
    {
        Default,
        Verbose,
        Visual,
    }

    public enum LogSeverity
    {
        Info,
        Warning,
        Error
    }
}