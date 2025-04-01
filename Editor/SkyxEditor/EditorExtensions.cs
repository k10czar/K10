using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class GUIStyleExtension
    {
        public static GUIStyle Darker(this GUIStyle style) => new(style) { normal = { textColor = style.normal.textColor.AddLight(-.3f) } };

        public static GUIStyle With(this GUIStyle style, Color color) => new(style) { normal = { textColor = color } };
        public static GUIStyle With(this GUIStyle style, EColor color) => new(style) { normal = { textColor = color.Get() } };

        public static GUIStyle Invisible(this GUIStyle style) => new(style) { normal = { textColor = Color.clear } };
    }

    public static class ColorExtensions
    {
        public static string RGB(this Color color) => ColorUtility.ToHtmlStringRGB(color);

        public static Color Expanded(this Color color, bool isExpanded) => isExpanded ? color.AddLight(-.15f) : color;
    }
}