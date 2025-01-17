using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace K10.DebugSystem
{
    public static class K10DebugSystem
    {
        private static readonly Type tempCategory = typeof(TempLogCategory);
        private static readonly K10DebugConfig config;

        public static bool CanDebug<T>(EDebugType debugType = EDebugType.Default) where T : IK10LogCategory, new()
            => CanDebug(typeof(T), debugType);

        public static bool CanDebug<T>(bool verbose) where T : IK10LogCategory, new() => CanDebug(typeof(T), verbose ? EDebugType.Verbose : EDebugType.Default);
        public static bool SkipDebug<T>(EDebugType debugType = EDebugType.Default) where T : IK10LogCategory, new() => !CanDebug<T>(debugType);

        public static bool ShowVisuals<T>() where T : IK10LogCategory, new() => CanDebug<T>(EDebugType.Visual);
        public static bool SkipVisuals<T>() where T : IK10LogCategory, new() => !ShowVisuals<T>();

        public static bool CanDebug(Type categoryType, bool verbose) => CanDebug(categoryType, verbose ? EDebugType.Verbose : EDebugType.Default);

        public static bool CanDebug(Type categoryType, EDebugType debugType = EDebugType.Default)
        {
            return categoryType == tempCategory || config.CanDebug(categoryType, debugType);
        }

        public static void ToggleCategory(Type categoryType, EDebugType debugType) => config.ToggleDebug(categoryType, debugType);
        public static void SetCategory(Type categoryType, EDebugType debugType, bool value) => config.SetDebug(categoryType, debugType, value);

        #region Debug Targets

        public static EDebugTargets DebugTargets() => config.targets;
        public static void ToggleDebugTargets() => config.ToggleDebugTargets();

        public static bool DebugErrors() => config.errors;
        public static void ToggleDebugErrors() => config.ToggleDebugErrors();

        public static bool CanDebugTarget(Object targetObject, LogSeverity severity = LogSeverity.Info)
        {
            if (DebugErrors() && severity is LogSeverity.Error) return true;

            GameObject target = targetObject switch
            {
                null => null,
                Component component => component.gameObject,
                GameObject gameObject => gameObject,
                _ => throw new NotImplementedException()
            };

            return DebugTargets() switch
            {
                EDebugTargets.Disabled => false,
                EDebugTargets.All => true,
                EDebugTargets.OnlySelected => selectedTargets.Contains(target),
                EDebugTargets.NullAndSelected => target == null || selectedTargets.Contains(target),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static readonly List<GameObject> selectedTargets = new();

        public static void AddTarget(GameObject go) => selectedTargets.Add(go);
        public static void RemoveTarget(GameObject go) => selectedTargets.Remove(go);

#if UNITY_EDITOR
        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange playModeStateChange) => selectedTargets.Clear();
#endif

        #endregion

        static K10DebugSystem()
        {
            config = K10DebugConfig.Load();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }
    }
}