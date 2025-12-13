using System;
using K10.DebugSystem;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class SerializedRefLib
    {
        #region Drawers

        public static void DrawDefaultInspector(Rect rect, SerializedProperty property, bool alwaysShowChangeRef = false, GUIContent labelContent = null)
        {
            if (!property.IsManagedRef())
            {
                SkyxGUI.Draw(rect, property, true);
                return;
            }

            var label = labelContent?.text ?? property.displayName;

            if (property.managedReferenceValue == null)
            {
                DrawMissingSerializedRef(ref rect, property, label);
                return;
            }

            if (!TryDrawWithCustomEditor(rect, property, labelContent))
                DrawManagedReference(ref rect, property, label, alwaysShowChangeRef);
        }

        private static bool TryDrawWithCustomEditor(Rect rect, SerializedProperty property, GUIContent label)
        {
            var customDrawer = CustomDrawersCache.Get(property);
            if (customDrawer == null) return false;

            try
            {
                customDrawer.OnGUI(rect, property, label);
            }
            catch (ExitGUIException)
            {
                // Ignore
            }
            catch (Exception exception)
            {
                CustomDrawersCache.ClearCache();
                SerializedPropertyExtension.ClearCache();
                PropertyCollection.ClearCollections();

                Debug.LogError($"Failed to draw managed property {property.propertyPath}. Clearing Caches!");
                Debug.LogException(exception);
            }

            return true;
        }

        private static void DrawMissingSerializedRef(ref Rect rect, SerializedProperty property, string label)
        {
            var text = $"{label} | MISSING REFERENCE!";

            if (SkyxGUI.Button(rect, text, EColor.Danger))
                DrawTypePickerMenu(rect, property);
        }

        private static void DrawManagedReference(ref Rect rect, SerializedProperty property, string label, bool alwaysShowChangeRef)
        {
            Rect? buttonRect = null;

            if (alwaysShowChangeRef)
            {
                bool clicked;
                (buttonRect, clicked) = RectLib.ExtractOverHeaderButton(rect, EElementSize.SingleLine);

                if (clicked)
                {
                    DrawTypePickerMenu(buttonRect.Value, property);
                    return;
                }
            }

            var currentValue = property.GetValue();
            var summarizable = currentValue as IContentSummary;
            var isSummarizable = summarizable != null;

            var info = isSummarizable ? summarizable.Summary : property.managedReferenceValue?.GetType().Name;
            var color = isSummarizable ? summarizable.SummaryColor : EColor.Secondary;

            label = label.AppendInfo(info, EColor.Support, EElementSize.SingleLine);

            using var scope = FoldoutScope.Open(ref rect, property, label, color, indent: true);
            if (alwaysShowChangeRef) SkyxGUI.Button(buttonRect.Value, "⚙️", EColor.Special, EElementSize.Mini);
            if (!scope.IsExpanded) return;

            if (isSummarizable && summarizable.Description != null)
            {
                rect.ExtractLineDef(out var startX, out var totalWidth);
                EditorGUI.LabelField(rect, summarizable.Description, SkyxStyles.DefaultLabel);

                rect.NextLine(startX, totalWidth);
                SkyxGUI.Separator(ref rect);
            }

            property.DrawAllInnerProperties(ref rect, true);
        }

        public static float GetPropertyHeight(SerializedProperty property)
        {
            return property.IsManagedRef()
                ? CalculateManagedRefHeight(property)
                : EditorGUI.GetPropertyHeight(property, true);
        }

        private static float CalculateManagedRefHeight(SerializedProperty property)
        {
            if (property.managedReferenceValue == null) return SkyxStyles.FullLineHeight;

            var customDrawer = CustomDrawersCache.Get(property);
            if (customDrawer != null) return customDrawer.GetPropertyHeight(property, null);

            if (!property.isExpanded) return SkyxStyles.ClosedScopeHeight(EElementSize.SingleLine);

            var height = SkyxStyles.ScopeTotalExtraHeight(EElementSize.SingleLine);
            height += property.GetPropertyHeight(true);

            var currentValue = property.GetValue();

            if (currentValue is IContentSummary summarizable && summarizable.Description != null)
                height += SkyxStyles.FullLineHeight + 6;

            return height;
        }

        #region Pickers

        private static Action<Type> currentCallback;

        public static void DrawTypePickerMenu(SerializedProperty property, Action<SerializedProperty> newElementSetup)
        {
            var mousePos = Event.current.mousePosition;
            var rect = new Rect(mousePos.x, mousePos.y, 1, 1);

            DrawTypePickerMenu(rect, property, newElementSetup);
        }

        private static void DrawTypePickerMenu(Rect rect, SerializedProperty property, Action<SerializedProperty> newElementSetup = null)
        {
            ClassTreePicker.Draw(rect, property.GetManagedType(), property.managedReferenceValue?.GetType(), OnTypeSelected);

            void OnTypeSelected(Type newSelection)
            {
                property.SetNewReferenceType(newSelection, true);
                property.ResetDefaultValues(newElementSetup, true);
            }
        }

        #endregion

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