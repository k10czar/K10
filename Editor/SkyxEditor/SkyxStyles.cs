using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class SkyxStyles
    {
        #region Constants

        public static readonly Color defaultSeparatorColor = new(0f, 0f, 0f, 0.3f);
        public static readonly Vector2 defaultSeparatorMargin = new(10f, 10f);
        public static readonly Vector2 headerSeparatorMargin = new(1f, 4f);
        public static readonly Vector2 noSeparatorMargin = new(-1f, -1f);

        public const float DefaultSeparatorSize = 1f;
        public const float BigSeparatorSize = 2f;
        public const float HugeSeparatorSize = 5f;

        public const float LineHeight = 18;
        public const float LineSpace = 4;
        public const float FullLineHeight = LineHeight + LineSpace;
        public const float ElementsMargin = 5;

        public const float SmallButtonSize = 22;
        public const float MiniButtonSize = 18;
        public const float HintIconWidth = 20;

        #endregion

        #region GUIStyles

        #region Support Data

        private const int SmallFontSize = 10;
        private const int DefaultFontSize = 13;
        private const int BigFontSize = 16;
        private const int HugeFontSize = 19;

        private static readonly RectOffset noPadding = new (3, 3, 0, 0);
        private static readonly RectOffset defaultPadding = new (6, 6, 2, 2);
        private static readonly RectOffset bigPadding = new (8, 8, 5, 5);
        private static readonly RectOffset hugePadding = new (8, 8, 8, 8);

        #endregion

        public static GUIStyle DefaultLabel => Style("defaultLabel", EditorStyles.label);
        public static GUIStyle CenterLabel => Style("centerLabel", DefaultLabel, TextAnchor.MiddleCenter);
        public static GUIStyle SmallLabel => Style("smallLabel", EditorStyles.label, SmallFontSize);
        public static GUIStyle BoldStyle => Style("boldLabel", EditorStyles.label, FontStyle.Bold);
        public static GUIStyle CenterBoldStyle => Style("centerBoldLabel", BoldStyle, TextAnchor.MiddleCenter);
        public static GUIStyle Header => Style("header", CenterBoldStyle, BigFontSize, padding: bigPadding);
        public static GUIStyle HugeHeader => Style("hugeHeader", Header, HugeFontSize, padding: hugePadding);
        public static GUIStyle PlainBGLabel => Style("plainBGLabel", CenterBoldStyle, padding: bigPadding, background: EditorGUIUtility.whiteTexture);
        public static GUIStyle PlainBGHeader => Style("plainBGHeader", Header, background: EditorGUIUtility.whiteTexture);
        public static GUIStyle LeftPlainBGHeader => Style("leftPlainBGHeader", PlainBGHeader, TextAnchor.MiddleLeft);
        public static GUIStyle InlaidHintLabel => Style("InlaidHint", CenterLabel, TextAnchor.MiddleRight);

        public static GUIStyle FoldStyle => Style("fold", EditorStyles.foldout, FontStyle.Bold);
        public static GUIStyle BigFoldStyle => Style("bigFold", FoldStyle, BigFontSize);

        public static GUIStyle PopupStyle => Style("popup", EditorStyles.popup);
        public static GUIStyle BoldPopupStyle => Style("boldPopup", PopupStyle, FontStyle.Bold);
        public static GUIStyle HeaderPopupStyle => Style("HeaderPopup", BoldPopupStyle, TextAnchor.MiddleCenter, BigFontSize);

        public static GUIStyle ButtonStyle => Style("button", GUI.skin.button, DefaultFontSize);
        public static GUIStyle BoldButtonStyle => Style("boldButton", ButtonStyle, FontStyle.Bold);

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

        public const float HeaderHeight = LineHeight;
        public const float BoxTotalExtraHeight = HeaderHeight;
        public static Color HeaderColor => Colors.AlmostBlack.WithAlpha(.4f);

        // public static GUIStyle BoxStyle(bool isRounded) => new(isRounded ? "HelpBox" : "Tooltip");
        // public static GUIStyle BoxStyle(bool isRounded) => isRounded ? roundedBox : squaredBox;
        public static GUIStyle BoxStyle(bool isRounded) => isRounded ? EditorStyles.helpBox : squaredBox;

        public static readonly GUIStyle roundedBox = new("HelpBox");
        public static readonly GUIStyle squaredBox = new("Tooltip");

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
        public static readonly GUIContent redLightIcon = EditorGUIUtility.TrIconContent("redLight");

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