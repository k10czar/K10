using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class SkyxGUI
    {
        #region Property Drawers

        public static void DrawTextField(Rect rect, SerializedProperty property, string inlaidHint, string overlayHint = null)
        {
            property.stringValue = EditorGUI.DelayedTextField(rect, property.stringValue);

            DrawHintOverlay(rect, overlayHint ?? inlaidHint);
            if (string.IsNullOrEmpty(property.stringValue)) DrawHindInlaid(rect, inlaidHint);
        }

        public static void DrawValidatedTextField(Rect rect, SerializedProperty property, string inlaidHint, string[] validValues, bool allowEmpty = false, string overlayHint = null)
        {
            var isNumber = float.TryParse(property.stringValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out _);

            var color = string.IsNullOrEmpty(property.stringValue)
                ? (allowEmpty ? Colors.Console.Success : Colors.Console.Warning)
                : (validValues.Contains(property.stringValue))
                    ? Colors.Console.SuccessBackground
                    : (isNumber ? Colors.Console.Success : Colors.Console.Danger);

            using var backgroundColor = BackgroundColorScope.Set(color);

            var currentIndex = Array.IndexOf(validValues, property.stringValue);

            GUI.Box(rect, GUIContent.none, SkyxStyles.DropDownButton);

            var innerRect = rect;
            var dropdownRect = innerRect.ExtractEndRect(10);

            var index = EditorGUI.Popup(dropdownRect, currentIndex, validValues, GUIStyle.none.Invisible());
            if (index != currentIndex)
            {
                property.stringValue = validValues[index];
                property.Apply();
            }

            innerRect.ApplyStartMargin();

            EditorGUI.BeginChangeCheck();
            property.stringValue = EditorGUI.DelayedTextField(innerRect, GUIContent.none, property.stringValue, SkyxStyles.DefaultLabel);
            if (EditorGUI.EndChangeCheck()) property.Apply();

            DrawHintOverlay(innerRect, overlayHint ?? inlaidHint);
            if (string.IsNullOrEmpty(property.stringValue)) DrawHindInlaid(innerRect, inlaidHint);
        }

        public static void DrawTextAreaField(Rect rect, SerializedProperty property, string hint)
        {
            property.stringValue = EditorGUI.TextArea(rect, property.stringValue);
            DrawHintOverlay(rect, hint);

            if (string.IsNullOrEmpty(property.stringValue)) DrawHindInlaid(rect, hint);
        }

        public static void DrawIntField(Rect rect, SerializedProperty property, string hint)
        {
            property.intValue = EditorGUI.DelayedIntField(rect, property.intValue);

            DrawHintOverlay(rect, hint);
            if (property.intValue == 0)  DrawHindInlaid(rect, hint);
        }

        public static void DrawFloatField(Rect rect, SerializedProperty property, string inlaidHint, bool alwaysVisible = true, string overlayHint = null)
        {
            property.floatValue = EditorGUI.DelayedFloatField(rect, property.floatValue);

            DrawHintOverlay(rect, overlayHint ?? inlaidHint);
            if (property.floatValue == 0 || alwaysVisible) DrawHindInlaid(rect, inlaidHint);
        }

        public static void DrawObjectField<T>(Rect rect, SerializedProperty property, string hint, bool allowSceneObjects = false)
            => DrawObjectField(rect, property, typeof(T), hint, allowSceneObjects);

        private static void DrawObjectField(Rect rect, SerializedProperty property, Type objType, string hint = null, bool allowSceneObjects = false)
        {
            var backgroundColor = property.objectReferenceValue != null ? Colors.Console.Success : Colors.Console.Danger;
            using var backgroundScope = BackgroundColorScope.Set(backgroundColor);

            property.objectReferenceValue = EditorGUI.ObjectField(rect, property.objectReferenceValue, objType, allowSceneObjects);

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
            EditorGUI.LabelField(rect, hint, SkyxStyles.InlaidHintLabel);
        }

        public static void Draw(Rect rect, SerializedProperty property, Type targetType, FieldDrawInfo drawInfo)
        {
            if (targetType == typeof(int))
            {
                if (targetType != drawInfo.requestedType)
                    EnumTreeGUI.DrawEnumMask(rect, property, drawInfo.requestedType, drawInfo.color);

                else DrawIntField(rect, property, drawInfo.hint);

                return;
            }

            Debug.Assert(targetType == drawInfo.requestedType || drawInfo.requestedType == null, $"Draw info type mismatch! {drawInfo.requestedType} != {targetType}");

            if (targetType.IsEnum)
            {
                var hasFlagsAttribute = targetType.IsDefined(typeof(FlagsAttribute), inherit: false);

                if (hasFlagsAttribute) EnumTreeGUI.DrawEnumMask(rect, property, targetType, drawInfo.color);
                else EnumTreeGUI.DrawEnum(rect, property, targetType, drawInfo.color, drawInfo.hint);
            }

            else if (targetType.IsClass)
                DrawObjectField(rect, property, targetType, drawInfo.hint, true);

            else if (targetType == typeof(float))
                DrawFloatField(rect, property, drawInfo.hint);

            else if (targetType == typeof(string))
                DrawTextField(rect, property, drawInfo.hint);

            else throw new Exception("Unknown type");
        }

        public static void Draw(Rect rect, SerializedProperty property, bool drawLabel = false)
        {
            var label = drawLabel ? null : GUIContent.none;

            EditorGUI.BeginChangeCheck();
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    EditorGUI.DelayedTextField(rect, property, label); break;

                case SerializedPropertyType.Integer:
                    EditorGUI.DelayedIntField(rect, property, label); break;

                case SerializedPropertyType.Float:
                    EditorGUI.DelayedFloatField(rect, property, label); break;

                default: EditorGUI.PropertyField(rect, property, label); break;
            }
            if (EditorGUI.EndChangeCheck()) property.Apply();
        }

        public static void Draw(SerializedProperty property, string label = null)
        {
            var labelGUI = new GUIContent(label ?? property.displayName);

            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    EditorGUILayout.DelayedTextField(property, labelGUI); break;

                case SerializedPropertyType.Integer:
                    EditorGUILayout.DelayedIntField(property, labelGUI); break;

                case SerializedPropertyType.Float:
                    EditorGUILayout.DelayedFloatField(property, labelGUI); break;

                default: EditorGUILayout.PropertyField(property, labelGUI); break;
            }
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
            => DrawToggle(rect, onLabel, offLabel, Colors.Console.Success, Colors.Console.Info, property, hint);

        public static bool MiniSuccessToggle(ref Rect rect, SerializedProperty toggleProp, string label, string hint, bool fromEnd = false)
            => DrawSuccessToggle(ExtractMiniButton(ref rect, fromEnd), label, toggleProp, hint);

        public static bool MiniEnableToggle(ref Rect rect, SerializedProperty toggleProp, string onLabel, string offLabel, string hint)
            => DrawEnableToggle(ExtractMiniButton(ref rect), onLabel, offLabel, toggleProp, hint);

        public static bool MiniWarningToggle(ref Rect rect, SerializedProperty toggleProp, string onLabel, string offLabel, string hint)
            => DrawWarningToggle(ExtractMiniButton(ref rect), onLabel, offLabel, toggleProp, hint);

        public static bool MiniChoiceToggle(ref Rect rect, SerializedProperty toggleProp, string onLabel, string offLabel, string hint)
            => DrawChoiceToggle(ExtractMiniButton(ref rect), onLabel, offLabel, toggleProp, hint);

        public static bool MiniToggle(ref Rect rect, SerializedProperty toggleProp, string onLabel, string offLabel, string hint, Color onColor, Color offColor, bool useExpandField = false, bool fromEnd = false)
            => DrawToggle(ExtractMiniButton(ref rect, fromEnd), onLabel, offLabel, onColor, offColor, toggleProp, hint, useExpandField);

        private static bool DrawToggle(Rect rect, string onLabel, string offLabel, Color onColor, Color offColor, SerializedProperty toggleProp, string hint, bool useExpandField = false)
        {
            var isActive = useExpandField ? toggleProp.isExpanded : toggleProp.boolValue;

            var label = isActive ? onLabel : offLabel;
            var color = isActive ? onColor : offColor;

            var clicked = Button(rect, label, color, SkyxStyles.ButtonStyle);

            DrawHintOverlay(rect, hint);

            if (clicked)
            {
                if (useExpandField) toggleProp.isExpanded = !isActive;
                else toggleProp.boolValue = !isActive;
            }

            return clicked;
        }

        public static void ExpandButton(ref Rect rect, SerializedProperty isExpandedProp)
        {
            var extracted = ExtractMiniButton(ref rect);

            var label = isExpandedProp.isExpanded ? "⇓" : ">";
            var backgroundColor = isExpandedProp.isExpanded ? Colors.Console.GrayOut : Colors.Console.DarkerGrayOut;

            using var backgroundScope = BackgroundColorScope.Set(backgroundColor);
            EditorGUI.LabelField(extracted, label, SkyxStyles.ButtonStyle);

            var clicked = extracted.TryUseClick(false);
            if (clicked) isExpandedProp.isExpanded = !isExpandedProp.isExpanded;
        }

        #endregion

        #region Buttons

        public static bool HeaderButton(Rect rect, string label, EColor color, EElementSize size)
            => Button(rect, label, color.Get(), size.GetButton());

        public static bool Button(Rect rect, string label, EColor color, string hint = null)
            => Button(rect, label, color.Get(), SkyxStyles.ButtonStyle, hint);

        public static bool Button(Rect rect, string label, Color backgroundColor, GUIStyle style = null, string hint = null)
        {
            style ??= SkyxStyles.ButtonStyle;

            using var backgroundScope = BackgroundColorScope.Set(backgroundColor);
            var result = GUI.Button(rect, label, style);

            if (!string.IsNullOrEmpty(hint)) DrawHintOverlay(rect, hint);

            return result;
        }

        public static bool MiniSuccessButton(ref Rect rect, string label, string hint, bool fromEnd = false)
            => Button(ExtractMiniButton(ref rect, fromEnd), label, Colors.Console.Success, SkyxStyles.MiniButtonStyle, hint);

        public static bool MiniEnableButton(ref Rect rect, string label, string hint, bool fromEnd = false)
            => Button(ExtractMiniButton(ref rect, fromEnd), label, Colors.Console.Secondary, SkyxStyles.MiniButtonStyle, hint);

        public static bool MiniWarningButton(ref Rect rect, string label, string hint, bool fromEnd = false)
            => Button(ExtractMiniButton(ref rect, fromEnd), label, Colors.Console.Warning, SkyxStyles.MiniButtonStyle, hint);

        public static bool MiniDangerButton(ref Rect rect, string label, string hint, bool fromEnd = false)
            => Button(ExtractMiniButton(ref rect, fromEnd), label, Colors.Console.Danger, SkyxStyles.MiniButtonStyle, hint);

        public static bool MiniButton(ref Rect rect, string label, EColor color, string hint = null, bool fromEnd = false)
            => Button(ExtractMiniButton(ref rect, fromEnd), label, color.Get(), SkyxStyles.MiniButtonStyle, hint);

        #endregion

        public static string DrawTextFieldWithSuggestions(Rect rect, string currentValue, string[] suggestions)
        {
            var currentIndex = Array.IndexOf(suggestions, currentValue);

            var dropdownRect = rect.ExtractEndRect(20);

            // TODO: find a better style :thinkingface:
            var index = EditorGUI.Popup(dropdownRect, currentIndex, suggestions, SkyxStyles.ButtonStyle);
            if (index != currentIndex) return suggestions[index];

            return EditorGUI.TextField(rect, GUIContent.none, currentValue);
        }

        public static void HintIcon(ref Rect rect, string icon, string hint, bool fromEnd = false)
        {
            var hintRect = ExtractRect(ref rect, SkyxStyles.HintIconWidth, fromEnd);

            EditorGUI.LabelField(hintRect, $"<b>{icon}</b>", SkyxStyles.CenterBoldStyle);

            if (!string.IsNullOrEmpty(hint)) DrawHintOverlay(hintRect, hint);
        }

        public static void Separator(ref Rect rect, float margin = SkyxStyles.ElementsMargin)
        {
            var separator = new Rect(rect.x, rect.y, rect.width, 1);
            EditorGUI.DrawRect(separator, SkyxStyles.defaultSeparatorColor);

            rect.y += 1 + margin;
        }

        public static void ShadowLabel(Rect rect, string label, GUIStyle style)
        {
            EditorGUI.LabelField(rect, label, style.Darker());
            rect.x -= 1; rect.y -= 1;
            EditorGUI.LabelField(rect, label, style);
        }

        public static void PlainBGLabel(Rect rect, string label, Color backgroundColor, bool isHeader = false, string hint = null)
        {
            using var backgroundScope = BackgroundColorScope.Set(backgroundColor);
            EditorGUI.LabelField(rect, new GUIContent(label, hint), isHeader ? SkyxStyles.PlainBGHeader : SkyxStyles.PlainBGLabel);
        }

        public static void PlainBGLabel(Rect rect, string label, EColor color, EElementSize size)
        {
            using var backgroundScope = BackgroundColorScope.Set(SkyxStyles.HeaderColor(color));
            EditorGUI.LabelField(rect, label, SkyxStyles.HeaderText(size));
        }

        #region Rect Control

        public static Rect GetDividedControlRect(int divisions)
        {
            var rect = EditorGUILayout.GetControlRect();
            rect.DivideRect(divisions);

            return rect;
        }

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

        public static Rect ExtractMediumButton(ref Rect rect, bool fromEnd = false) => ExtractRect(ref rect, SkyxStyles.MediumButtonSize, fromEnd);
        public static Rect ExtractSmallButton(ref Rect rect, bool fromEnd = false) => ExtractRect(ref rect, SkyxStyles.SmallButtonSize, fromEnd);
        public static Rect ExtractMiniButton(ref Rect rect, bool fromEnd = false) => ExtractRect(ref rect, SkyxStyles.MiniButtonSize, fromEnd);
        public static Rect ExtractHint(ref Rect rect, bool fromEnd = false) => ExtractRect(ref rect, SkyxStyles.HintIconWidth, fromEnd);

        public static void AdjustRectToLine(ref Rect rect, bool applyMargin = true)
        {
            rect.height = SkyxStyles.LineHeight;
            if (applyMargin) rect.y += 2;
        }

        public static void ExtractLineDef(ref Rect rect, out float startX, out float totalWidth)
        {
            startX = rect.x;
            totalWidth = rect.width;
        }

        public static void ApplyStartMargin(ref Rect rect, float margin)
        {
            rect.x += margin;
            rect.width -= margin;
        }

        public static void ApplyMargin(ref Rect rect, float margin, bool vertical, bool horizontal)
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