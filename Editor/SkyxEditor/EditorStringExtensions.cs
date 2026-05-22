using System;
using Skyx.RuntimeEditor;

namespace Rogue.REditor
{
    public static class StringExtensions
    {
        public static string TryAppendInfo(this string baseString, object info, EElementSize size = EElementSize.Primary, EColor color = EColor.Support)
            => info == null ? baseString : AppendInfo(baseString, info, size, color);

        public static string AppendInfo(this string baseString, object info, EElementSize size = EElementSize.Primary, EColor color = EColor.Support)
            => AppendInfo(baseString, info.ToString(), size, color);

        public static string AppendInfo(this string baseString, Enum info, EElementSize size = EElementSize.Primary, EColor color = EColor.Support)
            => AppendInfo(baseString, info.Pretty(), size, color);

        public static string AppendInfo(this string baseString, string infoString, EElementSize size = EElementSize.Primary, EColor color = EColor.Support)
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
    }
}