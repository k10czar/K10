using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rogue.REditor
{
    public static class SerializedTypeCache
    {
        private static readonly Dictionary<(int, string), Type> cache = new();

        public static Type GetCachedType(this SerializedProperty property)
        {
            var cacheID = property.GetCacheID();
            if (cache.TryGetValue(cacheID, out var cachedType))
                return cachedType;

            var value = property.GetValue();
            if (value == null) return null;

            var type = value.GetType();
            cache.Add(cacheID, type);

            return type;
        }

        public static Type GetObjectReferenceType(this SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                Debug.LogError($"Trying to get type from non-ObjectReference property: {property.propertyPath}");
                return null;
            }

            return property.objectReferenceValue != null
                ? property.objectReferenceValue.GetType()
                : GetTypeFromPropertyTypeString(property);
        }

        private static Type GetTypeFromPropertyTypeString(SerializedProperty property)
        {
            var typeName = property.type;

            if (string.IsNullOrEmpty(typeName) || !typeName.StartsWith("PPtr<") || !typeName.EndsWith(">"))
            {
                Debug.LogError($"Don't know how to parse property type: {typeName}");
                return null;
            }

            typeName = typeName.Substring(5, typeName.Length - 6).TrimStart('$');

            foreach (var type in TypeCache.GetTypesDerivedFrom<Object>())
            {
                if (type.Name == typeName) return type;
            }

            Debug.LogError($"Couldn't find type matching property type: {typeName}");
            return null;
        }

        public static void InvalidateCacheFromTarget(int mainCacheID)
        {
            var keysToRemove = cache.Keys.Where(k => k.Item1 == mainCacheID).ToList();

            foreach (var key in keysToRemove)
                cache.Remove(key);
        }

        [MenuItem("Rogue/Editor/Clear SerializedType Cache")]
        public static void Clear() => cache.Clear();

        static SerializedTypeCache() => Selection.selectionChanged += Clear;
    }
}