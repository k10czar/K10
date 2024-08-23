using UnityEngine;

public interface ILogglable<T> where T : IK10LogCategory, new()
{
    bool prefixWithObjName => false;
    string TryAddPrefix(string message) => prefixWithObjName ? $"<b>{ToString()} |</b> {message}" : message;
    Color DebugColor => K10Log<T>.Color;
}

public interface ILogglableTarget<T> : ILogglable<T> where T : IK10LogCategory, new()
{
    MonoBehaviour LogTarget { get; }
}

public static class LogglableTargetExtentions
{
    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>( this ILogglable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Info ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, obj.TryAddPrefix(message), obj as MonoBehaviour );
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogError<T>( this ILogglable<T> obj, string message ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( LogSeverity.Error, obj.TryAddPrefix(message), obj as MonoBehaviour );
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>( this ILogglable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, obj.TryAddPrefix(message), obj as MonoBehaviour, true );
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>( this ILogglableTarget<T> obj, string message, LogSeverity logSeverity = LogSeverity.Info ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, obj.TryAddPrefix(message), obj.LogTarget );
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>( this ILogglableTarget<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, obj.TryAddPrefix(message), obj.LogTarget, true );
    }

    public static bool CanLog<T>( this ILogglable<T> obj, bool verbose = false ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebug<T>( verbose );
    }

    public static bool CanDebugVisuals<T>( this ILogglable<T> obj ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebugVisuals<T>();
    }
}