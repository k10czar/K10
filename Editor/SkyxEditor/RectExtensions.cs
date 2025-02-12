using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class RectExtensions
    {
        public static void SlideRect(this ref Rect rect, float newWidth, float margin = SkyxStyles.ElementsMargin)
            => SkyxGUI.SlideRect(ref rect, newWidth, margin);

        public static void SlideSameRect(this ref Rect rect, float margin = SkyxStyles.ElementsMargin)
            => SkyxGUI.SlideSameRect(ref rect, margin);

        public static void RemainingRect(this ref Rect rect, float endX)
            => SkyxGUI.RemainingRect(ref rect, endX);

        public static void NextLine(this ref Rect rect, float startX, float width, float extraMargin = 0)
            => SkyxGUI.NextLine(ref rect, startX, width, extraMargin);

        public static void NextLineWithSeparator(this ref Rect rect, float startX, float width, float separatorMargin = 2)
        {
            SkyxGUI.NextLine(ref rect, startX, width);
            SkyxGUI.Separator(ref rect, separatorMargin);
        }

        public static void NextDividedLine(this ref Rect rect, float startX, float totalWidth, int divideCount)
            => SkyxGUI.NextDividedLine(ref rect, startX, totalWidth, divideCount);

        public static void NextSameLine(this ref Rect rect)
            => SkyxGUI.NextSameLine(ref rect);

        public static void DivideRect(this ref Rect rect, int elementsCount)
            => SkyxGUI.DivideRect(ref rect, elementsCount);

        public static void DivideRect(this ref Rect rect, float totalWidth, int elementsCount)
            => SkyxGUI.DivideRect(ref rect, totalWidth, elementsCount);

        public static void DivideVertically(this ref Rect rect, int elementsCount)
            => SkyxGUI.DivideRectVertically(ref rect, elementsCount);

        public static void SlideSameVertically(this ref Rect rect)
            => SkyxGUI.SlideSameVertically(ref rect);

        public static Rect ExtractRect(this ref Rect rect, float width, bool fromEnd = false)
            => SkyxGUI.ExtractRect(ref rect, width, fromEnd);

        public static Rect ExtractEndRect(this ref Rect rect, float width)
            => SkyxGUI.ExtractEndRect(ref rect, width);

        public static Rect ExtractSmallButton(this ref Rect rect, bool fromEnd = false)
            => SkyxGUI.ExtractSmallButton(ref rect, fromEnd);

        public static Rect ExtractMiniButton(this ref Rect rect, bool fromEnd = false)
            => SkyxGUI.ExtractMiniButton(ref rect, fromEnd);

        public static Rect ExtractHint(this ref Rect rect, bool fromEnd = false)
            => SkyxGUI.ExtractHint(ref rect, fromEnd);

        public static void AdjustToLine(this ref Rect rect, bool applyMargin = true) => SkyxGUI.AdjustRectToLine(ref rect, applyMargin);

        public static void AdjustToLineAndDivide(this ref Rect rect, int divisions, bool applyMargin = true)
        {
            SkyxGUI.AdjustRectToLine(ref rect, applyMargin);
            SkyxGUI.DivideRect(ref rect, divisions);
        }

        public static void ExtractLineDef(this ref Rect rect, out float startX, out float totalWidth)
            => SkyxGUI.ExtractLineDef(ref rect, out startX, out totalWidth);

        public static void ApplyStartMargin(this ref Rect rect, float margin = SkyxStyles.ElementsMargin) => SkyxGUI.ApplyStartMargin(ref rect, margin);
        public static void ApplyMargin(this ref Rect rect, float margin, bool vertical = true, bool horizontal = true) => SkyxGUI.ApplyMargin(ref rect, margin, vertical, horizontal);

        public static void ApplyBoxMargin(this ref Rect rect, float headerHeight)
            => SkyxGUI.ApplyBoxMargin(ref rect, headerHeight);

    }
}