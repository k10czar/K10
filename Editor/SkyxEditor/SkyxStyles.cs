using System;
using System.Collections.Generic;
using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class SkyxStyles
    {
        #region Constants

        public static readonly Vector2 defaultSeparatorMargin = new(10f, 10f);
        public static readonly Vector2 smallSeparatorMargin = new(2f, 2f);
        public static readonly Vector2 noSeparatorMargin = new(-1f, -1f);

        public const float DefaultSeparatorSize = 1f;

        public const float LineHeight = 18;
        public const float LineSpace = 4;
        public const float FullLineHeight = LineHeight + LineSpace;
        public const float ElementsMargin = 5;
        public const float CompactSpace = 2;
        public const float CompactListElement = LineHeight + CompactSpace;

        public const float MediumButtonSize = 110;
        public const float SmallButtonSize = 60;
        public const float MiniButtonSize = 20;
        public const float HintIconWidth = 20;

        #endregion

        #region GUIStyles

        #region Support

        public const int SmallFontSize = 10;
        public const int MiniFontSize = 12;
        public const int DefaultFontSize = 13;
        public const int BigFontSize = 16;
        public const int HugeFontSize = 18;

        private static readonly RectOffset noPadding = new (0, 0, 0, 0);
        private static readonly RectOffset miniPadding = new (3, 3, 0, 0);
        private static readonly RectOffset defaultPadding = new (6, 6, 2, 2);
        private static readonly RectOffset bigPadding = new (8, 8, 5, 5);
        private static readonly RectOffset hugePadding = new (8, 8, 8, 8);

        public static Texture2D CreateTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();

            return texture;
        }

        #endregion

        public static GUIStyle DefaultLabel => Style("defaultLabel", EditorStyles.label);
        public static GUIStyle CenterLabel => Style("centerLabel", DefaultLabel, TextAnchor.MiddleCenter);
        public static GUIStyle RightLabel => Style("rightLabel", DefaultLabel, TextAnchor.MiddleRight);
        public static GUIStyle SmallLabel => Style("smallLabel", EditorStyles.label, SmallFontSize);
        public static GUIStyle BoldStyle => Style("boldLabel", EditorStyles.label, FontStyle.Bold);
        public static GUIStyle BigBoldStyle => Style("bigBoldLabel", BoldStyle, BigFontSize);
        public static GUIStyle HugeBoldStyle => Style("hugeBoldLabel", BoldStyle, HugeFontSize);
        public static GUIStyle CenterBoldStyle => Style("centerBoldLabel", BoldStyle, TextAnchor.MiddleCenter);
        public static GUIStyle Header => Style("header", CenterBoldStyle, BigFontSize, padding: bigPadding);
        public static GUIStyle HugeHeader => Style("hugeHeader", Header, HugeFontSize, padding: hugePadding);
        public static GUIStyle PlainBGLabel => Style("plainBGLabel", CenterBoldStyle, padding: bigPadding, background: EditorGUIUtility.whiteTexture, hoverBackground: CreateTexture(Colors.AlmostWhite));
        public static GUIStyle PlainBGHeader => Style("plainBGHeader", Header, background: EditorGUIUtility.whiteTexture);
        public static GUIStyle InlaidHintLabel => Style("InlaidHint", DefaultLabel, TextAnchor.MiddleRight);

        public static GUIStyle FoldStyle => Style("fold", EditorStyles.foldout, FontStyle.Bold);
        public static GUIStyle BigFoldStyle => Style("bigFold", FoldStyle, BigFontSize);

        public static GUIStyle PopupStyle => Style("popup", EditorStyles.popup);
        public static GUIStyle BoldPopupStyle => Style("boldPopup", PopupStyle, FontStyle.Bold);
        public static GUIStyle HeaderPopupStyle => Style("HeaderPopup", BoldPopupStyle, TextAnchor.MiddleCenter, BigFontSize);

        public static GUIStyle ButtonStyle => Style("button", GUI.skin.button, DefaultFontSize, padding: defaultPadding);
        public static GUIStyle HeaderButtonStyle => Style("HeaderButton", GUI.skin.button, BigFontSize, padding: bigPadding);
        public static GUIStyle MiniButtonStyle => Style("MiniButton", GUI.skin.button, padding: miniPadding);
        public static GUIStyle BoldButtonStyle => Style("BoldButton", ButtonStyle, FontStyle.Bold);
        public static GUIStyle TextAreaStyle => Style("TextArea", GUI.skin.textArea, margin: noPadding, padding: defaultPadding);

        public static GUIStyle WhiteBackgroundStyle => Style("WhiteBackgroundStyle", GUIStyle.none, background: EditorGUIUtility.whiteTexture);

        public static GUIStyle DropDownButton = new("DropDownToggleButton");

        public static GUIStyle GetPlainBG(this EElementSize size) => size switch
        {
            EElementSize.Primary => PlainBGHeader,
            EElementSize.Secondary => PlainBGHeader,
            EElementSize.SingleLine => PlainBGLabel,
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };

        public static GUIStyle GetPlainBG(this EElementSize size, EColor color) => GetPlainBG(size).With(color.GetButtonLabelColor());

        private static Color GetButtonLabelColor(this EColor color) => color switch
        {
            EColor.Primary => Colors.LightGray,
            EColor.Secondary => Colors.LightGray,
            EColor.Info => Colors.LightGray,
            EColor.Success => Colors.AlmostBlack,
            EColor.Warning => Colors.LightGray,
            EColor.Danger => Colors.LightGray,
            EColor.Support => Colors.AlmostBlack,
            EColor.Special => Colors.LightGray,
            EColor.Disabled => Colors.Silver,
            EColor.Clear => Colors.LightGray,
            EColor.Backdrop => Colors.LightGray,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };

        #region Styles Management

        private static readonly Dictionary<string, GUIStyle> loadedStyles = new();

        private static GUIStyle Style(string name, GUIStyle baseStyle, TextAnchor alignment, int fontSize = 0, RectOffset padding = null)
            => Style(name, baseStyle, fontSize, alignment: alignment, hasAlignment: true, padding: padding);

        private static GUIStyle Style(string name, GUIStyle baseStyle, FontStyle fontStyle)
            => Style(name, baseStyle, fontStyle: fontStyle, hasFontStyle: true);

        private static GUIStyle Style(string name, GUIStyle baseStyle, FontStyle fontStyle, TextAnchor alignment)
            => Style(name, baseStyle, fontStyle: fontStyle, hasFontStyle: true, alignment: alignment, hasAlignment: true);

        private static GUIStyle Style(string name, GUIStyle baseStyle, int fontSize = 0, bool hasAlignment = false, TextAnchor alignment = TextAnchor.MiddleLeft, bool hasFontStyle = false, FontStyle fontStyle = FontStyle.Bold, RectOffset padding = null, Texture2D background = null, RectOffset margin = null, Color? textColor = null, RectOffset border = null, Texture2D hoverBackground = null)
        {
            if (loadedStyles.TryGetValue(name, out var style)) return style;

            var newStyle = new GUIStyle(baseStyle)
            {
                richText = true,
                normal = { textColor = textColor ?? Colors.Console.GrayOut }
            };

            if (hasAlignment) newStyle.alignment = alignment;
            if (hasFontStyle) newStyle.fontStyle = fontStyle;
            if (padding != null) newStyle.padding = padding;
            if (border != null) newStyle.border = border;
            if (fontSize != 0) newStyle.fontSize = fontSize;
            if (background != null) newStyle.normal.background = background;
            if (hoverBackground != null) newStyle.hover.background = hoverBackground;

            if (margin != null) newStyle.margin = margin;

            loadedStyles[name] = newStyle;

            return newStyle;
        }

        #endregion

        #endregion

        #region Buttons

        private static readonly Dictionary<(EButtonType, EElementSize, EColor), GUIStyle> buttonStyles = new();

        public static GUIStyle GetButton(this EElementSize size) => size switch
        {
            EElementSize.Primary => HeaderButtonStyle,
            EElementSize.Secondary => HeaderButtonStyle,
            EElementSize.SingleLine => ButtonStyle,
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };

        public static GUIStyle GetButton(this EButtonType type, EElementSize size, EColor color)
        {
            if (buttonStyles.TryGetValue((type, size, color), out var style)) return style;

            var baseStyle = type switch
            {
                EButtonType.Default when size is EElementSize.Mini => MiniButtonStyle,
                EButtonType.Default => GUI.skin.button,
                EButtonType.DropDown => EditorStyles.popup,
                EButtonType.Plain => PlainBGLabel,
                EButtonType.Toolbar => new GUIStyle("toolbarbutton") { fixedHeight = 0 },
                EButtonType.DropDownToolbar => EditorStyles.toolbarDropDown,
                EButtonType.Inlaid => PlainBGLabel,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            var fontSize = size switch
            {
                EElementSize.Primary => HugeFontSize,
                EElementSize.Secondary => BigFontSize,
                EElementSize.SingleLine => DefaultFontSize,
                EElementSize.Mini => MiniFontSize,
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };

            var padding = size switch
            {
                EElementSize.Primary => bigPadding,
                EElementSize.Secondary => bigPadding,
                EElementSize.SingleLine => defaultPadding,
                EElementSize.Mini => miniPadding,
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };

            var textColor = color switch
            {
                EColor.Disabled => Colors.DimGray,
                _ => Colors.LightGray,
            };

            style = Style($"{type}.{size}.{color}", baseStyle, textColor: textColor, fontSize: fontSize, padding: padding);

            buttonStyles[(type, size, color)] = style;
            return style;
        }

        #endregion

        #region Scopes

        public const float ListControlButtonSize = MiniButtonSize;
        public const float BoxMargin = 5;

        private static readonly GUIStyle[] defaultHeaderText =
        {
            PlainBGHeader, // Primary
            Style("SecondaryHeader", Header, fontSize: 15, background: EditorGUIUtility.whiteTexture), // Secondary
            Style("SingleLineHeader", CenterBoldStyle, background: EditorGUIUtility.whiteTexture), // SingleLine
        };

        private static readonly GUIStyle[] disabledHeaderText =
        {
            Style("DisabledHeader", defaultHeaderText[0], textColor: Colors.Console.Disabled), // Primary
            Style("DisabledSecondaryHeader", defaultHeaderText[1], textColor: Colors.Console.Disabled), // Secondary
            Style("DisabledSingleLineHeader", defaultHeaderText[2], textColor: Colors.Console.Disabled), // SingleLine
        };

        private static readonly GUIStyle[][] headerText =
        {
            defaultHeaderText, // Primary
            defaultHeaderText, // Secondary
            defaultHeaderText, // Info
            defaultHeaderText, // Success
            defaultHeaderText, // Warning
            defaultHeaderText, // Danger
            defaultHeaderText, // Support
            defaultHeaderText, // Special
            disabledHeaderText, // Disabled
        };

        private static readonly float[] headerHeights =
        {
            32, // Primary
            28, // Secondary
            24, // SingleLine
            24, // SingleLine
        };

        private static readonly Color[] headerColors =
        {
            Colors.Console.Dark.AddLight(.02f), // Primary
            Colors.Console.Dark.AddLight(-.08f), // Secondary
            Colors.Console.Secondary.AddLight(-.2f), // Info
            Colors.Avocado.AddLight(-.2f), // Success
            Colors.Console.Warning.AddLight(-.4f).AddSaturation(-.1f), // Warning
            Colors.DarkRed, // Danger
            Color.clear, // Support
            Colors.Console.Special, // Special
            Colors.Console.Dark.AddLight(-.08f), // Disabled
            Color.clear, // Clear
            Colors.Transparent02, // Backdrop
        };

        private static readonly Color[] boxColors =
        {
            Color.white,
            Colors.Console.Dark,
            Color.white,
            Colors.Pistachio,
            Colors.Yellow, // Warning
            Colors.LightSalmon, // Danger
            Color.white, // Support
            Colors.Console.Special, // Special
            Colors.Console.Dark, // Disabled
            Color.clear, // Clear,
            Color.white, // Backdrop
        };

        private static readonly GUIStyle[] boxStyles =
        {
            new("ScriptText"), // Primary
            new("HelpBox"), // Secondary
            new("SelectionRect"), // Info
            new("HelpBox"), // Success
            new("HelpBox"), // Warning
            new("HelpBox"), // Danger
            new("TE BoxBackground"), // Support
            new("HelpBox"), // Special
            new("HelpBox"), // Disabled
            new(GUIStyle.none), // Clear
            new("Wizard Box"), // Backdrop
        };

        public static GUIStyle HeaderStyle(EElementSize size, EColor color) => headerText[(int)color][(int)size];
        public static GUIStyle BoxStyle(EColor color) => boxStyles[(int)color];
        public static Color HeaderColor(EColor color) => headerColors[(int)color];
        public static Color BoxColor(EColor color) => boxColors[(int)color];

        public static float GetHelpBoxHeight(int lineCount, bool addMargin) => FullLineHeight * lineCount + (addMargin ? ElementsMargin : 0);

        public static float HeaderHeight(EElementSize size) => headerHeights[(int)size];
        public static float ScopeTotalExtraHeight(EElementSize size) => headerHeights[(int)size] + (3 * ElementsMargin);
        public static float ClosedScopeHeight(EElementSize size) => headerHeights[(int)size] + ElementsMargin;

        public static readonly GUIStyle borderBoxHeaderStyle = new()
        {
            margin = new RectOffset(3, 3, 2, 2),
            padding = new RectOffset(5, 5, 2, 5)
        };

        #endregion
    }
}