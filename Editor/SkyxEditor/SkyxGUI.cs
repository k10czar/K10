using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class SkyxGUI
    {
        #region Property Drawers

        public static void DrawTextField(Rect rect, SerializedProperty property, string inlaidHint)
        {
            property.stringValue = EditorGUI.TextField(rect, property.stringValue);
            DrawHintOverlay(rect, inlaidHint);

            if (string.IsNullOrEmpty(property.stringValue)) DrawHindInlaid(rect, inlaidHint);
        }

        public static void DrawValidatedTextField(Rect rect, SerializedProperty property, string inlaidHint, IEnumerable<string> validValues, bool allowEmpty = false)
        {
            var isNumber = float.TryParse(property.stringValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out _);

            var color = string.IsNullOrEmpty(property.stringValue)
                ? (allowEmpty ? Colors.Console.Success : Colors.Console.Warning)
                : (validValues.Contains(property.stringValue) || isNumber)
                    ? Colors.Console.Success
                    : Colors.Console.Danger;

            SkyxLayout.DrawWithBGColor(color, () => DrawTextField(rect, property, inlaidHint));
        }

        public static void DrawTextAreaField(Rect rect, SerializedProperty property, string hint)
        {
            property.stringValue = EditorGUI.TextArea(rect, property.stringValue);
            DrawHintOverlay(rect, hint);

            if (string.IsNullOrEmpty(property.stringValue)) DrawHindInlaid(rect, hint);
        }

        public static void DrawIntField(Rect rect, SerializedProperty property, string hint)
        {
            property.intValue = EditorGUI.IntField(rect, property.intValue);

            DrawHintOverlay(rect, hint);
            if (property.intValue == 0)  DrawHindInlaid(rect, hint);
        }

        public static void DrawFloatField(Rect rect, SerializedProperty property, string inlaidHint, bool alwaysVisible = true, string overlayHint = null)
        {
            property.floatValue = EditorGUI.FloatField(rect, property.floatValue);

            DrawHintOverlay(rect, overlayHint ?? inlaidHint);
            if (property.floatValue == 0 || alwaysVisible) DrawHindInlaid(rect, inlaidHint);
        }

        public static void DrawMaskSelector(Rect rect, SerializedProperty property, string[] possibleValues, string hint)
        {
            property.intValue = EditorGUI.MaskField(rect, property.intValue, possibleValues);
            DrawHintOverlay(rect, hint);
        }

        public static void DrawObjectField<T>(Rect rect, SerializedProperty property, string hint)
            => DrawObjectField(rect, property, typeof(T), hint);

        public static void DrawObjectField(Rect rect, SerializedProperty property, Type objType, string hint)
        {
            var backgroundColor = property.objectReferenceValue != null ? Colors.Console.Success : Colors.Console.Danger;
            SkyxLayout.SetBackgroundColor(backgroundColor);

            property.objectReferenceValue = EditorGUI.ObjectField(rect, property.objectReferenceValue, objType, false);

            SkyxLayout.RestoreBackgroundColor();

            DrawHintOverlay(rect, hint);
        }

        public static void DrawColorField(Rect rect, SerializedProperty property,string hint)
        {
            SkyxLayout.SetBackgroundColor(Colors.Console.Success);

            property.colorValue = EditorGUI.ColorField(rect, GUIContent.none, property.colorValue, true, true, true);

            SkyxLayout.RestoreBackgroundColor();

            DrawHintOverlay(rect, hint);
        }

        public static void DrawHintOverlay(Rect rect, string hint)
        {
            if (string.IsNullOrEmpty(hint)) return;

            EditorGUI.LabelField(rect, new GUIContent(" ", hint));
        }

        public static void DrawHindInlaid(Rect rect, string hint)
        {
            if (string.IsNullOrEmpty(hint)) return;

            rect.width -= 5;
            EditorGUI.LabelField(rect, hint, SkyxStyles.InlaidHintLabel.SecondaryText());
        }

        #endregion

        #region Toggles

        public static bool DrawSuccessToggle(Rect rect, string label, SerializedProperty property, string hint)
            => DrawToggle(rect, label, label, Colors.Console.Success, Colors.Console.GrayOut, property, hint);

        public static bool DrawEnableToggle(Rect rect, string onLabel, string offLabel, SerializedProperty property, string hint)
            => DrawToggle(rect, onLabel, offLabel, Colors.Console.Info, Colors.Console.GrayOut, property, hint);

        public static bool DrawWarningToggle(Rect rect, string onLabel, string offLabel, SerializedProperty property, string hint)
            => DrawToggle(rect, onLabel, offLabel, Colors.Console.Warning, Colors.Console.GrayOut, property, hint);

        public static bool DrawChoiceToggle(Rect rect, string onLabel, string offLabel, SerializedProperty property, string hint)
            => DrawToggle(rect, onLabel, offLabel, Colors.Console.Secondary, Colors.Console.Info, property, hint);

        public static bool MiniSuccessToggle(ref Rect rect, SerializedProperty toggleProp, string label, string hint, bool fromEnd = false)
            => DrawSuccessToggle(ExtractMiniButton(ref rect, fromEnd), label, toggleProp, hint);

        public static bool MiniEnableToggle(ref Rect rect, SerializedProperty toggleProp, string onLabel, string offLabel, string hint)
            => DrawEnableToggle(ExtractMiniButton(ref rect), onLabel, offLabel, toggleProp, hint);

        public static bool MiniWarningToggle(ref Rect rect, SerializedProperty toggleProp, string onLabel, string offLabel, string hint)
            => DrawWarningToggle(ExtractMiniButton(ref rect), onLabel, offLabel, toggleProp, hint);

        public static bool MiniChoiceToggle(ref Rect rect, SerializedProperty toggleProp, string onLabel, string offLabel, string hint)
            => DrawChoiceToggle(ExtractMiniButton(ref rect), onLabel, offLabel, toggleProp, hint);

        public static bool MiniToggle(ref Rect rect, SerializedProperty toggleProp, string onLabel, string offLabel, string hint, Color onColor, Color offColor, bool useExpandField = false)
            => DrawToggle(ExtractMiniButton(ref rect), onLabel, offLabel, onColor, offColor, toggleProp, hint, useExpandField);

        private static bool DrawToggle(Rect rect, string onLabel, string offLabel, Color onColor, Color offColor, SerializedProperty toggleProp, string hint, bool useExpandField = false)
        {
            var isActive = useExpandField ? toggleProp.isExpanded : toggleProp.boolValue;

            var label = isActive ? onLabel : offLabel;
            var color = isActive ? onColor : offColor;

            var clicked = RectButton(rect, label, color, SkyxStyles.ButtonStyle);

            DrawHintOverlay(rect, hint);

            if (clicked)
            {
                if (useExpandField) toggleProp.isExpanded = !isActive;
                else toggleProp.boolValue = !isActive;
            }

            return clicked;
        }

        public static bool ExpandButton(ref Rect rect, SerializedProperty isExpandedProp)
            => MiniToggle(ref rect, isExpandedProp, "⇓", ">", "Expand", Colors.Console.GrayOut, Colors.Console.DarkerGrayOut, true);

        #endregion

        #region Buttons

        public static bool RectButton(Rect rect, string label, Color backgroundColor, GUIStyle style = null, string hint = null)
        {
            style ??= SkyxStyles.ButtonStyle;

            SkyxLayout.SetBackgroundColor(backgroundColor);
            var result = GUI.Button(rect, label, style);

            SkyxLayout.RestoreBackgroundColor();

            if (!string.IsNullOrEmpty(hint)) DrawHintOverlay(rect, hint);

            return result;
        }

        public static bool MiniSuccessButton(ref Rect rect, string label, string hint, bool fromEnd = false)
            => RectButton(ExtractMiniButton(ref rect, fromEnd), label, Colors.Console.Success, SkyxStyles.ButtonStyle, hint);

        public static bool MiniEnableButton(ref Rect rect, string label, string hint, bool fromEnd = false)
            => RectButton(ExtractMiniButton(ref rect, fromEnd), label, Colors.Console.Secondary, SkyxStyles.ButtonStyle, hint);

        public static bool MiniWarningButton(ref Rect rect, string label, string hint, bool fromEnd = false)
            => RectButton(ExtractMiniButton(ref rect, fromEnd), label, Colors.Console.Warning, SkyxStyles.ButtonStyle, hint);

        public static bool MiniDangerButton(ref Rect rect, string label, string hint, bool fromEnd = false)
            => RectButton(ExtractMiniButton(ref rect, fromEnd), label, Colors.Console.Danger, SkyxStyles.ButtonStyle, hint);

        public static bool MiniButton(ref Rect rect, string label, EConsoleColor color, string hint = null, bool fromEnd = false)
            => RectButton(ExtractMiniButton(ref rect, fromEnd), label, Colors.Console.Get(color), SkyxStyles.ButtonStyle, hint);

        public static bool PlainBGButton(Rect rect, string label, Color color)
            => RectButton(rect, label, color, SkyxStyles.PlainBGHeader);

        #endregion

        public static void HintIcon(ref Rect rect, string icon, string hint)
        {
            var hintRect = ExtractRect(ref rect, SkyxStyles.HintIconWidth);

            EditorGUI.LabelField(hintRect, $"<b>{icon}</b>", SkyxStyles.CenterBoldStyle);

            if (!string.IsNullOrEmpty(hint)) DrawHintOverlay(hintRect, hint);
        }

        public static void Separator(ref Rect rect, float margin = 4, float size = 1)
        {
            var separator = new Rect(rect.x, rect.y + rect.height + margin, rect.width, size);
            EditorGUI.DrawRect(separator, SkyxStyles.defaultSeparatorColor);

            rect.y += rect.height + 2 * margin + size;
        }

        public static void ShadowLabel(Rect rect, string label, GUIStyle style)
        {
            EditorGUI.LabelField(rect, label, style.DarkText());
            rect.x -= 1; rect.y -= 1;
            EditorGUI.LabelField(rect, label, style);
        }

        #region Rect Control

        public static void SlideRect(ref Rect rect, float newWidth, float margin = SkyxStyles.ElementsMargin)
        {
            rect.x += rect.width + margin;
            rect.width = newWidth;
        }

        public static void SlideSameRect(ref Rect rect, float margin = SkyxStyles.ElementsMargin)
            => SlideRect(ref rect, rect.width, margin);

        public static void RemainingRect(ref Rect rect, float endX)
        {
            rect.x += rect.width + SkyxStyles.ElementsMargin;
            rect.width = endX - rect.x;
        }

        public static void NextLine(ref Rect rect, float startX, float totalWidth, float extraMargin = 0)
        {
            rect.x = startX;
            rect.y += SkyxStyles.FullLineHeight + extraMargin;
            rect.width = totalWidth;
        }

        public static void NextDividedLine(ref Rect rect, float startX, float totalWidth, int divideCount)
            => NextLine(ref rect, startX, DivideRect(totalWidth, divideCount));

        public static void NextSameLine(ref Rect rect) => NextLine(ref rect, rect.x, rect.width);

        public static float DivideRect(float totalWidth, int elementsCount)
        {
            return (totalWidth - (SkyxStyles.ElementsMargin * (elementsCount - 1))) / elementsCount;
        }

        public static void DivideRect(ref Rect rect, int elementsCount)
            => DivideRect(ref rect, rect.width, elementsCount);

        public static void DivideRect(ref Rect rect, float totalWidth, int elementsCount)
        {
            rect.width = DivideRect(totalWidth, elementsCount);
        }

        public static void DivideRectVertically(ref Rect rect, int elementsCount)
        {
            rect.height = (rect.height - (SkyxStyles.ElementsMargin * (elementsCount - 1))) / elementsCount;
        }

        public static void SlideSameVertically(ref Rect rect) => rect.y += rect.height + SkyxStyles.ElementsMargin;

        public static Rect ExtractRect(ref Rect rect, float width, bool fromEnd = false)
        {
            if (fromEnd) return ExtractEndRect(ref rect, width);

            var remaining = rect.width - width - SkyxStyles.ElementsMargin;

            rect.width = width;
            var newRect = new Rect(rect);

            SlideRect(ref rect, remaining);

            return newRect;
        }

        public static Rect ExtractEndRect(ref Rect rect, float width)
        {
            rect.width = rect.width - width - SkyxStyles.ElementsMargin;

            var newRect = new Rect(rect);
            SlideRect(ref newRect, width);

            return newRect;
        }

        public static Rect ExtractSmallButton(ref Rect rect, bool fromEnd = false) => ExtractRect(ref rect, SkyxStyles.SmallButtonSize, fromEnd);
        public static Rect ExtractMiniButton(ref Rect rect, bool fromEnd = false) => ExtractRect(ref rect, SkyxStyles.MiniButtonSize, fromEnd);
        public static Rect ExtractHint(ref Rect rect, bool fromEnd = false) => ExtractRect(ref rect, SkyxStyles.HintIconWidth, fromEnd);

        public static void AdjustRectToListLine(ref Rect rect, bool applyMargin = true)
        {
            rect.height = SkyxStyles.LineHeight;
            if (applyMargin) rect.y += 2;
        }

        public static void ExtractLineDef(ref Rect rect, out float startX, out float totalWidth)
        {
            startX = rect.x;
            totalWidth = rect.width;
        }

        public static void ApplyHorizontalMargin(ref Rect rect, float margin)
        {
            rect.x += margin;
            rect.width -= 2 * margin;
        }

        public static void ApplyMargin(ref Rect rect, float margin)
        {
            rect.y += margin;
            rect.height -= 2 * margin;
            rect.x += margin;
            rect.width -= 2 * margin;
        }

        public static void ApplyBoxMargin(ref Rect rect, float headerHeight)
        {
            rect.y += headerHeight + SkyxStyles.ElementsMargin;
            rect.height -= headerHeight - 2 * SkyxStyles.ElementsMargin;
            rect.x += SkyxStyles.BoxMargin;
            rect.width -= SkyxStyles.BoxMargin * 2;
        }

        #endregion
    }
}