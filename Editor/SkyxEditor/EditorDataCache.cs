using K10.Common;
using UnityEditor;

namespace Rogue.REditor
{
    public static class EditorDataCache
    {
        private static readonly AnyStore<(int, string)> cache = new();

        public static T Get<T>((int, string) cacheKey) => cache.Get<T>(cacheKey);
        public static T Get<T>(SerializedProperty property) => cache.Get<T>(property.GetCacheID());

        public static bool TryGet<T>((int, string) cacheKey, out T value) => cache.TryGet(cacheKey, out value);
        public static bool TryGet<T>(SerializedProperty property, out T value) => cache.TryGet(property.GetCacheID(), out value);

        public static void Cache((int, string) cacheKey, object value) => cache.Store(cacheKey, value, true);
        public static void Cache(SerializedProperty property, object value) => cache.Store(property.GetCacheID(), value, true);

        public static void Release((int, string) cacheKey) => cache.Release(cacheKey);
        public static void Release(SerializedProperty property) => cache.Release(property.GetCacheID());

        public static void Clear() => cache?.Clear();
    }
}