using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class SerializedRefLib
    {
        #region Drawers

        public static void DrawDefaultInspector(Rect rect, SerializedProperty property, bool boxManagedReferences, string label = null)
        {
            if (property.IsManagedRef())
            {
                if (property.managedReferenceValue == null)
                {
                    DrawMissingSerializedRef(ref rect, property);
                    return;
                }

                var customDrawer = GetCachedPropertyDrawer(property);
                if (customDrawer != null) customDrawer.OnGUI(rect, property, new GUIContent(label));
                else if (boxManagedReferences) DrawManagedReferenceBoxed(ref rect, property, label);
                else DrawManagedReference(ref rect, property);
            }
            else SkyxGUI.Draw(rect, property, true);
        }

        private static void DrawMissingSerializedRef(ref Rect rect, SerializedProperty property)
            => DrawTypePickerButton(ref rect, "MISSING REFERENCE!", property, EColor.Danger);

        private static void DrawManagedReferenceBoxed(ref Rect rect, SerializedProperty property, string label)
        {
            label ??= property.displayName.AppendInfo(property.managedReferenceValue?.GetType().Name, false, EColor.Warning);
            using var scope = FoldoutScope.Open(ref rect, property, label, indent: true);
            if (!scope.isExpanded) return;

            DrawManagedReference(ref rect, property);
        }

        private static void DrawManagedReference(ref Rect rect, SerializedProperty property)
        {
            var iterator = property.Copy();
            var endProperty = iterator.GetEndProperty();

            iterator.NextVisible(true); // Skip the managed reference wrapper

            while (true)
            {
                if (SerializedProperty.EqualContents(iterator, endProperty)) break;

                rect.height = EditorGUI.GetPropertyHeight(iterator, true);
                SkyxGUI.Draw(rect, iterator, true);
                rect.y += rect.height + SkyxStyles.ElementsMargin;

                if (!iterator.NextVisible(false)) break;
            }
        }

        public static float GetPropertyHeight(SerializedProperty property, bool boxManagedReferences)
        {
            return property.IsManagedRef()
                ? CalculateManagedRefHeight(property, boxManagedReferences)
                : EditorGUI.GetPropertyHeight(property, true);
        }

        private static float CalculateManagedRefHeight(SerializedProperty property, bool boxManagedReferences)
        {
            if (property.managedReferenceValue == null) return SkyxStyles.FullLineHeight;

            var customDrawer = GetCachedPropertyDrawer(property);
            if (customDrawer != null) return customDrawer.GetPropertyHeight(property, null);

            if (boxManagedReferences && !property.isExpanded) return SkyxStyles.ClosedScopeHeight(EElementSize.SingleLine);

            var height = boxManagedReferences
                ? SkyxStyles.ScopeTotalExtraHeight(EElementSize.SingleLine)
                : 0;

            var iterator = property.Copy();
            var endProperty = iterator.GetEndProperty();

            iterator.NextVisible(true); // Skip the managed reference wrapper

            while (true)
            {
                if (SerializedProperty.EqualContents(iterator, endProperty)) break;
                height += EditorGUI.GetPropertyHeight(iterator, true) + SkyxStyles.ElementsMargin;
                if (!iterator.NextVisible(false)) break;
            }

            return height;
        }

        #region Pickers

        private static Action<Type> currentCallback;

        public static void DrawTypePickerMenu(SerializedProperty property, Action<SerializedProperty> newElementSetup = null)
        {
            var mousePos = Event.current.mousePosition;
            var rect = new Rect(mousePos.x, mousePos.y, 1, 1);

            DrawTypePickerMenu(rect, property, newElementSetup);
        }

        public static void DrawTypePickerMenu(Rect rect, SerializedProperty property, Action<SerializedProperty> newElementSetup = null)
        {
            ClassTreePicker.Draw(rect, property.GetManagedType(), property.managedReferenceValue?.GetType(), OnTypeSelected);

            void OnTypeSelected(Type newSelection)
            {
                property.SetNewReferenceType(newSelection, true);
                newElementSetup?.Invoke(property);
            }
        }

        public static void DrawTypePickerButton(ref Rect rect, string label, SerializedProperty property, EColor color)
        {
            using var backgroundScope = BackgroundColorScope.Set(color);
            if (EditorGUI.DropdownButton(rect, new GUIContent(label), FocusType.Passive))
                DrawTypePickerMenu(rect, property);
        }

        #endregion

        #region Custom Drawers

        private static readonly Dictionary<Type, PropertyDrawer> customDrawerCache = new();

        private static PropertyDrawer GetCachedPropertyDrawer(SerializedProperty property)
        {
            var fieldType = property.GetCachedType();
            if (customDrawerCache.TryGetValue(fieldType, out var cachedDrawer)) return cachedDrawer;

            var drawer = FindPropertyDrawerForType(fieldType);
            customDrawerCache[fieldType] = drawer;
            return drawer;
        }

        private static PropertyDrawer FindPropertyDrawerForType(Type type)
        {
            if (IsIgnoredAssemblyForDrawers(type.Assembly)) return null;

            List<(Type drawerType, Type targetType)> potentialDrawers = new();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsIgnoredAssemblyForDrawers(assembly)) continue;

                foreach (var drawerType in assembly.GetTypes())
                {
                    var customAttributes = drawerType.GetCustomAttributes(typeof(CustomPropertyDrawer), true);
                    if (customAttributes.Length == 0) continue;

                    foreach (CustomPropertyDrawer candidate in customAttributes)
                    {
                        var drawerTargetType = GetDrawerTargetType(candidate);
                        if (drawerTargetType == null) continue;

                        if (drawerTargetType.IsSubclassOf(typeof(PropertyAttribute))) continue;

                        if (drawerTargetType.IsAssignableFrom(type))
                            potentialDrawers.Add((drawerType, drawerTargetType));
                    }
                }
            }

            if (potentialDrawers.Count == 0) return null;

            var bestMatch = potentialDrawers[0];
            foreach (var candidate in potentialDrawers)
            {
                if (candidate.targetType == type)
                {
                    bestMatch = candidate;
                    break;
                }

                if (bestMatch.targetType.IsAssignableFrom(candidate.targetType))
                    bestMatch = candidate;
            }

            try
            {
                return (PropertyDrawer)Activator.CreateInstance(bestMatch.drawerType);
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

        #endregion

        #endregion

        #region Extensions

        public static void SetNewReferenceType(this SerializedProperty property, Type newType, bool forceRecreate)
        {
            var currentType = property.managedReferenceValue?.GetType();
            if (!forceRecreate && currentType == newType) return;

            property.managedReferenceValue = newType != null ? Activator.CreateInstance(newType) : null;
            property.Apply();
        }

        public static bool IsManagedRef(this SerializedProperty property) => property.propertyType == SerializedPropertyType.ManagedReference;

        #endregion

        #region Utils

        private static Type GetManagedType(this SerializedProperty prop)
        {
            var assType = prop.managedReferenceFieldTypename;
            var split = assType.Split(' ');

            if (split.Length <= 0) return null;
            if (split.Length == 1) return TypeFinder.WithName(split[0]);

            var assemblyName = split[0];
            if (split.Length > 2)
            {
                var cut = split[0].Length + 1;
                var fullTypeName = assType.Substring(split[0].Length + 1, assType.Length - cut);
                return TypeFinder.WithNameFromAssembly(fullTypeName, assemblyName);
            }

            var typeName = split[1];
            var type = TypeFinder.WithNameFromAssembly(typeName, assemblyName);
            return type;
        }

        #endregion
    }
}