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

        public readonly bool indent;
        public readonly bool contentIsDisabled;
        public bool autoAddSkopeButtons = true;

        public bool HasDescription => !string.IsNullOrEmpty(description);
        public bool CanExpand() => hasCustomExpand || HasDescription || property.CanExpand();

        #region Buttons

        public bool buttonsAreDisabled;
        public List<SkopeButton> buttons = new();

        public void AddUniqueButton(SkopeButton entry)
        {
            if (buttons.Contains(entry)) return;
            buttons.Add(entry);
        }

        public void DrawButtons(Rect rect, bool draw, bool getClicks)
        {
            if (buttons == null) return;

            EditorGUI.BeginDisabledGroup(buttonsAreDisabled);

            var (deltaX, deltaY) = size switch
            {
                EElementSize.Primary => (-4, 5),
                EElementSize.Secondary => (-4, 4),
                EElementSize.SingleLine => (-4, 1),
                _ => throw new ArgumentOutOfRangeException()
            };

            rect.x += deltaX;
            rect.y += deltaY;
            rect.height = SkyxStyles.LineHeight;

            foreach (var button in buttons)
            {
                if (button.isDisabled) continue;

                var buttonRect = rect.ExtractMiniButton(true);

                if (draw) SkyxGUI.Button(buttonRect, button.label, button.color, EElementSize.Mini, EButtonType.Default, button.tooltip);

                if (getClicks && buttonRect.TryUseClick(false))
                    button.onClick(property);
            }

            EditorGUI.EndDisabledGroup();
        }

        #endregion

        #region Constructors

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, string title, EColor color = EColor.Infer, EElementSize size = EElementSize.Infer)
        {
            this.scopeType = scopeType;
            this.property = property;
            this.title = title;
            this.color = color is EColor.Infer ? scopeType.InferColor() : color;
            this.size = size is EElementSize.Infer ? scopeType.PreferredSize() : size;
        }

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, EColor color = EColor.Infer, EElementSize size = EElementSize.Infer)
            : this(scopeType, property, property.displayName, color, size) {}

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, string title, EColor color, EElementSize size, bool indent, bool contentIsDisabled = false, bool hasCustomExpand = false)
            : this(scopeType, property, title, color, size)
        {
            this.name = title;
            this.contentIsDisabled = contentIsDisabled;
            this.indent = indent;
            this.hasCustomExpand = hasCustomExpand;
        }

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, string name, string title, string description, EColor color, EElementSize size, bool indent, bool contentIsDisabled)
            : this(scopeType, property, title, color, size)
        {
            this.name = name;
            this.description = description;
            this.indent = indent;
            this.contentIsDisabled = contentIsDisabled;
        }

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, EColor color, EElementSize size, bool indent)
            : this(scopeType, property, property.displayName, color, size)
        {
            this.indent = indent;
        }

        #endregion
    }

    public static class SkopeInfoExtensions
    {
        public static SkopeInfo GetInfo(this ScopedAttribute scopedAtt, SerializedProperty property, SkopeOverride skopeOverride = null)
        {
            var currentValue = property.GetValue();
            var editorInfo = currentValue as IContentEditorInfo;
            var hasOverrides = skopeOverride != null;

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
                _ when hasOverrides && !string.IsNullOrEmpty(skopeOverride.appendTitle) => skopeOverride.appendTitle,
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

            var color = scopedAtt.colorSource switch
            {
                _ when EditorPropertyHighlights.IsLit(property) => EColor.Special,
                _ when hasOverrides => skopeOverride.color,
                EEditorInfoSource.Provided => scopedAtt.color,
                _ when scopedAtt.isDisabled => EColor.Disabled,
                EEditorInfoSource.Nothing or
                EEditorInfoSource.Property or
                EEditorInfoSource.FieldValue => EColor.Infer,
                EEditorInfoSource.EditorContent => editorInfo?.ContentColor ?? EColor.Infer,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (color is EColor.Infer) color = scopedAtt.scopeType.InferColor();

            var hasName = !string.IsNullOrEmpty(name);
            var hasAppend = !string.IsNullOrEmpty(append);

            var title = hasName
                ? hasAppend ? name.AppendInfo(append, scopedAtt.elementSize) : name
                : "Missing Name!";

            var contentIsDisabled = hasOverrides ? skopeOverride.disableInput : scopedAtt.isDisabled;

            var info = new SkopeInfo(scopedAtt.scopeType, property, name, title, description, color, scopedAtt.elementSize, scopedAtt.indent, contentIsDisabled)
            {
                autoAddSkopeButtons = scopedAtt.autoAddSkopeButtons,
            };

            if (hasOverrides && skopeOverride.ForcesButtons) info.buttons = skopeOverride.buttons;
            else if (scopedAtt.buttons != null) info.buttons.InsertRange(0, scopedAtt.buttons);
            else if (scopedAtt.isDisabled) info.buttonsAreDisabled = true;

            return info;
        }
    }
}