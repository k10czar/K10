using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Skyx.RuntimeEditor
{
    public enum EScopePreset
    {
        FoldoutNameOnly,
        FoldoutPropertySummary,
        FoldoutSummaryOnly,
        FoldoutNameSummary,
        Inline,
        InlinePropertySummary,
        InlineFullContent,
        HeaderNameOnly,
        HeaderPropertySummary,
        HeaderNameSummary,
        InlineHeaderNameSummary,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    public class ScopedAttribute : PropertyAttribute
    {
        public readonly EScopeType scopeType = EScopeType.Foldout;
        public readonly EElementSize elementSize = EElementSize.SingleLine;
        public readonly EEditorInfoSource nameSource = EEditorInfoSource.EditorContent;
        public readonly EEditorInfoSource appendSource = EEditorInfoSource.EditorContent;
        public readonly EEditorInfoSource descriptionSource = EEditorInfoSource.EditorContent;
        public readonly EEditorInfoSource colorSource = EEditorInfoSource.EditorContent;
        public readonly bool indent;

        public readonly string name;
        public readonly string append;
        public readonly string description;
        public readonly EColor color;

        public bool isDisabled;
        public List<SkopeButton> buttons;

        public ScopedAttribute() {}

        public ScopedAttribute(EScopePreset preset)
        {
            var existing = presets[preset];

            scopeType = existing.scopeType;
            elementSize = existing.elementSize;
            nameSource = existing.nameSource;
            appendSource = existing.appendSource;
            descriptionSource = existing.descriptionSource;
            colorSource = existing.colorSource;
            indent = existing.indent;
        }

        public ScopedAttribute(string name, EScopePreset preset)
        {
            var existing = presets[preset];

            this.name = existing.name;
            nameSource = EEditorInfoSource.Provided;

            scopeType = existing.scopeType;
            elementSize = existing.elementSize;
            appendSource = existing.appendSource;
            descriptionSource = existing.descriptionSource;
            colorSource = existing.colorSource;
            indent = existing.indent;
        }

        private ScopedAttribute(EScopeType scopeType, EEditorInfoSource name, EEditorInfoSource append, EEditorInfoSource description, EEditorInfoSource color, bool indent = false)
        {
            this.scopeType = scopeType;
            elementSize = scopeType.PreferredSize();

            nameSource = name;
            appendSource = append;
            descriptionSource = description;
            colorSource = color;
            this.indent = indent;
        }

        #region Presets

        private static readonly Dictionary<EScopePreset, ScopedAttribute> presets = new()
        {
            { EScopePreset.FoldoutNameOnly, new ScopedAttribute(EScopeType.Foldout, EEditorInfoSource.EditorContent, EEditorInfoSource.Nothing, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },
            { EScopePreset.FoldoutPropertySummary, new ScopedAttribute(EScopeType.Foldout, EEditorInfoSource.Property, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },
            { EScopePreset.FoldoutSummaryOnly, new ScopedAttribute(EScopeType.Foldout, EEditorInfoSource.EditorAltContent, EEditorInfoSource.Nothing, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },
            { EScopePreset.FoldoutNameSummary, new ScopedAttribute(EScopeType.Foldout, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },

            { EScopePreset.HeaderNameOnly, new ScopedAttribute(EScopeType.Header, EEditorInfoSource.EditorContent, EEditorInfoSource.Nothing, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },
            { EScopePreset.HeaderPropertySummary, new ScopedAttribute(EScopeType.Header, EEditorInfoSource.Property, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },
            { EScopePreset.HeaderNameSummary, new ScopedAttribute(EScopeType.Header, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },

            { EScopePreset.Inline, new ScopedAttribute(EScopeType.Inline, EEditorInfoSource.EditorContent, EEditorInfoSource.Nothing, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },
            { EScopePreset.InlinePropertySummary, new ScopedAttribute(EScopeType.Inline, EEditorInfoSource.Property, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },
            { EScopePreset.InlineFullContent, new ScopedAttribute(EScopeType.Inline, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },

            { EScopePreset.InlineHeaderNameSummary, new ScopedAttribute(EScopeType.InlineHeader, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, EEditorInfoSource.EditorContent, true) },
        };

        #endregion
    }

    public class SkopeButton
    {
        public readonly string label;
        public EColor color;
        public Action<SerializedProperty> onClick;

        public SkopeButton(string label, EColor color, Action<SerializedProperty> onClick)
        {
            this.label = label;
            this.color = color;
            this.onClick = onClick;
        }

        public void Deconstruct(out string dLabel, out EColor dColor, out Action<SerializedProperty> dOnClick)
        {
            dLabel = label;
            dColor = color;
            dOnClick = onClick;
        }

        public override bool Equals(object obj) => obj is SkopeButton other && Equals(other);
        protected bool Equals(SkopeButton other) => label == other.label;
        public override int GetHashCode() => HashCode.Combine(label);
    }
}