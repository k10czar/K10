using System;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class SerializedRefLib
    {
        #region Pickers

        private static Action<Type> currentCallback;

        public static bool TryDrawMissingRef(ref Rect rect, SerializedProperty property, string label = null)
        {
            if (property.managedReferenceValue != null) return false;

            var text = label != null ? $"{label} | MISSING REFERENCE!" : "MISSING REFERENCE!";

            if (SkyxGUI.Button(rect, text, EColor.Danger))
                DrawTypePickerMenu(rect, property);

            return true;
        }

        public static void DrawTypePickerMenu(SerializedProperty property, Action<SerializedProperty> newElementSetup)
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
                property.ResetDefaultValues(newElementSetup, true);
            }
        }

        #endregion

        #region Extensions

        private static void SetNewReferenceType(this SerializedProperty property, Type newType, bool forceRecreate)
        {
            var currentType = property.managedReferenceValue?.GetType();
            if (!forceRecreate && currentType == newType) return;

            property.managedReferenceValue = newType != null ? Activator.CreateInstance(newType) : null;
            property.isExpanded = true;
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