using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rogue.REditor
{
    public static class StringExtensions
    {
        public static string Clean(this string source) => Regex.Replace(source, "[^a-zA-Z0-9]", "");

        public static int LineCount(this string source) => string.IsNullOrEmpty(source) ? 0 : source.Count(entry => entry == '\n') + 1;

        #if UNITY_EDITOR
        public static string Pretty(this Enum value) => UnityEditor.ObjectNames.NicifyVariableName(value.ToString());
        public static string Pretty(this string value) => UnityEditor.ObjectNames.NicifyVariableName(value);

        public static string Highlight(this string baseString, EColor color) => $"<color={color.ToHexRGB()}><b>{baseString}</b></color>";
        public static string Highlight(this Enum info, EColor color) => $"<color={color.ToHexRGB()}><b>{info}</b></color>";
        public static string Highlight(this Enum info) => $"<b>{info}</b>";
        #else
        public static string Pretty(this Enum value) => value.ToString();
        public static string Pretty(this string value) => value;

        public static string Highlight(this string baseString, EColor color) => baseString;
        public static string Highlight(this Enum info, EColor color) => info.ToString();
        public static string Highlight(this Enum info) => info.ToString();
        #endif
    }
}