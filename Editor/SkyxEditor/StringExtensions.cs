using System;
using System.Linq;
using System.Text.RegularExpressions;
using Skyx.RuntimeEditor;
using UnityEditor;

namespace Skyx.SkyxEditor
{
    public static class StringExtensions
    {
        public static string TryAppendInfo(this string baseString, object info, EColor color = EColor.Support, EElementSize size = EElementSize.Primary)
            => info == null ? baseString : AppendInfo(baseString, info, color, size);

        public static string AppendInfo(this string baseString, object info, EColor color = EColor.Support, EElementSize size = EElementSize.Primary)
            => AppendInfo(baseString, info.ToString(), color, size);

        public static string AppendInfo(this string baseString, Enum info, EColor color = EColor.Support, EElementSize size = EElementSize.Primary)
            => AppendInfo(baseString, info.Pretty(), color, size);

        public static string AppendInfo(this string baseString, string infoString, EColor color = EColor.Support, EElementSize size = EElementSize.Primary)
        {
            var targetColor = color.Get();

            var targetSize = size switch
            {
                EElementSize.Primary => SkyxStyles.DefaultFontSize,
                EElementSize.Secondary => SkyxStyles.DefaultFontSize,
                EElementSize.SingleLine => SkyxStyles.SmallFontSize,
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };

            return $"{baseString} | <color={targetColor.ToHexRGB()}><size={targetSize}>{infoString}</size></color>";
        }

        public static string Pretty(this Enum value) => ObjectNames.NicifyVariableName(value.ToString());
        public static string Pretty(this string value) => ObjectNames.NicifyVariableName(value);

        public static string Clean(this string source) => Regex.Replace(source, "[^a-zA-Z0-9]", "");

        public static int LineCount(this string source) => source.Count(entry => entry == '\n');
    }
}