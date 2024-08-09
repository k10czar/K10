using UnityEngine;

namespace Skyx.CustomEditor
{
    public static class GUIStyleExtension
    {
        public static GUIStyle DarkText(this GUIStyle style) => new(style) { normal = { textColor = SkyxStyles.dark } };
        public static GUIStyle DangerText(this GUIStyle style) => new(style) { normal = { textColor = SkyxStyles.danger } };
        public static GUIStyle WarningText(this GUIStyle style) => new(style) { normal = { textColor = SkyxStyles.warning } };

        public static GUIStyle PrimaryText(this GUIStyle style) => new(style) { normal = { textColor = SkyxStyles.primary } };
        public static GUIStyle SecondaryText(this GUIStyle style) => new(style) { normal = { textColor = SkyxStyles.secondary } };
        public static GUIStyle PurpleText(this GUIStyle style) => new(style) { normal = { textColor = SkyxStyles.purple } };
        public static GUIStyle LightText(this GUIStyle style) => new(style) { normal = { textColor = SkyxStyles.light } };
        public static GUIStyle InfoText(this GUIStyle style) => new(style) { normal = { textColor = SkyxStyles.info } };

        public static float DrawSize(this GUIStyle style, string text) => style.CalcSize(new GUIContent(text)).x;

        public static GUIStyle AlignRight(this GUIStyle style) => new(style) { alignment = TextAnchor.MiddleRight };
    }

    public static class ColorExtensions
    {
        public static string RGB(this Color color) => ColorUtility.ToHtmlStringRGB(color);
    }
}