using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace K10.DebugSystem
{
    public interface ILoggable<T> where T : DebugCategory, new()
    {
        public static readonly Object[] nullOwners = { null };

        Object[] LogOwners => nullOwners;
        Object MainLogOwner => LogOwners[0];
    }

    public static class LoggableTargetExtensions
    {
        #region Logs

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void Log<T>(this ILoggable<T> obj, string message) where T : DebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Info, message, false, obj.MainLogOwner, obj.LogOwners);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void Log<T>(this ILoggable<T> obj, string message, Object consoleTarget) where T : DebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Info, message, false, consoleTarget, obj.LogOwners.Append(consoleTarget));
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogVerbose<T>(this ILoggable<T> obj, string message, bool isVerbose = true) where T : DebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Warning, message, isVerbose, obj.MainLogOwner, obj.LogOwners);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogVerbose<T>(this ILoggable<T> obj, string message, Object consoleTarget, bool isVerbose = true) where T : DebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Warning, message, isVerbose, consoleTarget, obj.LogOwners.Append(consoleTarget));
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogError<T>(this ILoggable<T> obj, string message) where T : DebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Error, message, false, obj.MainLogOwner, obj.LogOwners);
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogError<T>(this ILoggable<T> obj, string message, Object consoleTarget) where T : DebugCategory, new()
        {
            K10Log<T>.Log(LogSeverity.Error, message, false, consoleTarget, obj.LogOwners.Append(consoleTarget));
        }

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void LogException<T>(this ILoggable<T> obj, System.Exception exception) where T : DebugCategory, new()
        {
            Debug.LogException(exception, obj.MainLogOwner);
        }

        #endregion

        #region Can Debug

        public static bool CanLog<T>(this ILoggable<T> obj, bool verbose = false) where T : DebugCategory, new()
        {
            return K10DebugSystem.CanDebug<T>(verbose) && K10DebugSystem.CheckDebugOwners(obj.LogOwners);
        }

        public static bool CanDebugVisuals<T>(this ILoggable<T> obj) where T : DebugCategory, new()
        {
            return K10DebugSystem.ShowVisuals<T>() && K10DebugSystem.CheckDebugOwners(obj.LogOwners);
        }

        public static bool SkipVisuals<T>(this ILoggable<T> obj) where T : DebugCategory, new() => !CanDebugVisuals(obj);

        public static Color LogColor<T>(this ILoggable<T> obj) where T : DebugCategory, new() => K10Log<T>.Category.Color;

        [HideInCallstack, System.Diagnostics.Conditional(K10Log.ConditionalDirective)]
        public static void TryHide<T>(this ILoggable<T> loggable, GameObject obj) where T : DebugCategory, new()
        {
            obj.hideFlags = loggable.SkipVisuals() ? HideFlags.HideAndDontSave : HideFlags.None;
        }

        #endregion

        #region Hide

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void HideInEditor<T>(this ILoggable<T> obj, Component target) where T : DebugCategory, new()
            => HideInEditor(obj, target.gameObject);

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void HideInEditor<T>(this ILoggable<T> obj, GameObject target) where T : DebugCategory, new()
        {
            var shouldHide = K10DebugSystem.CanDebug<T>(EDebugType.Hide);
            var flags = Application.isPlaying
                ? shouldHide ? HideFlags.HideInHierarchy : HideFlags.None
                : shouldHide ? HideFlags.HideAndDontSave : HideFlags.DontSave;

            target.hideFlags = flags;

            var category = K10DebugSystem.GetCategory<T>();
            category.HiddenObjects ??= ListPool<GameObject>.Get();
            category.HiddenObjects.Add(target);
        }

        #endregion
    }
}