namespace K10.DebugSystem
{
    public enum EDebugTargets
    {
        Disabled,
        All,
        ListedTarget,
        ListedAndSelected,
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