using System;
using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    [CustomPropertyDrawer(typeof(ScopedAttribute))]
    public class ScopedPropertyDrawer : PropertyDrawer
    {
        private static (string, string, string, EColor) GetInfo(SerializedProperty property, ScopedAttribute scopedAtt)
        {
            var currentValue = property.GetValue();
            var editorInfo = currentValue as IContentEditorInfo;

            var name = scopedAtt.nameSource switch
            {
                EEditorInfoSource.Nothing => string.Empty,
                EEditorInfoSource.Property => property.displayName,
                EEditorInfoSource.FieldValue => currentValue?.ToString() ?? property.displayName,
                EEditorInfoSource.FieldType => currentValue?.GetType().Name ?? "Null",
                EEditorInfoSource.EditorContent => editorInfo?.ContentName ?? property.displayName,
                EEditorInfoSource.Provided => scopedAtt.name,
                _ => throw new ArgumentOutOfRangeException()
            };

            var append = scopedAtt.appendSource switch
            {
                EEditorInfoSource.Nothing => string.Empty,
                EEditorInfoSource.Property => currentValue?.GetType().Name ?? string.Empty,
                EEditorInfoSource.FieldValue => currentValue?.ToString() ?? property.displayName,
                EEditorInfoSource.FieldType => currentValue?.GetType().Name ?? "Null",
                EEditorInfoSource.EditorContent => editorInfo?.ContentSummary ?? string.Empty,
                EEditorInfoSource.Provided => scopedAtt.append,
                _ => throw new ArgumentOutOfRangeException()
            };

            var description = scopedAtt.descriptionSource switch
            {
                EEditorInfoSource.Nothing => string.Empty,
                EEditorInfoSource.Property => string.Empty,
                EEditorInfoSource.FieldValue => string.Empty,
                EEditorInfoSource.EditorContent => editorInfo?.ContentDescription ?? string.Empty,
                EEditorInfoSource.Provided => scopedAtt.description,
                _ => throw new ArgumentOutOfRangeException()
            };

            var color = scopedAtt.colorSource switch
            {
                EEditorInfoSource.Nothing or
                EEditorInfoSource.Property or
                EEditorInfoSource.FieldValue => EColor.Infer,
                EEditorInfoSource.EditorContent => editorInfo?.ContentColor ?? EColor.Infer,
                EEditorInfoSource.Provided => scopedAtt.color,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (color is EColor.Infer) color = scopedAtt.scopeType switch
            {
                EScopeType.Header => EColor.Primary,
                EScopeType.Foldout => EColor.Secondary,
                EScopeType.InlineHeader => EColor.Primary,
                EScopeType.Inline => EColor.Clear,
                _ => throw new ArgumentOutOfRangeException()
            };

            var hasName = !string.IsNullOrEmpty(name);
            var hasAppend = !string.IsNullOrEmpty(append);

            var title = hasName
                ? hasAppend ? name.AppendInfo(append, size: scopedAtt.elementSize) : name
                : hasAppend ? append : "Missing Name!";

            return (name, title, description, color);
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var scopedAtt = (ScopedAttribute)attribute;

            var isSerialized = property.IsManagedRef();

            var (name, title, description, color) = GetInfo(property, scopedAtt);

            if (isSerialized && SerializedRefLib.TryDrawMissingRef(ref rect, property, name))
                return;

            var info = scopedAtt.GetSkope(property, title, color);
            if (isSerialized && info.scopeType is not EScopeType.Inline)
                info.SetButtons(("⚙️", EColor.Special, () => SerializedRefLib.DrawTypePickerMenu(property)));

            using var scope = Skope.Open(ref rect, info);
            if (!scope.IsExpanded) return;

            if (!string.IsNullOrEmpty(description))
            {
                rect.ExtractLineDef(out var startX, out var totalWidth);
                EditorGUI.LabelField(rect, description, SkyxStyles.DefaultLabel);

                rect.NextLine(startX, totalWidth);
                SkyxGUI.Separator(ref rect);
            }

            property.DrawAllInnerProperties(ref rect, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var isSerialized = property.IsManagedRef();
            if (isSerialized && property.managedReferenceValue == null) return SkyxStyles.FullLineHeight;

            var scopedAtt = (ScopedAttribute)attribute;
            var (name, title, description, color) = GetInfo(property, scopedAtt);
            var info = scopedAtt.GetSkope(property, title, color);

            var height = Skope.ScopeHeight(info, property.isExpanded);

            if (property.isExpanded)
            {
                height += property.GetPropertyHeight(true);

                if (!string.IsNullOrEmpty(description))
                    height += SkyxStyles.FullLineHeight + 6;
            }

            return height;
        }
    }
}