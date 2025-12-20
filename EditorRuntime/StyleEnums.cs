using System;

namespace Skyx.RuntimeEditor
{
    public enum EElementSize
    {
        Primary,
        Secondary,
        SingleLine,
        Mini,
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
        EditorSecondaryContent,
        Provided,
    }

    public static class StyleEnumsLib
    {
        public static bool HasHeader(this EScopeType scope) => scope <= EScopeType.InlineHeader;

        public static EElementSize PreferredSize(this EScopeType scopeType) => scopeType switch
        {
            EScopeType.Header => EElementSize.Primary,
            EScopeType.Foldout => EElementSize.SingleLine,
            EScopeType.InlineHeader => EElementSize.Primary,
            EScopeType.Inline => EElementSize.SingleLine,
            _ => throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null)
        };
    }
}