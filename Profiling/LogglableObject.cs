using UnityEngine;

public interface ILogglable<T> where T : IK10LogCategory, new()
{
}

public interface ILogglableTarget<T> : ILogglable<T> where T : IK10LogCategory, new()
{
    MonoBehaviour LogTarget { get; }
}

public static class LogglableTargetExtentions
{
    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>( this ILogglable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Info ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, message, obj as MonoBehaviour );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogError<T>( this ILogglable<T> obj, string message ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( LogSeverity.Error, message, obj as MonoBehaviour );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogException<T>( this ILogglable<T> obj, System.Exception exception ) where T : IK10LogCategory, new()
    {
        K10Log<T>.LogException( exception, obj as MonoBehaviour );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>( this ILogglable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, message, obj as MonoBehaviour, true );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>( this ILogglableTarget<T> obj, string message, LogSeverity logSeverity = LogSeverity.Info ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, message, obj.LogTarget );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>( this ILogglableTarget<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, message, obj.LogTarget, true );
    }

    public static bool CanLog<T>( this ILogglable<T> obj, bool verbose = false ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebug<T>( verbose );
    }

    public static bool CanLogVisuals<T>( this ILogglable<T> obj ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebugVisuals<T>();
    }
    
    public static bool SkipVisuals<T>( this ILogglable<T> behaviour ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.SkipVisuals<T>();
    }
    
    public static Color LogColor<T>( this ILogglable<T> behaviour ) where T : IK10LogCategory, new()
    {
        return K10Log<T>.Category.Color;
    }
}
