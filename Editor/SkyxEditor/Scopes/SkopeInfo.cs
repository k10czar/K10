using System;
using System.Collections.Generic;
using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    public class SkopeInfo
    {
        public readonly EScopeType scopeType;
        public readonly EColor color;
        public readonly EElementSize size;

        public readonly SerializedProperty property;

        public readonly string name;
        public readonly string title;
        public readonly string description;
        public readonly bool hasCustomExpand;

        public readonly bool indent = false;
        public readonly bool isDisabled = false;

        public bool HasDescription => !string.IsNullOrEmpty(description);
        public bool CanExpand() => hasCustomExpand || HasDescription || property.CanExpand();

        #region Buttons

        public List<SkopeButton> buttons = new();

        public void AddUniqueButton(SkopeButton entry)
        {
            if (buttons.Contains(entry)) return;
            buttons.Add(entry);
        }

        public void DrawButtons(Rect rect, bool reallyDraw)
        {
            if (buttons == null) return;

            EditorGUI.BeginDisabledGroup(isDisabled);

            rect.y += 5;
            rect.x -= 4;
            rect.height = SkyxStyles.LineHeight;

            foreach (var (label, color, action) in buttons)
            {
                if (reallyDraw) SkyxGUI.MiniButton(ref rect, label, color, null, true);
                else
                {
                    var buttonRect = rect.ExtractMiniButton(true);
                    if (buttonRect.TryUseClick(false))
                        action(property);
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        #endregion

        #region Constructors

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, string title, EColor color, EElementSize size)
        {
            this.scopeType = scopeType;
            this.property = property;
            this.title = title;
            this.color = color;
            this.size = size;
        }

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, EColor color, EElementSize size)
        {
            this.scopeType = scopeType;
            this.property = property;
            this.title = property.displayName;
            this.color = color;
            this.size = size;
        }

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, string title, EColor color, EElementSize size, bool indent, bool isDisabled = false, bool hasCustomExpand = false)
        {
            this.scopeType = scopeType;
            this.property = property;
            this.name = title;
            this.title = title;
            this.color = color;
            this.size = size;
            this.isDisabled = isDisabled;
            this.indent = indent;
            this.hasCustomExpand = hasCustomExpand;
        }

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, string name, string title, string description, EColor color, EElementSize size, bool indent, bool isDisabled)
        {
            this.scopeType = scopeType;
            this.property = property;
            this.name = name;
            this.title = title;
            this.description = description;
            this.color = color;
            this.size = size;
            this.indent = indent;
            this.isDisabled = isDisabled;
        }

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, EColor color, EElementSize size, bool indent)
        {
            this.scopeType = scopeType;
            this.property = property;
            this.title = property.displayName;
            this.color = color;
            this.size = size;
            this.indent = indent;
        }

        #endregion
    }

    public static class SkopeInfoExtensions
    {
        public static SkopeInfo GetInfo(this ScopedAttribute scopedAtt, SerializedProperty property)
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
                EEditorInfoSource.EditorAltContent => editorInfo?.ContentSummary ?? property.displayName,
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
                EEditorInfoSource.EditorAltContent => editorInfo?.ContentName ?? property.displayName,
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

            var color = scopedAtt.isDisabled
                ? EColor.Disabled
                : scopedAtt.colorSource switch
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
                ? hasAppend ? name.AppendInfo(append, scopedAtt.elementSize) : name
                : hasAppend ? append : "Missing Name!";

            var info = new SkopeInfo(scopedAtt.scopeType, property, name, title, description, color, scopedAtt.elementSize, scopedAtt.indent, scopedAtt.isDisabled);
            if (scopedAtt.buttons != null) info.buttons.InsertRange(0, scopedAtt.buttons);

            return info;
        }
    }
}