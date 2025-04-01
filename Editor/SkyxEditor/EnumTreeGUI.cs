#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class EnumTreeGUI
    {
        public static void DrawSecondary<T>(Rect rect, SerializedProperty property, string hint = null) => DrawEnum(rect, property, typeof(T), Colors.Console.Secondary, hint);
        public static void DrawSupport<T>(Rect rect, SerializedProperty property, string hint = null) => DrawEnum(rect, property, typeof(T), Colors.Console.GrayOut, hint);
        public static void DrawSecondary(Rect rect, SerializedProperty property, Type enumType, string hint = null) => DrawEnum(rect, property, enumType, Colors.Console.Secondary, hint);

        public static void DrawEnum<T>(Rect rect, SerializedProperty property, EColor color, string hint = "")
            => DrawEnum(rect, property, typeof(T), color.Get(), hint);

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
        public static void DrawSwitch<T>(Rect rect, SerializedProperty property, string hint = null, bool nicify = true)
            where T: Enum => DrawSwitch(rect, property, Enum.GetNames(typeof(T)), hint, nicify);

        public static void DrawSwitch(Rect rect, SerializedProperty property, string hint = null, bool nicify = true)
            => DrawSwitch(rect, property, property.enumNames, hint, nicify);

        public static void DrawSwitch(Rect rect, SerializedProperty property, string[] enumNames, string hint = null, bool nicify = true)
        {
            var enumValueCount = enumNames.Length;
            var currentIndex = Mathf.Abs(property.intValue) % enumValueCount;

            var label = nicify ? ObjectNames.NicifyVariableName(enumNames[currentIndex]) : enumNames[currentIndex];
            var color = Colors.FromSequence(currentIndex);

            var clicked = SkyxGUI.Button(rect, label, color, SkyxStyles.ButtonStyle, hint);

            if (clicked)
            {
                property.intValue = (currentIndex + 1) % enumValueCount;
                property.Apply();
            }
        }

        // Only works with sequenced enums!
        public static void DrawIntSwitch(Rect rect, SerializedProperty property, string[] names, string hint = null, Color[] colorSequence = null)
        {
            var currentIndex = property.intValue;
            var enumValueCount = names.Length;

            colorSequence ??= Colors.EColorSequence;
            var color = colorSequence[currentIndex];

            var label = names[currentIndex];

            var clicked = SkyxGUI.Button(rect, label, color, SkyxStyles.ButtonStyle, hint);

            if (clicked)
            {
                property.intValue = (currentIndex + 1) % enumValueCount;
                property.Apply();
            }
        }

        public static void DrawEnumMask<T>(Rect rect, SerializedProperty property, EColor color = EColor.Primary, string hint = null) where T : Enum
            => DrawEnumMask(rect, property, typeof(T), color, hint);

        public static void DrawEnumMask(Rect rect, SerializedProperty property, Type enumType, EColor color = EColor.Primary, string hint = null)
        {
            var value = (Enum) Enum.ToObject(enumType, property.intValue);
            using var colorScope = new BackgroundColorScope(color.Get());

            var newValue = (int)(object) EditorGUI.EnumFlagsField(rect, value);

            if (newValue != property.intValue)
            {
                property.intValue = newValue;
                property.Apply();
            }

            var fullHint = $"[{enumType}] {hint}";
            SkyxGUI.DrawHintOverlay(rect, fullHint);
        }
    }
}
#endif