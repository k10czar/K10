using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Skyx.CustomEditor
{
    public static class SkyxStyles
    {
        #region Constants

        public static readonly Color defaultSeparatorColor = new(0f, 0f, 0f, 0.3f);
        public static readonly Vector2 defaultSeparatorMargin = new(10f, 10f);
        public static readonly Vector2 smallSeparatorMargin = new(4f, 4f);
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

        #region Colors

        // ReSharper disable UnassignedField.Global MemberCanBePrivate.Global
        [HexColorSource("#0d6efd")]
        public static Color primary;
        public static Color darkerPrimary;
        public static Color desaturatedPrimary;
        public static Color transparentPrimary;
        public static Color lightPrimary;

        [HexColorSource("#6c757d")]
        public static Color secondary;
        public static Color darkerSecondary;
        public static Color desaturatedSecondary;
        public static Color transparentSecondary;
        public static Color lightSecondary;

        [HexColorSource("#198754")]
        public static Color success;
        public static Color darkerSuccess;
        public static Color desaturatedSuccess;
        public static Color transparentSuccess;
        public static Color lightSuccess;

        [HexColorSource("#dc3545")]
        public static Color danger;
        public static Color darkerDanger;
        public static Color desaturatedDanger;
        public static Color transparentDanger;
        public static Color lightDanger;

        [HexColorSource("#ffc107")]
        public static Color warning;
        public static Color darkerWarning;
        public static Color desaturatedWarning;
        public static Color transparentWarning;
        public static Color lightWarning;

        [HexColorSource("#0dcaf0")]
        public static Color info;
        public static Color darkerInfo;
        public static Color desaturatedInfo;
        public static Color transparentInfo;
        public static Color lightInfo;

        [HexColorSource("#f8f9fa")]
        public static Color light;
        public static Color darkerLight;
        public static Color desaturatedLight;
        public static Color transparentLight;
        public static Color lightLight;

        [HexColorSource("#2A2A2A")]
        public static Color dark;
        public static Color darkerDark;
        public static Color desaturatedDark;
        public static Color transparentDark;
        public static Color lightDark;

        [HexColorSource("#7f3e98")]
        public static Color purple;
        public static Color darkerPurple;
        public static Color desaturatedPurple;
        public static Color transparentPurple;
        public static Color lightPurple;

        [HexColorSource("#5A5A5A")]
        public static Color gray;
        public static Color darkerGray;
        public static Color desaturatedGray;
        public static Color transparentGray;
        public static Color lightGray;

        public static readonly Color[] ColorSequence;

        public static Color ColorFromSequence<T>(T value) where T : Enum => ColorFromSequence((int)(object)value);
        public static Color ColorFromSequence(int index) => ColorSequence[Mathf.Clamp(index, 0, ColorSequence.Length)];

        // ReSharper restore UnassignedField.Global MemberCanBePrivate.Global

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

        private static readonly Dictionary<string, GUIStyle> loadedStyles = new();

        private static GUIStyle Style(string name, GUIStyle baseStyle, TextAnchor alignment, int fontSize = 0, RectOffset padding = null)
            => Style(name, baseStyle, fontSize, alignment: alignment, hasAlignment: true, padding: padding);

        private static GUIStyle Style(string name, GUIStyle baseStyle, FontStyle fontStyle)
            => Style(name, baseStyle, fontStyle: fontStyle, hasFontStyle: true);

        private static GUIStyle Style(string name, GUIStyle baseStyle, int fontSize = 0, bool hasAlignment = false, TextAnchor alignment = TextAnchor.MiddleLeft, bool hasFontStyle = false, FontStyle fontStyle = FontStyle.Bold, RectOffset padding = null, Texture2D background = null, RectOffset margin = null)
        {
            if (loadedStyles.ContainsKey(name)) return loadedStyles[name];

            var newStyle = new GUIStyle(baseStyle) { richText = true };
            newStyle.normal.textColor = light;

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

        #region Setup

        private static void SetupColors()
        {
            var fieldsWithAttribute = typeof(SkyxStyles)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.GetCustomAttributes(typeof(HexColorSource)).Any());

            foreach (var fieldInfo in fieldsWithAttribute)
            {
                var hex = fieldInfo.GetCustomAttributes(typeof(HexColorSource)).First() as HexColorSource;
                ColorUtility.TryParseHtmlString(hex!.hexColor, out var color);

                SetColor(fieldInfo, color);
                SetDarkerColor(fieldInfo, color);
                SetTransparentColor(fieldInfo, color);
                SetDesaturatedColor(fieldInfo, color);
                SetLightColor(fieldInfo, color);
            }
        }

        private static void SetDarkerColor(FieldInfo fieldInfo, Color color)
        {
            color.r = Math.Max(0, color.r - 0.125f);
            color.g = Math.Max(0, color.g - 0.125f);
            color.b = Math.Max(0, color.b - 0.125f);

            var name = fieldInfo.Name;
            name = $"darker{name[..1].ToUpper()}{name[1..]}";

            var targetFieldInfo = typeof(SkyxStyles).GetField(name);
            targetFieldInfo.SetValue(null, color);
        }

        private static void SetDesaturatedColor(FieldInfo fieldInfo, Color color)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            color = Color.HSVToRGB(h, Math.Max(0, s - .22f), v);

            var name = fieldInfo.Name;
            name = $"desaturated{name[..1].ToUpper()}{name[1..]}";

            var targetFieldInfo = typeof(SkyxStyles).GetField(name);
            targetFieldInfo.SetValue(null, color);
        }

        private static void SetLightColor(FieldInfo fieldInfo, Color color)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            color = Color.HSVToRGB(h, Math.Max(1, s + .1f), Math.Max(1, v + .15f));

            var name = fieldInfo.Name;
            name = $"light{name[..1].ToUpper()}{name[1..]}";

            var targetFieldInfo = typeof(SkyxStyles).GetField(name);
            targetFieldInfo.SetValue(null, color);
        }

        private static void SetTransparentColor(FieldInfo fieldInfo, Color color)
        {
            color.a = .4f;

            var name = fieldInfo.Name;
            name = $"transparent{name[..1].ToUpper()}{name[1..]}";

            var targetFieldInfo = typeof(SkyxStyles).GetField(name);
            targetFieldInfo.SetValue(null, color);
        }

        private static void SetColor(FieldInfo fieldInfo, Color color) => fieldInfo.SetValue(null, color);

        static SkyxStyles()
        {
            SetupColors();

            ColorSequence = new[]
            {
                transparentInfo,
                transparentWarning,
                transparentPurple,
                transparentSuccess,
                transparentPrimary,
                transparentDanger,
                transparentSecondary,
                transparentDark,
                darkerLight,
            };
        }

        #endregion
    }
}