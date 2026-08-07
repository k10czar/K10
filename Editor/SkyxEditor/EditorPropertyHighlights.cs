using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    public static class EditorPropertyHighlights
    {
        public static bool IsEnabled
        {
            get => EditorPrefs.GetBool("EditorPropertyHighlights_IsEnabled", false);
            set => EditorPrefs.SetBool("EditorPropertyHighlights_IsEnabled", value);
        }

        private static readonly HashSet<(int, string)> targets = new();

        public static bool IsLit(Object root, string path) => IsEnabled && targets.Contains((root.GetInstanceID(), path));
        public static bool IsLit((int, string) cacheKey) => IsEnabled && targets.Contains(cacheKey);
        public static bool IsLit(SerializedProperty property) => IsEnabled && targets.Contains(property.GetCacheID());

        public static void Add(Object root, string path) => targets.Add((root.GetInstanceID(), path));
        public static void Add((int, string) cacheKey) => targets.Add(cacheKey);
        public static void Add(SerializedProperty property) => targets.Add(property.GetCacheID());

        public static void Release(Object root, string path) => targets.Remove((root.GetInstanceID(), path));
        public static void Release((int, string) cacheKey) => targets.Remove(cacheKey);
        public static void Release(SerializedProperty property) => targets.Remove(property.GetCacheID());

        public static void Release(int mainCacheID)
        {
            var keysToRemove = targets.Where(k => k.Item1 == mainCacheID).ToList();
            foreach (var key in keysToRemove)
                targets.Remove(key);
        }

        public static void Clear() => targets?.Clear();
    }
}