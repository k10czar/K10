using System.Linq;
using UnityEngine;

namespace K10.DebugSystem
{
    public interface ILoggable<T> where T : IDebugCategory, new()
    {
        private static readonly Object[] nullOwners = { null };

        Object[] LogOwners => nullOwners;
        Object MainLogOwner => LogOwners[0];
    }

    public static class LoggableTargetExtensions
    {
        #region Logs

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void Log<T>(this ILoggable<T> obj, string message) where T : IDebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Info, message, false, obj.MainLogOwner, obj.LogOwners);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void Log<T>(this ILoggable<T> obj, string message, Object consoleTarget) where T : IDebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Info, message, false, consoleTarget, obj.LogOwners.Append(consoleTarget));
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogVerbose<T>(this ILoggable<T> obj, string message, bool isVerbose = true) where T : IDebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Warning, message, isVerbose, obj.MainLogOwner, obj.LogOwners);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogVerbose<T>(this ILoggable<T> obj, string message, Object consoleTarget, bool isVerbose = true) where T : IDebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Warning, message, isVerbose, consoleTarget, obj.LogOwners.Append(consoleTarget));
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogError<T>(this ILoggable<T> obj, string message) where T : IDebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Error, message, false, obj.MainLogOwner, obj.LogOwners);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogError<T>(this ILoggable<T> obj, string message, Object consoleTarget) where T : IDebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Error, message, false, consoleTarget, obj.LogOwners.Append(consoleTarget));
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogException<T>(this ILoggable<T> obj, System.Exception exception) where T : IDebugCategory, new()
        {
            Debug.LogException(exception, obj.MainLogOwner);
        }

        #endregion

        #region Can Debug

        public static bool CanLog<T>(this ILoggable<T> obj, bool verbose = false) where T : IDebugCategory, new()
        {
            return K10DebugSystem.CanDebug<T>(verbose) && K10DebugSystem.CheckDebugOwners(obj.LogOwners);
        }

        public static bool CanDebugVisuals<T>(this ILoggable<T> obj) where T : IDebugCategory, new()
        {
            return K10DebugSystem.ShowVisuals<T>() && K10DebugSystem.CheckDebugOwners(obj.LogOwners);
        }

        public static bool SkipVisuals<T>(this ILoggable<T> obj) where T : IDebugCategory, new() => !CanDebugVisuals(obj);

        public static Color LogColor<T>(this ILoggable<T> obj) where T : IDebugCategory, new() => K10Log<T>.Category.Color;

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void TryHide<T>(this ILoggable<T> loggable, GameObject obj) where T : IDebugCategory, new()
        {
            obj.hideFlags = loggable.SkipVisuals() ? HideFlags.HideAndDontSave : HideFlags.None;
        }

        #endregion

        #region Hide

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void HideInEditor<T>(this ILoggable<T> obj, Component target) where T : IDebugCategory, new()
            => HideInEditor(obj, target.gameObject);

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void HideInEditor<T>(this ILoggable<T> obj, GameObject target) where T : IDebugCategory, new()
        {
            var shouldHide = K10DebugSystem.CanDebug<T>(EDebugType.Hide);
            var flags = Application.isPlaying
                ? shouldHide ? HideFlags.HideInHierarchy : HideFlags.None
                : shouldHide ? HideFlags.HideAndDontSave : HideFlags.DontSave;

            target.hideFlags = flags;

            var category = K10DebugSystem.GetCategory<T>();
            category.HiddenObjects.Add(target);
        }

        #endregion
    }
}