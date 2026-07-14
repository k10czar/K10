using System;
using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    [CustomPropertyDrawer(typeof(ScopedAttribute))]
    public class ScopedPropertyDrawer : PropertyDrawer
    {
        #region Default Header Buttons

        public static readonly SkopeButton descriptionToggleSkopeButton = new("?", EColor.Info, _ => isShowingDescriptions = !isShowingDescriptions);
        public static readonly SkopeButton managedPickerSkopeButton = new("⚙️", EColor.Support, SerializedRefLib.ShowTypePicker);
        public static readonly SkopeButton arrayRemovalSkopeButton = new("X", EColor.Warning, SerializedPropertyExtension.RemoveSelfFromArrayDelayed);

        #endregion

        public static bool isShowingDescriptions;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
            => OnGUI(rect, property, (ScopedAttribute) attribute);

        protected void OnGUI(Rect rect, SerializedProperty property, ScopedAttribute scopedAtt)
        {
            var isSerialized = property.IsManagedRef();
            var info = scopedAtt.GetInfo(property);

            if (isSerialized)
            {
                if (SerializedRefLib.TryDrawMissingRef(ref rect, property, info.name)) return;

                if (info.buttons.Count == 0 && info.scopeType is not EScopeType.Inline)
                    info.buttons.Add(managedPickerSkopeButton);
            }

            var hasDescription = info.HasDescription;
            if (hasDescription)
            {
                descriptionToggleSkopeButton.color = isShowingDescriptions ? EColor.Info : EColor.Support;
                info.AddUniqueButton(descriptionToggleSkopeButton);
            }

            using var scope = Skope.Open(ref rect, info);
            if (!scope.IsExpanded) return;

            if (isShowingDescriptions && hasDescription)
            {
                rect.height = SkyxStyles.GetHelpBoxHeight(info.description.LineCount(), false);
                EditorGUI.HelpBox(rect, info.description, MessageType.Info);
                rect.SlideSameVertically();
                rect.AdjustToLine();
            }

            DrawContent(ref rect, property, info);
        }

        protected virtual void DrawContent(ref Rect rect, SerializedProperty property, SkopeInfo info)
        {
            if (property.hasVisibleChildren)
            {
                EditorGUI.BeginDisabledGroup(info.contentIsDisabled);
                property.DrawAllInnerProperties(ref rect, true);
                EditorGUI.EndDisabledGroup();
            }
            else if (info.scopeType.ShowNoChildProperties())
                EditorGUI.LabelField(rect, "No properties.");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => GetPropertyHeight(property, (ScopedAttribute) attribute);

        protected float GetPropertyHeight(SerializedProperty property, ScopedAttribute scopedAtt)
        {
            var isSerialized = property.IsManagedRef();
            if (isSerialized && property.managedReferenceValue == null) return SkyxStyles.FullLineHeight;

            var info = scopedAtt.GetInfo(property);
            var height = Skope.ScopeHeight(info, property.isExpanded);

            if (property.isExpanded)
            {
                if (isShowingDescriptions && info.HasDescription)
                    height += SkyxStyles.GetHelpBoxHeight(info.description.LineCount(), true);

                height += GetContentHeight(property, info);
            }

            return height;
        }

        protected virtual float GetContentHeight(SerializedProperty property, SkopeInfo info)
        {
            var height = property.GetPropertyHeight(true);

            if (info.scopeType.ShowNoChildProperties() && !property.hasVisibleChildren)
                height += SkyxStyles.FullLineHeight;

            return height;
        }
    }
}