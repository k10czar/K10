using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Debug Owners

        public static Func<Object, string> getOwnerKey;

        public static EDebugOwnerBehaviour DebugOwnerBehaviour => config.ownerBehaviour;
        public static void ToggleOwnerBehaviour() => config.ToggleOwnerBehaviour();

        public static List<string> ValidOwners => config.validOwners;
        public static void ToggleValidOwner(string target) => config.ToggleValidOwner(target);
        public static void ToggleValidOwner(Object target) => config.ToggleValidOwner(getOwnerKey(target));

        public static string DefaultGetOwnerKey(Object target) => target switch
        {
            null => null,
            Component component => component.gameObject.name,
            GameObject gameObject => gameObject.name,
            ScriptableObject scriptableObject => scriptableObject.name,
            _ => throw new NotImplementedException()
        };

        private static GameObject GetGameObject(Object target) => target switch
        {
            null => null,
            Component component => component.gameObject,
            GameObject gameObject => gameObject,
            _ => throw new NotImplementedException()
        };

        #if UNITY_EDITOR
        private static bool IsSelection(Object candidate) => UnityEditor.Selection.activeGameObject == GetGameObject(candidate);
        #endif

        public static bool CheckDebugOwners(params Object[] requesters)
        {
            if (DebugOwnerBehaviour is EDebugOwnerBehaviour.Ignore) return true;

            var count = config.validOwners.Count;
            var keys = requesters.Select(getOwnerKey);
            var intersectCount = config.validOwners.Intersect(keys).Count();

            return DebugOwnerBehaviour switch
            {
                EDebugOwnerBehaviour.AnyOwnerListed => intersectCount > 0,
                EDebugOwnerBehaviour.AllOwnersListed => intersectCount == count,

                #if UNITY_EDITOR
                EDebugOwnerBehaviour.AnyListedAndSelected => intersectCount > 0 && requesters.Any(IsSelection),
                #endif

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion

        static K10DebugSystem()
        {
            config = K10DebugConfig.Load();
            getOwnerKey = DefaultGetOwnerKey;
        }
    }
}