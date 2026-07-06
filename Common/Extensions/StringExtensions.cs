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
        #else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Pretty(this Enum value) => value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Pretty(this string value) => value;
        #endif
    }
}