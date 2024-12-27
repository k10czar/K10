using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class GUIStyleExtension
    {
        public static GUIStyle DarkText(this GUIStyle style) => new(style) { normal = { textColor = Colors.Console.Dark } };
        public static GUIStyle DangerText(this GUIStyle style) => new(style) { normal = { textColor = Colors.Console.Danger } };
        public static GUIStyle WarningText(this GUIStyle style) => new(style) { normal = { textColor = Colors.Console.Warning } };

        public static GUIStyle PrimaryText(this GUIStyle style) => new(style) { normal = { textColor = Colors.Console.Primary } };
        public static GUIStyle SecondaryText(this GUIStyle style) => new(style) { normal = { textColor = Colors.Console.GrayOut } };
        public static GUIStyle InfoText(this GUIStyle style) => new(style) { normal = { textColor = Colors.Console.Info } };
        public static GUIStyle LightText(this GUIStyle style) => new(style) { normal = { textColor = Colors.Console.Light } };

        public static float DrawSize(this GUIStyle style, string text) => style.CalcSize(new GUIContent(text)).x;

        public static GUIStyle AlignRight(this GUIStyle style) => new(style) { alignment = TextAnchor.MiddleRight };

        public static GUIStyle Invisible(this GUIStyle style) => new(style) { normal = { textColor = Color.clear } };
    }

    public static class ColorExtensions
    {
        public static string RGB(this Color color) => ColorUtility.ToHtmlStringRGB(color);

        public static Color Expanded(this Color color, bool isExpanded) => isExpanded ? color.AddLight(-.1f) : color;
    }
}