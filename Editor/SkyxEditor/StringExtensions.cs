using System;

namespace Skyx.SkyxEditor
{
    public static class StringExtensions
    {
        public static string AppendInfo(this string baseString, string infoString, bool isHeader = true, EConsoleColor color = EConsoleColor.Support)
        {
            var targetColor = Colors.Console.Get(color);
            var targetSize = isHeader ? SkyxStyles.DefaultFontSize : SkyxStyles.SmallFontSize;

            return $"{baseString} | <color=#{targetColor.RGB()}><size={targetSize}>{infoString}</size></color>";
        }
    }
}