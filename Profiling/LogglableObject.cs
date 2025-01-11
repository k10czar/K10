using K10.DebugSystem;
using UnityEngine;

public interface ILoggable<T> where T : IK10LogCategory, new()
{
    Color DebugColor => K10Log<T>.Color;
    string AddPrefix(string message, Object target) => $"{GetPrefix(target)}{message}";

    virtual string GetPrefix(Object target) => K10Log<T>.PrefixType switch
    {
        ELogPrefix.None => "",
        ELogPrefix.Name => $"<b>{target.name} |</b> ",
        ELogPrefix.ToString => $"<b>{target} |</b> ",
        _ => throw new System.ArgumentOutOfRangeException()
    };
}

public interface ILoggableTarget<T> : ILoggable<T> where T : IK10LogCategory, new()
{
    MonoBehaviour LogTarget { get; }
}

public static class LoggableTargetExtensions
{
    #region ILoggable

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>(this ILoggable<T> obj, string message) where T : IK10LogCategory, new()
    {
        var ownerTarget = obj as Object;
        K10Log<T>.Log(LogSeverity.Info, obj.AddPrefix(message, ownerTarget), ownerTarget);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>(this ILoggable<T> obj, string message, bool isVerbose = true) where T : IK10LogCategory, new()
    {
        var ownerTarget = obj as Object;
        K10Log<T>.Log(LogSeverity.Warning, obj.AddPrefix(message, ownerTarget), ownerTarget, ownerTarget, isVerbose);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>(this ILoggable<T> obj, string message, Object customTarget) where T : IK10LogCategory, new()
    {
        var owner = obj as Object;
        K10Log<T>.Log(LogSeverity.Info, obj.AddPrefix(message, customTarget), owner, customTarget);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>(this ILoggable<T> obj, string message, Object customTarget, bool isVerbose = true) where T : IK10LogCategory, new()
    {
        var owner = obj as Object;
        K10Log<T>.Log(LogSeverity.Warning, obj.AddPrefix(message, customTarget), owner, customTarget, isVerbose);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogError<T>(this ILoggable<T> obj, string message) where T : IK10LogCategory, new()
    {
        var ownerTarget = obj as Object;
        K10Log<T>.Log(LogSeverity.Error, obj.AddPrefix(message, ownerTarget), ownerTarget);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogException<T>(this ILoggable<T> obj, System.Exception exception) where T : IK10LogCategory, new()
    {
        K10Log<T>.LogException(exception, obj as Object);
    }

    #endregion

    #region ILoggableTarget

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>(this ILoggableTarget<T> obj, string message) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(LogSeverity.Info, obj.AddPrefix(message, obj.LogTarget), obj.LogTarget);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>(this ILoggableTarget<T> obj, string message, bool isVerbose = true) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(LogSeverity.Warning, obj.AddPrefix(message, obj.LogTarget), obj.LogTarget, obj.LogTarget, isVerbose);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>(this ILoggableTarget<T> obj, string message, Object customTarget) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(LogSeverity.Info, obj.AddPrefix(message, obj.LogTarget), obj.LogTarget);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>(this ILoggableTarget<T> obj, string message, Object customTarget, bool isVerbose = true) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(LogSeverity.Warning, obj.AddPrefix(message, obj.LogTarget), obj.LogTarget, customTarget, isVerbose);
    }

    #endregion

    #region Can Log

    public static bool CanLog<T>(this ILoggable<T> obj, bool verbose = false) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebug<T>(verbose);
    }

    public static bool CanDebugVisuals<T>(this ILoggable<T> obj) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebugVisuals<T>() && K10DebugSystem.CanDebugTarget(obj as MonoBehaviour);
    }

    public static bool CanDebugVisuals<T>(this ILoggableTarget<T> obj) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebugVisuals<T>() && K10DebugSystem.CanDebugTarget(obj.LogTarget);
    }

    public static bool CanLogVisuals<T>(this ILoggable<T> obj) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebugVisuals<T>();
    }

    public static bool SkipVisuals<T>(this ILoggable<T> behaviour) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.SkipVisuals<T>();
    }

    public static Color LogColor<T>(this ILoggable<T> behaviour) where T : IK10LogCategory, new() => K10Log<T>.Category.Color;

    #endregion

    #region Log w/ Hierarchy

    [ConstLike] private static readonly Color HIERARCHY_COLOR = Colors.Console.Names;

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogErrorWithHierarchy<T>(this ILoggable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(LogSeverity.Error, HierarchyOnMessage(message, obj), obj as Object);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogWithHierarchy<T>(this ILoggable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(logSeverity, HierarchyOnMessage(message, obj), obj as Object);
    }

    [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerboseWithHierarchy<T>(this ILoggable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(logSeverity, HierarchyOnMessage(message, obj), obj as Object, obj as Object, true);
    }

    private static string HierarchyOnMessage(string message, object obj)
    {
        if (obj is Component comp) return $"{comp.HierarchyNameOrNullColored(HIERARCHY_COLOR)}.{message}";
        return $"{obj.TypeNameOrNullColored(HIERARCHY_COLOR)}.{message}";
    }

    #endregion
}