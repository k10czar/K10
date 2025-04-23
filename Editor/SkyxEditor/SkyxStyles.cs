using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public enum EElementSize
    {
        Primary,
        Secondary,
        SingleLine,
    }

    public static class SkyxStyles
    {
        #region Constants

        public static readonly Color defaultSeparatorColor = new(0f, 0f, 0f, 0.3f);
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

        #region Support Data

        public const int SmallFontSize = 10;
        public const int DefaultFontSize = 13;
        public const int BigFontSize = 16;
        public const int HugeFontSize = 19;

        private static readonly RectOffset noPadding = new (3, 3, 0, 0);
        private static readonly RectOffset defaultPadding = new (6, 6, 2, 2);
        private static readonly RectOffset bigPadding = new (8, 8, 5, 5);
        private static readonly RectOffset hugePadding = new (8, 8, 8, 8);

        #endregion

        public static GUIStyle DefaultLabel => Style("defaultLabel", EditorStyles.label);
        public static GUIStyle CenterLabel => Style("centerLabel", DefaultLabel, TextAnchor.MiddleCenter);
        public static GUIStyle RightLabel => Style("rightLabel", DefaultLabel, TextAnchor.MiddleRight);
        public static GUIStyle SmallLabel => Style("smallLabel", EditorStyles.label, SmallFontSize);
        public static GUIStyle BoldStyle => Style("boldLabel", EditorStyles.label, FontStyle.Bold);
        public static GUIStyle CenterBoldStyle => Style("centerBoldLabel", BoldStyle, TextAnchor.MiddleCenter);
        public static GUIStyle Header => Style("header", CenterBoldStyle, BigFontSize, padding: bigPadding);
        public static GUIStyle HugeHeader => Style("hugeHeader", Header, HugeFontSize, padding: hugePadding);
        public static GUIStyle PlainBGLabel => Style("plainBGLabel", CenterBoldStyle, padding: bigPadding, background: EditorGUIUtility.whiteTexture);
        public static GUIStyle PlainBGHeader => Style("plainBGHeader", Header, background: EditorGUIUtility.whiteTexture);
        public static GUIStyle InlaidHintLabel => Style("InlaidHint", DefaultLabel, TextAnchor.MiddleRight);

        public static GUIStyle FoldStyle => Style("fold", EditorStyles.foldout, FontStyle.Bold);
        public static GUIStyle BigFoldStyle => Style("bigFold", FoldStyle, BigFontSize);

        public static GUIStyle PopupStyle => Style("popup", EditorStyles.popup);
        public static GUIStyle BoldPopupStyle => Style("boldPopup", PopupStyle, FontStyle.Bold);
        public static GUIStyle HeaderPopupStyle => Style("HeaderPopup", BoldPopupStyle, TextAnchor.MiddleCenter, BigFontSize);

        public static GUIStyle ButtonStyle => Style("button", GUI.skin.button, DefaultFontSize, padding: defaultPadding);
        public static GUIStyle HeaderButtonStyle => Style("HeaderButton", GUI.skin.button, BigFontSize, padding: bigPadding);
        public static GUIStyle MiniButtonStyle => Style("button", GUI.skin.button, padding: noPadding);
        public static GUIStyle BoldButtonStyle => Style("boldButton", ButtonStyle, FontStyle.Bold);

        public static GUIStyle DropDownButton = new("DropDownToggleButton");

        public static GUIStyle GetButton(this EElementSize size) => size switch
        {
            EElementSize.Primary => HeaderButtonStyle,
            EElementSize.Secondary => HeaderButtonStyle,
            EElementSize.SingleLine => ButtonStyle,
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };

        public static GUIStyle GetPlainBG(this EElementSize size) => size switch
        {
            EElementSize.Primary => PlainBGHeader,
            EElementSize.Secondary => PlainBGHeader,
            EElementSize.SingleLine => PlainBGLabel,
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };

        public static GUIStyle GetPlainBG(this EElementSize size, EColor color) => GetPlainBG(size).With(color.GetPlainBGLabelColor());

        public static Color GetPlainBGLabelColor(this EColor color) => color switch
        {
            EColor.Primary => Colors.LightGray,
            EColor.Secondary => Colors.LightGray,
            EColor.Info => Colors.AlmostBlack,
            EColor.Success => Colors.AlmostBlack,
            EColor.Warning => Colors.LightGray,
            EColor.Danger => Colors.LightGray,
            EColor.Support => Colors.AlmostBlack,
            EColor.Special => Colors.LightGray,
            EColor.Disabled => Colors.LightGray,
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

        private static GUIStyle Style(string name, GUIStyle baseStyle, int fontSize = 0, bool hasAlignment = false, TextAnchor alignment = TextAnchor.MiddleLeft, bool hasFontStyle = false, FontStyle fontStyle = FontStyle.Bold, RectOffset padding = null, Texture2D background = null, RectOffset margin = null)
        {
            if (loadedStyles.TryGetValue(name, out var style)) return style;

            var newStyle = new GUIStyle(baseStyle) { richText = true };
            newStyle.normal.textColor = Colors.Console.GrayOut;

            if (hasAlignment) newStyle.alignment = alignment;
            if (hasFontStyle) newStyle.fontStyle = fontStyle;
            if (padding != null) newStyle.padding = padding;
            if (fontSize != 0) newStyle.fontSize = fontSize;
            if (background != null) newStyle.normal.background = background;
            if (margin != null) newStyle.margin = margin;

            loadedStyles[name] = newStyle;

            return newStyle;
        }

        #endregion

        #endregion

        #region Box

        public const float ListControlButtonSize = MiniButtonSize;
        public const float BoxHeaderHeight = FullLineHeight;
        public const float BoxMargin = 5;

        private static readonly GUIStyle[] headerText =
        {
            PlainBGHeader, // Primary
            Style("SecondaryHeader", Header, fontSize: 15, background: EditorGUIUtility.whiteTexture), // Secondary
            Style("SingleLineHeader", CenterBoldStyle, background: EditorGUIUtility.whiteTexture), // SingleLine
        };

        private static readonly float[] headerHeights =
        {
            32, // Primary
            28, // Secondary
            24, // SingleLine
        };

        private static readonly Color[] headerColors =
        {
            Colors.Console.Dark.AddLight(.02f), // Primary
            Colors.Console.Dark.AddLight(-.08f), // Secondary
            Colors.Console.Secondary.AddLight(-.2f), // Info
            Colors.Avocado.AddLight(-.2f), // Success
            Colors.Console.Warning.AddLight(-.4f).AddSaturation(-.1f), // Warning
            Color.clear, // Danger
            Color.clear, // Support
            Color.clear, // Special
        };

        private static readonly Color[] boxColors =
        {
            Color.white,
            Colors.Console.Dark,
            Color.white,
            Color.white,
            Colors.Console.Warning.AddLight(.2f).WithAlpha(.2f), // Warning
            Color.white,
            Color.white,
            Color.white,
        };

        private static readonly string[] boxStyles =
        {
            "ScriptText", // Primary
            "HelpBox", // Secondary
            "SelectionRect", // Info
            "U2D.createRect", // Success
            "TE ElementBackground", // Warning
            "flow node 6", // Danger
            "TE BoxBackground", // Support
            "ProgressBarBar", // Special
        };

        public static GUIStyle HeaderText(EElementSize size = EElementSize.Primary) => headerText[(int)size];
        public static float HeaderHeight(EElementSize size = EElementSize.Primary) => headerHeights[(int)size];
        public static float ScopeTotalExtraHeight(EElementSize size = EElementSize.Primary) => headerHeights[(int)size] + (3 * ElementsMargin);
        public static float ClosedScopeHeight(EElementSize size = EElementSize.Primary) => headerHeights[(int)size] + ElementsMargin;

        public static GUIStyle BoxStyle(EColor color) => new(boxStyles[(int)color]);
        public static Color HeaderColor(EColor color) => headerColors[(int)color];
        public static Color BoxColor(EColor color) => boxColors[(int)color];

        public static Color DefaultHeaderColor => HeaderColor(EColor.Secondary);

        public static readonly GUIStyle borderBoxHeaderStyle = new()
        {
            margin = new RectOffset(3, 3, 2, 2),
            padding = new RectOffset(5, 5, 2, 5)
        };

        public static readonly GUIStyle borderBoxStyle = new()
        {
            margin = new RectOffset(3, 3, 2, 2),
            padding = new RectOffset(5, 5, 5, 5)
        };

        #endregion

        #region Icons & Textures

        public static GUIStyle IconButton => GUI.skin.FindStyle("IconButton");
        public static readonly GUIContent plusIcon = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add Item");
        public static readonly GUIContent minusIcon = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove Item");
        public static readonly GUIContent trashIcon = EditorGUIUtility.TrIconContent("TreeEditor.Trash", "Remove Item");
        public static readonly GUIContent refreshIcon = EditorGUIUtility.TrIconContent("Refresh", "Refresh");
        public static readonly GUIContent linkedIcon = EditorGUIUtility.TrIconContent("Linked");
        public static readonly GUIContent unLinkedIcon = EditorGUIUtility.TrIconContent("Unlinked");
        public static readonly GUIContent databaseIcon = EditorGUIUtility.TrIconContent("Package Manager");
        public static readonly GUIContent greenLightIcon = EditorGUIUtility.TrIconContent("greenLight");
        public static readonly GUIContent orangeLightIcon = EditorGUIUtility.TrIconContent("orangeLight");
        public static readonly GUIContent downArrowIcon = EditorGUIUtility.TrIconContent("CollabPull");
        public static readonly GUIContent upArrowIcon = EditorGUIUtility.TrIconContent("CollabPush");


        public static Texture2D TransparentCheckerTexture
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    return EditorGUIUtility.LoadRequired("Previews/Textures/textureCheckerDark.png") as Texture2D;
                }

                return EditorGUIUtility.LoadRequired("Previews/Textures/textureChecker.png") as Texture2D;
            }
        }

        #endregion
    }
}