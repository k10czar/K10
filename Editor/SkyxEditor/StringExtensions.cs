using System;
using UnityEditor;

namespace Skyx.SkyxEditor
{
    public static class StringExtensions
    {
        public static string AppendInfo(this string baseString, object info, bool isHeader = true, EColor color = EColor.Support)
            => AppendInfo(baseString, info.ToString(), isHeader, color);

        public static string AppendInfo(this string baseString, string infoString, bool isHeader = true, EColor color = EColor.Support)
        {
            var targetColor = color.Get();
            var targetSize = isHeader ? SkyxStyles.DefaultFontSize : SkyxStyles.SmallFontSize;

            return $"{baseString} | <color=#{targetColor.RGB()}><size={targetSize}>{infoString}</size></color>";
        }

        public static string Pretty(this Enum value) => ObjectNames.NicifyVariableName(value.ToString());
    }
}