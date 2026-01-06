using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace K10.DebugSystem
{
    public static class K10DebugSystem
    {
        private static readonly K10DebugConfig config;

        #region Categories

        private static readonly Type tempCategory = typeof(TempDebug);

        private static List<DebugCategory> categories;
        public static IEnumerable<DebugCategory> Categories
        {
            get
            {
                if (categories != null) return categories;

                categories = new List<DebugCategory>();

                foreach (var catType in TypeListDataCache.GetFrom(typeof(DebugCategory)).GetTypes())
                {
                    try
                    {
                        var newCategory = (DebugCategory)catType.CreateInstance();
                        newCategory.Setup();

                        categories.Add(newCategory);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"{catType.ToStringOrNullColored(Colors.Console.TypeName)}: {ex.Message}");
                    }
                }

                categories.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

                return categories;
            }
        }

        public static T GetCategory<T>() where T : DebugCategory
        {
            foreach (var candidate in Categories)
            {
                if (candidate is T t) return t;
            }

            throw new Exception($"Category {typeof(T).Name} not found!");
        }

        #endregion

        #region Debug Type

        public static bool CanDebug<T>(EDebugType debugType = EDebugType.Default) where T : DebugCategory, new()
            => CanDebug(typeof(T), debugType);

        public static bool CanDebug<T>(bool verbose) where T : DebugCategory, new() => CanDebug(typeof(T), verbose ? EDebugType.Verbose : EDebugType.Default);
        public static bool SkipDebug<T>(EDebugType debugType = EDebugType.Default) where T : DebugCategory, new() => !CanDebug<T>(debugType);

        public static bool ShowVisuals<T>() where T : DebugCategory, new() => CanDebug<T>(EDebugType.Visual);
        public static bool SkipVisuals<T>() where T : DebugCategory, new() => !ShowVisuals<T>();

        public static bool CanDebug(Type categoryType, bool verbose) => CanDebug(categoryType, verbose ? EDebugType.Verbose : EDebugType.Default);

        public static bool CanDebug(Type categoryType, EDebugType debugType = EDebugType.Default)
        {
            return categoryType == tempCategory || config.CanDebug(categoryType, debugType);
        }

        public static void ToggleCategory(Type categoryType, EDebugType debugType) => config.ToggleDebug(categoryType, debugType);
        public static void SetCategory(Type categoryType, EDebugType debugType, bool value, bool save = true) => config.SetDebug(categoryType, debugType, value, save);

        #endregion

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

        public static bool CheckDebugOwners(IEnumerable<Object> requesters)
        {
            if (DebugOwnerBehaviour is EDebugOwnerBehaviour.Ignore) return true;

            var count = requesters.Count();
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

        #region Custom Debug Flags

        public static bool CanDebugFlag(string flag) => config.CanDebugFlag(flag);
        public static void ToggleFlag(string flag) => config.ToggleCustomFlag(flag);

        #endregion

        private static void OnSceneUnloaded(Scene _)
        {
            foreach (var category in Categories)
                category.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ForceLoad() => _ = Categories;

        static K10DebugSystem()
        {
            config = K10DebugConfig.Load();
            getOwnerKey = DefaultGetOwnerKey;

            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
    }
}