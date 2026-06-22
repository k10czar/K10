using System;

namespace Skyx.RuntimeEditor
{
    public enum EElementSize
    {
        Infer = -1,
        Primary = 0,
        Secondary = 1,
        SingleLine = 2,
        Mini = 3,
    }

    public enum EButtonType
    {
        Default,
        DropDown,
        Plain,
        Toolbar,
        DropDownToolbar,
        Inlaid,
    }

    public enum EScopeType
    {
        Header,
        Foldout,
        InlineHeader,
        Inline,
    }

    public enum EEditorInfoSource
    {
        Nothing,
        Property,
        FieldValue,
        FieldType,
        EditorContent,
        EditorAltContent,
        Provided,
    }

    public enum ERectSlideDir
    {
        None,
        Horizontal,
        Vertical,
    }

    public static class StyleEnumsLib
    {
        public static bool HasHeader(this EScopeType scope) => scope <= EScopeType.InlineHeader;

        public static bool ShowNoChildProperties(this EScopeType scope) => scope is EScopeType.Header or EScopeType.InlineHeader;

        public static EElementSize PreferredSize(this EScopeType scopeType) => scopeType switch
        {
            EScopeType.Header => EElementSize.Primary,
            EScopeType.Foldout => EElementSize.SingleLine,
            EScopeType.InlineHeader => EElementSize.Secondary,
            EScopeType.Inline => EElementSize.SingleLine,
            _ => throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null)
        };

        public static EColor InferColor(this EScopeType scopeType) => scopeType switch
        {
            EScopeType.Header => EColor.Primary,
            EScopeType.Foldout => EColor.Secondary,
            EScopeType.InlineHeader => EColor.Primary,
            EScopeType.Inline => EColor.Clear,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}