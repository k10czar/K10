using System;
using System.Collections.Generic;
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
                if (boxManagedReferences) DrawManagedReferenceBoxed(ref rect, property, label);
                else DrawManagedReference(ref rect, property);
            }
            else SkyxGUI.Draw(rect, property, true);
        }

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

        private static readonly Dictionary<Type, GenericMenu> menuCache = new();
        private static Action<Type> currentCallback;

        private static GenericMenu GetTypePickerMenu(Type target, Action<Type> onSelection)
        {
            currentCallback = onSelection;
            if (menuCache.TryGetValue(target, out var menu)) return menu;

            var newMenu = new GenericMenu();

            newMenu.AddItem(new GUIContent("None"), false, () => currentCallback(null));

            var listing = TypeListDataCache.GetFrom(target);
            var candidates = listing.GetTypes();
            var guis = listing.GetGUIs();

            for (var index = 0; index < candidates.Length; index++)
            {
                var candidate = candidates[index];
                var gui = guis[index];

                newMenu.AddItem(gui, false, () => currentCallback(candidate));
            }

            menuCache[target] = newMenu;
            return newMenu;
        }

        public static void DrawTypePickerMenu(SerializedProperty property, Action<SerializedProperty> newElementSetup)
            => DrawTypePickerMenu(property.GetManagedType(), selectedType =>
            {
                property.SetNewReferenceType(selectedType, true);
                newElementSetup.Invoke(property);
            });

        public static void DrawTypePickerMenu(Type pickerTarget, Action<Type> onSelection)
        {
            var menu = GetTypePickerMenu(pickerTarget, onSelection);
            menu.ShowAsContext();
        }

        public static void DrawTypePickerButton(ref Rect rect, string label, Type pickerTarget, Action<Type> onSelection)
        {
            if (EditorGUI.DropdownButton(rect, new GUIContent(label), FocusType.Keyboard))
                DrawTypePickerMenu(pickerTarget, onSelection);
        }

        public static bool DrawTypePickerButton(ref Rect rect, Type pickerTarget, Type currentSelection, out Type newSelection)
        {
            var listing = TypeListDataCache.GetFrom(pickerTarget);
            var candidates = listing.GetTypes();

            var currentIndex = Array.IndexOf(candidates, currentSelection);
            var newIndex = EditorGUI.Popup(rect, currentIndex, listing.GetGUIs());

            newSelection = candidates[newIndex];
            return newIndex != currentIndex;
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