using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class CustomDrawersCache
    {
        private static readonly Dictionary<Type, PropertyEditor> cache = new();

        public static PropertyEditor Get(SerializedProperty property)
        {
            var fieldType = property.GetCachedType();
            if (fieldType == null) return null;

            if (cache.TryGetValue(fieldType, out var cachedDrawer)) return cachedDrawer;

            var drawer = FindPropertyDrawerForType(fieldType);
            cache[fieldType] = drawer;
            return drawer;
        }

        public static bool TryResetNewElement(SerializedProperty newElement)
        {
            var drawer = Get(newElement);
            var wasReset = drawer != null && drawer.ResetNewObject(newElement);

            return wasReset;
        }

        private static PropertyEditor FindPropertyDrawerForType(Type target)
        {
            if (IsIgnoredAssemblyForDrawers(target.Assembly)) return null;

            List<(Type drawerType, Type targetType)> potentialDrawers = new();

            var propertyEditorTypes = TypeListDataCache.GetFrom(typeof(PropertyEditor)).GetTypes();
            foreach (var drawerType in propertyEditorTypes)
            {
                var customAttributes = drawerType.GetCustomAttributes<CustomPropertyDrawer>(true);
                if (!customAttributes.Any()) continue;

                foreach (var candidate in customAttributes)
                {
                    var drawerTargetType = GetDrawerTargetType(candidate);
                    if (drawerTargetType == null) continue;

                    if (drawerTargetType.IsSubclassOf(typeof(PropertyAttribute))) continue;

                    if (drawerTargetType.IsAssignableFrom(target))
                        potentialDrawers.Add((drawerType, drawerTargetType));
                }
            }

            if (potentialDrawers.Count == 0) return null;

            var bestMatch = potentialDrawers[0];
            foreach (var candidate in potentialDrawers)
            {
                if (candidate.targetType == target)
                {
                    bestMatch = candidate;
                    break;
                }

                if (bestMatch.targetType.IsAssignableFrom(candidate.targetType))
                    bestMatch = candidate;
            }

            try
            {
                return (PropertyEditor) Activator.CreateInstance(bestMatch.drawerType);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create property drawer {bestMatch.drawerType.Name}: {e}");
                return null;
            }
        }

        private static bool IsIgnoredAssemblyForDrawers(Assembly assembly)
        {
            var assemblyName = assembly.FullName;
            return assemblyName.StartsWith("UnityEngine") ||
                   assemblyName.StartsWith("UnityEditor") ||
                   assemblyName.StartsWith("System") ||
                   assemblyName.StartsWith("mscorlib") ||
                   assemblyName.StartsWith("netstandard");
        }

        private static Type GetDrawerTargetType(CustomPropertyDrawer drawerAttribute)
        {
            var typeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
            return typeField?.GetValue(drawerAttribute) as Type;
        }

        [MenuItem("Disyphus/Editor/Clear CustomDrawersCache")]
        private static void ClearSerializedRefDrawers()
        {
            cache.Clear();
        }
    }
}