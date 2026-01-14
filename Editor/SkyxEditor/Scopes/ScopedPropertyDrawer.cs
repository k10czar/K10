using System;
using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    [CustomPropertyDrawer(typeof(ScopedAttribute))]
    public class ScopedPropertyDrawer : PropertyDrawer
    {
        #region Default Header Buttons

        public static readonly (string, EColor, Action) defaultDescriptionToggle = ("?", EColor.Info, () => isShowingDescriptions = !isShowingDescriptions);
        public static (string, EColor, Action) GetManagedPicker(SerializedProperty property) => ("⚙️", EColor.Support, () => SerializedRefLib.DrawTypePickerMenu(property));
        public static (string, EColor, Action) GetArrayRemoval(SerializedProperty property) => ("X", EColor.Warning, property.RemoveSelfFromArrayDelayed);

        #endregion

        public static bool isShowingDescriptions;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
            => OnGUI(rect, property, (ScopedAttribute) attribute);

        protected static void OnGUI(Rect rect, SerializedProperty property, ScopedAttribute scopedAtt)
        {
            var isSerialized = property.IsManagedRef();
            var info = scopedAtt.GetInfo(property);

            if (isSerialized)
            {
                if (SerializedRefLib.TryDrawMissingRef(ref rect, property, info.name)) return;

                if (info.buttons.Count == 0 && info.scopeType is not EScopeType.Inline)
                    info.buttons.Add(GetManagedPicker(property));
            }

            var hasDescription = info.HasDescription;
            if (hasDescription) info.AddUniqueButton(defaultDescriptionToggle);

            using var scope = Skope.Open(ref rect, info);
            if (!scope.IsExpanded) return;

            if (isShowingDescriptions && hasDescription)
            {
                rect.AdjustToLine();
                EditorGUI.LabelField(rect, info.description, SkyxStyles.DefaultLabel);
                rect.NextSameLine();
                SkyxGUI.Separator(ref rect);
            }

            if (property.hasVisibleChildren)
            {
                EditorGUI.BeginDisabledGroup(info.isDisabled);
                property.DrawAllInnerProperties(ref rect, true);
                EditorGUI.EndDisabledGroup();
            }
            else if (scopedAtt.scopeType.ShowNoChildProperties())
                EditorGUI.LabelField(rect, "No properties.");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => GetPropertyHeight(property, (ScopedAttribute) attribute);

        protected static float GetPropertyHeight(SerializedProperty property, ScopedAttribute scopedAtt)
        {
            var isSerialized = property.IsManagedRef();
            if (isSerialized && property.managedReferenceValue == null) return SkyxStyles.FullLineHeight;

            var info = scopedAtt.GetInfo(property);
            var height = Skope.ScopeHeight(info, property.isExpanded);

            if (property.isExpanded)
            {
                height += property.GetPropertyHeight(true);

                if (isShowingDescriptions && info.HasDescription)
                    height += SkyxStyles.FullLineHeight + 6;

                if (info.scopeType.ShowNoChildProperties() && !property.hasVisibleChildren)
                    height += SkyxStyles.FullLineHeight;
            }

            return height;
        }
    }
}