using UnityEngine;

public interface ILogglableObject<T> where T : IK10LogCategory, new()
{
    MonoBehaviour Target => null;
}

public interface ILogglableTarget<T> where T : IK10LogCategory, new()
{
    MonoBehaviour Target { get; }
}

public class LogglableBehaviour<T> : MonoBehaviour where T : IK10LogCategory, new()
{
    public MonoBehaviour Target => this;
}

public static class LogglableTargetExtentions
{
    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>( this ILogglableTarget<T> obj, string message, LogSeverity logSeverity = LogSeverity.Info ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, message, obj.Target );
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>( this ILogglableTarget<T> obj, string message, LogSeverity logSeverity = LogSeverity.Info ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, message, obj.Target, true );
    }

    public static bool CanLog<T>( this ILogglableTarget<T> obj, bool verbose = false ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebug<T>( verbose );
    }

    public static bool CanLogVisuals<T>( this ILogglableTarget<T> obj ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebugVisuals<T>();
    }
}
