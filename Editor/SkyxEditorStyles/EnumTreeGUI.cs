#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Skyx.CustomEditor
{
    public static class EnumTreeGUI
    {
        public static void DrawPrimaryEnum(Rect rect, SerializedProperty property, Type enumType, string hint = null) => DrawEnum(rect, property, enumType, SkyxStyles.desaturatedPrimary, hint);
        public static void DrawSecondaryEnum(Rect rect, SerializedProperty property, Type enumType, string hint = null) => DrawEnum(rect, property, enumType, SkyxStyles.transparentInfo, hint);

        public static void DrawEnum(Rect rect, SerializedProperty property, Type enumType, Color color, string hint = "")
        {
            EnumTreeDrawer.DrawEnumDropdown(
                rect,
                property,
                color,
                SkyxStyles.PopupStyle,
                enumType);

            var value = Enum.ToObject(enumType, property.intValue);
            var fullHint = $"[{enumType.Name}.{value}] {hint}";

            SkyxGUI.DrawHintOverlay(rect, fullHint);
        }

        // Only works with sequenced enums!
        public static void DrawEnumSwitch(Rect rect, SerializedProperty property, Type enumType, string hint = null, bool nicify = true)
            => DrawEnumSwitch(rect, property, Enum.GetNames(enumType), hint, nicify);

        public static void DrawEnumSwitch(Rect rect, SerializedProperty property, string hint = null, bool nicify = true)
            => DrawEnumSwitch(rect, property, property.enumNames, hint, nicify);

        public static void DrawEnumSwitch(Rect rect, SerializedProperty property, string[] enumNames, string hint = null, bool nicify = true)
        {
            var enumValueCount = enumNames.Length;
            var currentIndex = Mathf.Abs(property.intValue) % enumValueCount;

            var label = nicify ? ObjectNames.NicifyVariableName(enumNames[currentIndex]) : enumNames[currentIndex];
            var color = SkyxStyles.ColorFromSequence(currentIndex);

            var clicked = SkyxGUI.RectButton(rect, label, color, SkyxStyles.ButtonStyle, hint);

            if (clicked) property.intValue = (currentIndex + 1) % enumValueCount;
        }

        // Only works with sequenced enums!
        public static void DrawIntEnumSwitch(Rect rect, SerializedProperty property, string[] names, string hint = null, Color[] colorSequence = null)
        {
            var currentIndex = property.intValue;
            var enumValueCount = names.Length;

            colorSequence ??= SkyxStyles.ColorSequence;
            var color = colorSequence[currentIndex];

            var label = names[currentIndex];

            var clicked = SkyxGUI.RectButton(rect, label, color, SkyxStyles.ButtonStyle, hint);

            if (clicked) property.intValue = (currentIndex + 1) % enumValueCount;
        }
    }
}
#endif