using System;
using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    public static class RectLib
    {
        public static Rect GetDividedControlRect(int divisions)
        {
            var rect = EditorGUILayout.GetControlRect(false);
            rect.DivideRect(divisions);

            return rect;
        }

        private static float DivideSize(float totalSize, int elementsCount)
        {
            return (totalSize - (SkyxStyles.ElementsMargin * (elementsCount - 1))) / elementsCount;
        }

        #region Extensions

        public static void SlideRect(this ref Rect rect, float newWidth, float margin = SkyxStyles.ElementsMargin)
        {
            rect.x += rect.width + margin;
            rect.width = newWidth;
        }

        public static void SlideSame(this ref Rect rect, float margin = SkyxStyles.ElementsMargin)
            => SlideRect(ref rect, rect.width, margin);

        public static void RemainingRect(this ref Rect rect, float endX)
        {
            rect.x += rect.width + SkyxStyles.ElementsMargin;
            rect.width = endX - rect.x;
        }

        public static void NextLine(this ref Rect rect, float startX, float totalWidth)
        {
            rect.x = startX;
            rect.y += SkyxStyles.FullLineHeight;
            rect.width = totalWidth;
        }

        public static void NextLineWithSeparator(this ref Rect rect, float startX, float totalWidth, float separatorMargin = 3)
        {
            NextLine(ref rect, startX, totalWidth);
            SkyxGUI.Separator(ref rect, separatorMargin);
        }

        public static void NextDividedLine(this ref Rect rect, float startX, float totalWidth, int divideCount)
            => NextLine(ref rect, startX, DivideSize(totalWidth, divideCount));

        public static void NextSameLine(this ref Rect rect)
            => NextLine(ref rect, rect.x, rect.width);

        public static void NextCustomLine(this ref Rect rect, float startX, float totalWidth, float height)
        {
            rect.x = startX;
            rect.height = height;
            rect.y += height + SkyxStyles.ElementsMargin;
            rect.width = totalWidth;
        }

        public static void DivideRect(this ref Rect rect, int elementsCount)
            => DivideRect(ref rect, rect.width, elementsCount);

        public static void DivideRect(this ref Rect rect, float totalWidth, int elementsCount)
            => rect.width = DivideSize(totalWidth, elementsCount);

        public static void DivideVertically(this ref Rect rect, int elementsCount)
            => rect.height = (rect.height - (SkyxStyles.ElementsMargin * (elementsCount - 1))) / elementsCount;

        public static void SlideSameVertically(this ref Rect rect, float margin = SkyxStyles.ElementsMargin)
            => rect.y += rect.height + margin;

        public static void SlideVertically(this ref Rect rect, float height, float margin = SkyxStyles.ElementsMargin)
        {
            rect.y += rect.height + margin;
            rect.height = height;
        }

        public static void Slide(this ref Rect rect, ERectSlideDir dir)
        {
            if (dir is ERectSlideDir.Vertical) rect.SlideSameVertically();
            else if (dir is ERectSlideDir.Horizontal) rect.SlideSame();
        }

        public static Rect ExtractRect(this ref Rect rect, float width, bool fromEnd = false, float margin = SkyxStyles.ElementsMargin)
        {
            if (fromEnd) return ExtractEndRect(ref rect, width, margin);

            var remaining = rect.width - width - margin;

            rect.width = width;
            var newRect = new Rect(rect);

            SlideRect(ref rect, remaining, margin);

            return newRect;
        }

        public static Rect ExtractEndRect(this ref Rect rect, float width, float margin = SkyxStyles.ElementsMargin)
        {
            rect.width = rect.width - width - margin;

            var newRect = new Rect(rect);
            SlideRect(ref newRect, width, margin);

            return newRect;
        }

        public static Rect ExtractVertical(this ref Rect rect, float height, float margin = SkyxStyles.ElementsMargin)
        {
            var remaining = rect.height - height - margin;

            rect.height = height;
            var newRect = new Rect(rect);

            rect.SlideVertically(remaining, margin);

            return newRect;
        }

        public static void AdjustToLine(this ref Rect rect, bool applyMargin = true)
        {
            rect.height = SkyxStyles.LineHeight;
            if (applyMargin) rect.y += 2;
        }

        public static void AdjustToLineAndDivide(this ref Rect rect, int divisions, bool applyMargin = true)
        {
            rect.AdjustToLine(applyMargin);
            rect.DivideRect(divisions);
        }

        public static void ExtractLineDef(this ref Rect rect, out float startX, out float totalWidth)
        {
            startX = rect.x;
            totalWidth = rect.width;
        }

        public static void ApplyStartMargin(this ref Rect rect, float margin = SkyxStyles.ElementsMargin, bool horizontal = true)
        {
            if (horizontal)
            {
                rect.x += margin;
                rect.width -= margin;
            }
            else
            {
                rect.y += margin;
                rect.height -= margin;
            }
        }

        public static void ApplyMargin(this ref Rect rect, float margin, bool vertical, bool horizontal)
        {
            if (vertical)
            {
                rect.y += margin;
                rect.height -= 2 * margin;
            }

            if (horizontal)
            {
                rect.x += margin;
                rect.width -= 2 * margin;
            }
        }

        public static void ApplyBoxMargin(this ref Rect rect, float headerHeight)
        {
            rect.y += headerHeight + SkyxStyles.ElementsMargin;
            rect.height -= headerHeight + 2 * SkyxStyles.ElementsMargin;
            rect.x += SkyxStyles.BoxMargin;
            rect.width -= SkyxStyles.BoxMargin * 2;
        }

        public static bool TryUseClick(this ref Rect rect, bool isRightClick)
        {
            var current = Event.current;
            var target = isRightClick ? 1 : 0;

            if (current.type == EventType.MouseDown && current.button == target && rect.Contains(current.mousePosition))
            {
                current.Use();
                return true;
            }

            return false;
        }

        public static (Rect, bool) ExtractOverHeaderButton(EElementSize headerSize)
            => ExtractOverHeaderButton(EditorGUILayout.GetControlRect(false, 1), headerSize);

        public static (Rect, bool) ExtractOverHeaderButton(Rect rect, EElementSize headerSize)
        {
            rect = rect.ExtractMiniButton(true);
            rect.height = EditorGUIUtility.singleLineHeight;

            var (deltaX, deltaY) = headerSize switch
            {
                EElementSize.Mini => (1, 2),
                EElementSize.Primary => (-6, 10),
                EElementSize.Secondary => (-5, 6),
                EElementSize.SingleLine => (-5, 3),
                _ => throw new ArgumentOutOfRangeException(nameof(headerSize), headerSize, null)
            };

            rect.y += deltaY;
            rect.x += deltaX;

            return (rect, rect.TryUseClick(false));
        }

        public static void Trim(this ref Rect rect, float max, bool horizontal = true)
        {
            if (horizontal)
            {
                var excess = rect.width - max;
                if (excess > 0)
                {
                    rect.width = max;
                    rect.x += excess / 2;
                }
            }
            else
            {
                var excess = rect.height - max;
                if (excess > 0)
                {
                    rect.height = max;
                    rect.x += excess / 2;
                }
            }
        }

        #endregion

        #region Sugar Extractions

        public static Rect ExtractLabelRect(this ref Rect rect)
            => ExtractRect(ref rect, EditorGUIUtility.labelWidth - SkyxStyles.ElementsMargin, false);

        public static Rect ExtractMediumButton(this ref Rect rect, bool fromEnd = false)
            => ExtractRect(ref rect, SkyxStyles.MediumButtonSize, fromEnd);

        public static Rect ExtractSmallButton(this ref Rect rect, bool fromEnd = false)
            => ExtractRect(ref rect, SkyxStyles.SmallButtonSize, fromEnd);

        public static Rect ExtractMiniButton(this ref Rect rect, bool fromEnd = false)
            => ExtractRect(ref rect, SkyxStyles.MiniButtonSize, fromEnd);

        public static Rect ExtractHint(this ref Rect rect, bool fromEnd = false)
            => ExtractRect(ref rect, SkyxStyles.HintIconWidth, fromEnd);

        #endregion
    }
}