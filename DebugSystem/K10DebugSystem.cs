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

        public static EDebugTargets DebugTargetType() => config.targetType;
        public static void ToggleDebugTargetType() => config.ToggleDebugTargetType();

        public static List<string> DebugTargets() => config.targets;
        public static void ToggleDebugTarget(string target) => config.ToggleDebugTargets(target);
        public static void ToggleDebugTarget(Object target) => config.ToggleDebugTargets(getTargetKey(target));

        public static bool DebugErrors() => config.errors;
        public static void ToggleDebugErrors() => config.ToggleDebugErrors();

        public static Func<Object, string> getTargetKey;

        public static string DefaultGetDebugTargetKey(Object target) => target switch
        {
            null => null,
            Component component => component.gameObject.name,
            GameObject gameObject => gameObject.name,
            _ => throw new NotImplementedException()
        };

        public static bool CanDebugTarget(Object targetObject, LogSeverity severity = LogSeverity.Info)
        {
            if (DebugErrors() && severity is LogSeverity.Error) return true;

            var key = getTargetKey(targetObject);

            return DebugTargetType() switch
            {
                EDebugTargets.Disabled => false,
                EDebugTargets.All => true,
                EDebugTargets.OnlySelected => config.targets.Contains(key),
                EDebugTargets.NullAndSelected => key == null || config.targets.Contains(key),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion

        static K10DebugSystem()
        {
            config = K10DebugConfig.Load();
            getTargetKey = DefaultGetDebugTargetKey;
        }
    }
}