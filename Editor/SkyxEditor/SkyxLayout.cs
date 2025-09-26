using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    public static class SkyxLayout
    {
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

        #region Buttons

        public static bool PlainBGButton(string label, EColor color, EElementSize size = EElementSize.SingleLine)
            => Button(label, SkyxStyles.HeaderColor(color), size.GetPlainBG());

        public static bool PlainBGHeaderButton(string label, EColor color) => Button(label, color.Get(), SkyxStyles.PlainBGHeader);
        public static bool PlainBGHeaderButton(string label, Color color) => Button(label, color, SkyxStyles.PlainBGHeader);

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static bool Button(string label) => Button(label, Color.white, SkyxStyles.ButtonStyle);
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static bool Button(string label, EColor color) => Button(label, color.Get(), SkyxStyles.ButtonStyle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Button(string label, EColor color = EColor.Support, EElementSize size = EElementSize.SingleLine, EButtonType type = EButtonType.Default)
            => Button(label, color.Get(), type.GetButton(size, color));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Button(string label, Color backgroundColor) => Button(label, backgroundColor, SkyxStyles.ButtonStyle);

        public static bool Button(string label, Color backgroundColor, GUIStyle style, params GUILayoutOption[] layouts)
        {
            style ??= SkyxStyles.ButtonStyle;

            using var backgroundScope = BackgroundColorScope.Set(backgroundColor);
            var result = GUILayout.Button(label, style, layouts);

            return result;
        }

        #endregion

        #region Labels

        public static bool DrawTitle(Object asset)
        {
            Separator();
            var clicked = GUILayout.Button(ObjectNames.NicifyVariableName(asset.name), SkyxStyles.HugeHeader.With(Colors.Console.Light));
            Separator();

            return clicked;
        }

        public static void PlainBGLabel(string label, bool isSuccess, string hint = null) => PlainBGLabel(label, BoolToColor(isSuccess), hint: hint);
        public static void PlainBGLabel(string label) => PlainBGLabel(label, Colors.Console.GrayOut);

        public static void PlainBGLabel(string label, Color backgroundColor, bool isHeader = false, string hint = null)
        {
            using var backgroundScope = BackgroundColorScope.Set(backgroundColor);
            GUILayout.Label(new GUIContent(label, hint), isHeader ? SkyxStyles.PlainBGHeader : SkyxStyles.PlainBGLabel);
        }

        public static void PlainBGLabel(string label, EColor color, EElementSize size, string hint = null)
        {
            using var _ = BackgroundColorScope.Set(color.Get());
            GUILayout.Label(new GUIContent(label, hint), size.GetPlainBG(color));
        }

        private static Color BoolToColor(bool active) => active ? Colors.Console.Success.WithAlpha(0.4f) : Colors.Console.Danger.WithAlpha(0.4f);

        public static void CompactHeader(string label)
        {
            CompactSpace();
            EditorGUILayout.LabelField(label, SkyxStyles.BoldStyle);
        }

        #endregion

        #region Separators

        private static void Separator(Color color, float height, Vector2 margin)
        {
            GUILayout.Space(margin.x);

            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height), color);

            GUILayout.Space(margin.y);
        }

        public static void Separator() => Separator(Colors.Transparent02, SkyxStyles.DefaultSeparatorSize, SkyxStyles.defaultSeparatorMargin);
        public static void SmallSeparator() => Separator(Colors.Transparent02, SkyxStyles.DefaultSeparatorSize, SkyxStyles.smallSeparatorMargin);
        public static void NoMarginSeparator() => Separator(Colors.Transparent02, SkyxStyles.DefaultSeparatorSize, SkyxStyles.noSeparatorMargin);

        public static void GroupSeparator(Rect start, Rect end, Color color, float yOffset = -1, float size = SkyxStyles.DefaultSeparatorSize)
        {
            EditorGUI.DrawRect(new Rect(start.x, start.y - yOffset, size, end.y - start.y - 2 * yOffset), color);
        }

        public static void Space() => EditorGUILayout.Space(SkyxStyles.ElementsMargin);
        public static void CompactSpace() => EditorGUILayout.Space(SkyxStyles.CompactSpace);

        #endregion
    }
}