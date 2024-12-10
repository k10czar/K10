using K10.DebugSystem;
using UnityEngine;

public interface ILogglable<T> where T : IK10LogCategory, new()
{
    Color DebugColor => K10Log<T>.Color;
    bool prefixWithObjName => false;
    string AddPrefix(string message) => $"{GetPrefix()}{message}";

    string GetPrefix() => K10Log<T>.PrefixType switch
    {
        ELogPrefix.None => "",
        ELogPrefix.Name => $"<b>{((MonoBehaviour) this).name} |</b> ",
        ELogPrefix.ToString => $"<b>{ToString()} |</b> ",
        _ => throw new System.ArgumentOutOfRangeException()
    };
}

public interface ILogglableTarget<T> : ILogglable<T> where T : IK10LogCategory, new()
{
    MonoBehaviour LogTarget { get; }
}

public static class LogglableTargetExtentions
{
    [ConstLike] public static readonly Color HIERARCHY_COLOR = Colors.Console.Names;

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>( this ILogglable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Info ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, obj.AddPrefix(message), obj as MonoBehaviour );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogWithHierarchy<T>( this ILogglable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, HierarchyOnMessage( message, obj ), obj as Object );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogError<T>( this ILogglable<T> obj, string message ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( LogSeverity.Error, obj.AddPrefix(message), obj as MonoBehaviour );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogErrorWithHierarchy<T>( this ILogglable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( LogSeverity.Error, HierarchyOnMessage( message, obj ), obj as Object );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogException<T>( this ILogglable<T> obj, System.Exception exception ) where T : IK10LogCategory, new()
    {
        K10Log<T>.LogException( exception, obj as Object );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>( this ILogglable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, obj.AddPrefix(message), obj as MonoBehaviour, true );
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>(this ILogglable<T> obj, string message, Object customTarget, LogSeverity logSeverity = LogSeverity.Info) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(logSeverity, obj.AddPrefix(message), customTarget, false);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>(this ILogglable<T> obj, string message, Object customTarget, LogSeverity logSeverity = LogSeverity.Warning) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(logSeverity, obj.AddPrefix(message), customTarget, true);
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerboseWithHierarchy<T>( this ILogglable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, HierarchyOnMessage( message, obj ), obj as Object, true );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>( this ILogglableTarget<T> obj, string message, LogSeverity logSeverity = LogSeverity.Info ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, obj.AddPrefix(message), obj.LogTarget );
    }

    [HideInCallstack,System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>( this ILogglableTarget<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, obj.AddPrefix(message), obj.LogTarget, true );
    }

    public static bool CanLog<T>( this ILogglable<T> obj, bool verbose = false ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebug<T>( verbose );
    }

    public static bool CanDebugVisuals<T>( this ILogglable<T> obj ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebugVisuals<T>();
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

    private static string HierarchyOnMessage( string message, object obj )
    {
        if( obj is Component comp ) return $"{comp.HierarchyNameOrNullColored(HIERARCHY_COLOR)}.{message}";
        return $"{obj.TypeNameOrNullColored(HIERARCHY_COLOR)}.{message}";
    }
}