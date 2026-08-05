using System;
using System.Collections.Generic;
using Rogue.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    public static class SerializedRefLib
    {
        #region Pickers

        public static Action<SerializedProperty> onPickedType;

        public static bool TryDrawMissingRef(ref Rect rect, SerializedProperty property, string label = null)
            => TryDrawMissingRef(ref rect, property, label ?? property.displayName, !property.IsArrayEntry());

        public static bool TryDrawMissingRef(ref Rect rect, SerializedProperty property, SerializedRefOptionsAttribute optionsAtt)
            => TryDrawMissingRef(ref rect, property, property.displayName, !property.IsArrayEntry(), optionsAtt);

        public static bool TryDrawMissingRef(ref Rect rect, SerializedProperty property, string label, bool drawSeparateLabel, SerializedRefOptionsAttribute optionsAtt = null)
        {
            if (!property.IsManagedRef() || property.managedReferenceValue != null) return false;

            var myRect = rect;
            rect.NextSameLine();
            myRect.AdjustToLine(false);

            string text;
            var color = EColor.Danger;

            if (drawSeparateLabel)
            {
                SkyxGUI.DrawLabel(ref myRect, label);
                text = "MISSING REFERENCE!";
            }
            else text = label != null ? $"{label} | MISSING REFERENCE!" : "MISSING REFERENCE!";

            if (optionsAtt is { canBeNull: true })
            {
                color = EColor.Support;
                text = optionsAtt.nullLabel;
            }

            if (SkyxGUI.Button(myRect, text, color))
                ShowTypePicker(myRect, property);

            return true;
        }

        public static void DelayedTypePicker(SerializedProperty property, Action<SerializedProperty> newElementSetup = null, IEnumerable<Type> validTypes = null)
        {
            EditorUtils.RunDelayedOnce(() => ShowTypePicker(property, newElementSetup, validTypes));
        }

        public static void ShowTypePicker(SerializedProperty property) => ShowTypePicker(property, null, null);

        public static void ShowTypePicker(SerializedProperty property, Action<SerializedProperty> newElementSetup, IEnumerable<Type> validTypes = null)
            => ShowTypePicker(EditorUtils.GetRectAtMouse(), property, newElementSetup, validTypes);

        public static void ShowTypePicker(Rect rect, SerializedProperty property, Action<SerializedProperty> newElementSetup = null, IEnumerable<Type> validTypes = null)
        {
            ClassTreePicker.Draw(rect, property.GetManagedType(), property.managedReferenceValue?.GetType(), OnTypeSelected, validTypes);

            void OnTypeSelected(Type newSelection)
            {
                property.SetNewReferenceType(newSelection, true);
                property.ResetDefaultValues(newElementSetup, true, true);
                onPickedType?.Invoke(property);
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