using Rogue.RuntimeEditor;
using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    [CustomPropertyDrawer(typeof(ScopedAttribute))]
    public class SkopedDrawer : PropertyDrawer
    {
        #region Default Skope Buttons

        public static readonly SkopeButton DescriptionToggleSkopeButton = new("?", EColor.Info, _ => isShowingDescriptions = !isShowingDescriptions);
        public static readonly SkopeButton ManagedPickerSkopeButton = new("⚙️", EColor.Support, SerializedRefLib.ShowTypePicker);
        public static readonly SkopeButton ArrayRemovalSkopeButton = new("X", EColor.Warning, SerializedPropertyExtension.RemoveSelfFromArrayDelayed);

        #endregion

        public static bool isShowingDescriptions;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
            => OnGUI(rect, property, (ScopedAttribute) attribute);

        protected void OnGUI(Rect rect, SerializedProperty property, ScopedAttribute scopedAtt)
        {
            SkopeOverride.TryGetOverride(property, out var overrideData);
            fieldInfo.TryGetAttribute(out SerializedRefOptionsAttribute optionsAtt);

            var info = scopedAtt.GetInfo(property, overrideData);

            if (property.IsManagedRef())
            {
                using (new EditorGUI.DisabledGroupScope(info.contentIsDisabled))
                {
                    if (SerializedRefLib.TryDrawMissingRef(ref rect, property, info.name, !property.IsArrayEntry(), optionsAtt))
                        return;
                }

                if (info.buttons.Count == 0 && info.scopeType is not EScopeType.Inline)
                    info.buttons.Add(ManagedPickerSkopeButton);
            }

            if (info.autoAddSkopeButtons)
            {
                if (info.HasDescription)
                {
                    DescriptionToggleSkopeButton.color = isShowingDescriptions ? EColor.Info : EColor.Support;
                    info.AddUniqueButton(DescriptionToggleSkopeButton);
                }

                if (property.IsArrayEntry()) info.AddUniqueButton(ArrayRemovalSkopeButton);
            }

            using var scope = Skope.Open(ref rect, info);
            if (!scope.IsExpanded) return;

            if (isShowingDescriptions && info.HasDescription)
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

            SkopeOverride.TryGetOverride(property, out var overrideData);
            var info = scopedAtt.GetInfo(property, overrideData);
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