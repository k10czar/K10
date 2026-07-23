using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Skyx.RuntimeEditor
{
    public enum EScopePreset
    {
        NoPreset = -1,
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
        private readonly EScopePreset preset = EScopePreset.NoPreset;

        public readonly EScopeType scopeType = EScopeType.Foldout;
        public readonly EElementSize elementSize = EElementSize.SingleLine;
        public readonly EEditorInfoSource nameSource = EEditorInfoSource.EditorContent;
        public EEditorInfoSource appendSource = EEditorInfoSource.EditorContent;
        public readonly EEditorInfoSource descriptionSource = EEditorInfoSource.EditorContent;
        public EEditorInfoSource colorSource = EEditorInfoSource.EditorContent;
        public readonly bool indent;

        public readonly string name;
        public readonly string description;
        public string append;
        public EColor color;

        public bool isDisabled;

        #if UNITY_EDITOR
        public List<SkopeButton> buttons;
        #endif

        public ScopedAttribute() {}

        public ScopedAttribute(EScopePreset preset)
        {
            var existing = presets[preset];

            this.preset = preset;
            scopeType = existing.scopeType;
            elementSize = existing.elementSize;
            nameSource = existing.nameSource;
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

        public void ForceInfo(string forcedAppend, EColor forcedColor)
        {
            colorSource = EEditorInfoSource.Provided;
            color = forcedColor;

            appendSource = EEditorInfoSource.Provided;
            append = forcedAppend;
        }

        public void ReleaseForcedInfo()
        {
            var existing = presets[preset];

            appendSource = existing.appendSource;
            colorSource = existing.colorSource;
            color = EColor.Infer;
            append = null;
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
}