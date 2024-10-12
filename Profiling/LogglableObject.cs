using System;
using K10.DebugSystem;
using UnityEngine;

public interface ILoggable<T> where T : IK10LogCategory, new()
{
    Color DebugColor => K10Log<T>.Color;
    string AddPrefix(string message) => $"{GetPrefix()}{message}";

    string GetPrefix() => K10Log<T>.PrefixType switch
    {
        ELogPrefix.None => "",
        ELogPrefix.Name => $"<b>{((MonoBehaviour) this).name} |</b> ",
        ELogPrefix.ToString => $"<b>{ToString()} |</b> ",
        _ => throw new ArgumentOutOfRangeException()
    };
}

public interface ILoggableTarget<T> : ILoggable<T> where T : IK10LogCategory, new()
{
    MonoBehaviour LogTarget { get; }
}

public static class LoggableTargetExtensions
{
    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>(this ILoggable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Info) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(logSeverity, obj.AddPrefix(message), obj as MonoBehaviour);
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>(this ILoggable<T> obj, string message, Component target, LogSeverity logSeverity = LogSeverity.Info) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(logSeverity, obj.AddPrefix(message), target);
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogError<T>(this ILoggable<T> obj, string message) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(LogSeverity.Error, obj.AddPrefix(message), obj as MonoBehaviour);
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogError<T>(this ILoggable<T> obj, string message, Component target) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(LogSeverity.Error, obj.AddPrefix(message), target);
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>(this ILoggable<T> obj, string message, Component target, LogSeverity logSeverity = LogSeverity.Warning) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(logSeverity, obj.AddPrefix(message), target, true);
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>(this ILoggable<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(logSeverity, obj.AddPrefix(message), obj as MonoBehaviour, true);
    }

    public static bool CanLog<T>(this ILoggable<T> obj, bool verbose = false) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebug<T>(verbose);
    }

    public static bool CanDebugVisuals<T>(this ILoggable<T> obj) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebugVisuals<T>() && K10DebugSystem.CanDebugTarget(obj as MonoBehaviour);
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void Log<T>(this ILoggableTarget<T> obj, string message, LogSeverity logSeverity = LogSeverity.Info) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(logSeverity, obj.AddPrefix(message), obj.LogTarget);
    }

    [System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
    public static void LogVerbose<T>(this ILoggableTarget<T> obj, string message, LogSeverity logSeverity = LogSeverity.Warning) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log(logSeverity, obj.AddPrefix(message), obj.LogTarget, true);
    }

    public static bool CanDebugVisuals<T>(this ILoggableTarget<T> obj) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebugVisuals<T>() && K10DebugSystem.CanDebugTarget(obj.LogTarget);
    }
}