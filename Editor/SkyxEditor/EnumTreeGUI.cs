#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class EnumTreeGUI
    {
        public static void DrawPrimary<T>(Rect rect, SerializedProperty property, string hint = null) => DrawEnum(rect, property, typeof(T), Colors.Console.Primary, hint);
        public static void DrawSecondary<T>(Rect rect, SerializedProperty property, string hint = null) => DrawEnum(rect, property, typeof(T), Colors.Console.Secondary, hint);
        public static void DrawSupport<T>(Rect rect, SerializedProperty property, string hint = null) => DrawEnum(rect, property, typeof(T), Colors.Console.GrayOut, hint);

        public static void DrawPrimary(Rect rect, SerializedProperty property, Type enumType, string hint = null) => DrawEnum(rect, property, enumType, Colors.Console.Primary, hint);
        public static void DrawSecondary(Rect rect, SerializedProperty property, Type enumType, string hint = null) => DrawEnum(rect, property, enumType, Colors.Console.Secondary, hint);

        public static void DrawEnum<T>(Rect rect, SerializedProperty property, EConsoleColor color, string hint = "")
            => DrawEnum(rect, property, typeof(T), Colors.Console.Get(color), hint);

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

            var clicked = SkyxGUI.RectButton(rect, label, color, SkyxStyles.ButtonStyle, hint);

            if (clicked) property.intValue = (currentIndex + 1) % enumValueCount;
        }

        // Only works with sequenced enums!
        public static void DrawIntSwitch(Rect rect, SerializedProperty property, string[] names, string hint = null, Color[] colorSequence = null)
        {
            var currentIndex = property.intValue;
            var enumValueCount = names.Length;

            colorSequence ??= Colors.OptionsSequence;
            var color = colorSequence[currentIndex];

            var label = names[currentIndex];

            var clicked = SkyxGUI.RectButton(rect, label, color, SkyxStyles.ButtonStyle, hint);

            if (clicked) property.intValue = (currentIndex + 1) % enumValueCount;
        }

        public static void DrawEnumMask<T>(Rect rect, SerializedProperty property, EConsoleColor color = EConsoleColor.Primary, string hint = null) where T : Enum
        {
            using var colorScope = new BackgroundColorScope(Colors.Console.Get(color));
            property.intValue = (int)(object) EditorGUI.EnumFlagsField(rect, (T)(object) property.intValue);

            var fullHint = $"[{typeof(T)}] {hint}";
            SkyxGUI.DrawHintOverlay(rect, fullHint);
        }
    }
}
#endif