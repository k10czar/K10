using UnityEngine;

namespace K10.DebugSystem
{
    public interface ILoggable<T> where T : IK10LogCategory, new()
    {
        Object LogTarget { get; }

        string AddPrefix(string message, Object target) => $"{GetPrefix(target)}{message}";

        string GetPrefix(Object target) => K10Log<T>.PrefixType switch
        {
            ELogPrefix.None => "",
            ELogPrefix.Name => $"<b>{target.name} |</b> ",
            ELogPrefix.ToString => $"<b>{target} |</b> ",
            _ => throw new System.ArgumentOutOfRangeException()
        };
    }

    public static class LoggableTargetExtensions
    {
        #region Logs

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void Log<T>(this ILoggable<T> obj, string message) where T : IK10LogCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Info, obj.AddPrefix(message, obj.LogTarget), obj.LogTarget);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogVerbose<T>(this ILoggable<T> obj, string message, bool isVerbose = true) where T : IK10LogCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Warning, obj.AddPrefix(message, obj.LogTarget), obj.LogTarget, obj.LogTarget, isVerbose);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void Log<T>(this ILoggable<T> obj, string message, Object customTarget) where T : IK10LogCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Info, obj.AddPrefix(message, obj.LogTarget), obj.LogTarget, customTarget);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogVerbose<T>(this ILoggable<T> obj, string message, Object customTarget, bool isVerbose = true) where T : IK10LogCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Warning, obj.AddPrefix(message, obj.LogTarget), obj.LogTarget, customTarget, isVerbose);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogError<T>(this ILoggable<T> obj, string message) where T : IK10LogCategory, new()
        {
            var ownerTarget = obj as Object;
            K10Log<T>.Log(LogSeverity.Error, obj.AddPrefix(message, ownerTarget), ownerTarget);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogError<T>(this ILoggable<T> obj, string message, Object customTarget) where T : IK10LogCategory, new()
        {
            var ownerTarget = obj as Object;
            K10Log<T>.Log(LogSeverity.Error, obj.AddPrefix(message, ownerTarget), ownerTarget, customTarget);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogException<T>(this ILoggable<T> obj, System.Exception exception) where T : IK10LogCategory, new()
        {
            K10Log<T>.LogException(exception, obj as Object);
        }

        #endregion

        #region Can Debug

        public static bool CanLog<T>(this ILoggable<T> obj, bool verbose = false) where T : IK10LogCategory, new()
        {
            return K10DebugSystem.CanDebug<T>(verbose) && K10DebugSystem.CheckDebugOwners(obj.LogTarget);
        }

        public static bool CanDebugVisuals<T>(this ILoggable<T> obj) where T : IK10LogCategory, new()
        {
            return K10DebugSystem.ShowVisuals<T>() && K10DebugSystem.CheckDebugOwners(obj.LogTarget);
        }

        public static bool SkipVisuals<T>(this ILoggable<T> obj) where T : IK10LogCategory, new() => !CanDebugVisuals(obj);

        public static Color LogColor<T>(this ILoggable<T> obj) where T : IK10LogCategory, new() => K10Log<T>.Category.Color;

        #endregion
    }
}