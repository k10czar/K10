using System.Linq;
using K10.Common;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    public static class EditorDataCache
    {
        private static readonly AnyStore<(int, string)> cache = new();

        public static T Get<T>(Object root, string path) => cache.Get<T>((root.GetInstanceID(), path));
        public static T Get<T>((int, string) cacheKey) => cache.Get<T>(cacheKey);
        public static T Get<T>(SerializedProperty property) => cache.Get<T>(property.GetCacheID());

        public static bool TryGet<T>(Object root, string path, out T value) => cache.TryGet((root.GetInstanceID(), path), out value);
        public static bool TryGet<T>((int, string) cacheKey, out T value) => cache.TryGet(cacheKey, out value);
        public static bool TryGet<T>(SerializedProperty property, out T value) => cache.TryGet(property.GetCacheID(), out value);

        public static void Cache(Object root, string path, object value) => cache.Store((root.GetInstanceID(), path), value, true);
        public static void Cache((int, string) cacheKey, object value) => cache.Store(cacheKey, value, true);
        public static void Cache(SerializedProperty property, object value) => cache.Store(property.GetCacheID(), value, true);

        public static void Release(Object root, string path) => cache.Release((root.GetInstanceID(), path));
        public static void Release((int, string) cacheKey) => cache.Release(cacheKey);
        public static void Release(SerializedProperty property) => cache.Release(property.GetCacheID());

        // Global Keys
        public static bool TryGet<T>(string globalKey, out T value) => cache.TryGet((0, globalKey), out value);
        public static void Cache(string globalKey, object value) => cache.Store((0, globalKey), value, true);
        public static void Release(string globalKey) => cache.Release((0, globalKey));

        public static void Release(int mainCacheID)
        {
            var keysToRemove = cache.Keys.Where(k => k.Item1 == mainCacheID).ToList();
            foreach (var key in keysToRemove)
                cache.Release(key);
        }

        public static void Clear() => cache?.Clear();
    }
}