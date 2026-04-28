using System;
using Skyx.RuntimeEditor;

namespace Rogue.REditor
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
    }
}