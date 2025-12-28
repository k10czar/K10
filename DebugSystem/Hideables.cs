using UnityEngine;
using UnityEngine.Pool;

namespace K10.DebugSystem
{
    public static class Hideables
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void HideInEditor<T>(this ILoggable<T> obj, Component target) where T : DebugCategory, new()
            => HideInEditor<T>(target.gameObject);

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void HideInEditor<T>(this ILoggable<T> obj, GameObject target) where T : DebugCategory, new()
            => HideInEditor<T>(target);

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void HideInEditor<T>(GameObject target) where T : DebugCategory, new()
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

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ReleaseHideInEditor<T>(this ILoggable<T> obj, Component target) where T : DebugCategory, new()
            => ReleaseHideInEditor<T>(target.gameObject);

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ReleaseHideInEditor<T>(this ILoggable<T> obj, GameObject target) where T : DebugCategory, new()
            => ReleaseHideInEditor<T>(target);

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ReleaseHideInEditor<T>(GameObject target) where T : DebugCategory, new()
        {
            target.hideFlags = HideFlags.None;

            var category = K10DebugSystem.GetCategory<T>();
            category.HiddenObjects.Remove(target);
        }
    }
}