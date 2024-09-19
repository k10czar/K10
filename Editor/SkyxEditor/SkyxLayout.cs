using System;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class SkyxLayout
    {
        #region Buttons

        private static GUIStyle noBackgroundButton;

        public static bool PrimaryButton(string label, params GUILayoutOption[] layouts) => Button(label, Colors.Console.Dark, null, layouts);
        public static bool SecondaryButton(string label, params GUILayoutOption[] layouts) => Button(label, Colors.Console.GrayOut, null, layouts);
        public static bool ClearButton(string label) => Button(label, Color.clear);

        public static bool PlainBGButton(string label, Color color) => Button(label, color, SkyxStyles.PlainBGLabel);
        public static bool PlainBGHeaderButton(string label, Color color) => Button(label, color, SkyxStyles.PlainBGHeader);
        public static bool PlainBGHeaderButton(string label, bool success) => Button(label, success ? Colors.Console.Success : Colors.Console.Danger, SkyxStyles.PlainBGHeader);

        public static bool Button(string label, Color backgroundColor) => Button(label, backgroundColor, null);

        public static bool Button(string label, Color backgroundColor, GUIStyle style, params GUILayoutOption[] layouts)
        {
            style ??= SkyxStyles.ButtonStyle;

            SetBackgroundColor(backgroundColor);
            var result = GUILayout.Button(label, style, layouts);
            RestoreBackgroundColor();

            return result;
        }

        public static bool FixedSizePrimaryButton(string label, float size) => PrimaryButton(label, GUILayout.Width(size));

        #endregion

        #region Labels

        public static void PlainBGLabel(string label, bool isSuccess, string hint = null) => PlainBGLabel(label, BoolToColor(isSuccess), hint: hint);
        public static void PlainBGLabel(string label) => PlainBGLabel(label, Colors.Console.GrayOut);

        public static void PlainBGLabel(string label, Color backgroundColor, bool isHeader = false, string hint = null)
        {
            DrawWithBGColor(backgroundColor, () => GUILayout.Label(new GUIContent(label, hint), isHeader ? SkyxStyles.PlainBGHeader : SkyxStyles.PlainBGLabel));
        }

        private static Color BoolToColor(bool active) => active ? Colors.Console.Success.WithAlpha(0.4f) : Colors.Console.Danger.WithAlpha(0.4f);

        #endregion

        #region Separators

        public static void Separator(Color color, float height, Vector2 margin)
        {
            GUILayout.Space(margin.x);

            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height), color);

            GUILayout.Space(margin.y);
        }

        public static void HeaderSeparator() => Separator(SkyxStyles.headerSeparatorMargin);
        public static void Separator() => Separator(SkyxStyles.defaultSeparatorColor, SkyxStyles.DefaultSeparatorSize, SkyxStyles.defaultSeparatorMargin);
        public static void Separator(Color color, float height) => Separator(color, height, SkyxStyles.defaultSeparatorMargin);
        public static void Separator(Vector2 margin) => Separator(SkyxStyles.defaultSeparatorColor, SkyxStyles.DefaultSeparatorSize, margin);
        public static void Separator(float height, Vector2 margin) => Separator(SkyxStyles.defaultSeparatorColor, height, margin);

        public static void GroupSeparator(Rect start, Rect end, Color color, float yOffset = -1, float size = SkyxStyles.DefaultSeparatorSize)
        {
            EditorGUI.DrawRect(new Rect(start.x, start.y - yOffset, size, end.y - start.y - 2 * yOffset), color);
        }

        public static void Space() => EditorGUILayout.Space(SkyxStyles.ElementsMargin);

        #endregion

        #region GUI Variables Manipulation

        private static readonly Color DefaultBackgroundColor = GUI.backgroundColor;
        private static readonly Color DefaultGUIColor = GUI.color;
        private static readonly float DefaultLabelWidth = EditorGUIUtility.labelWidth;

        public static void SetAllColors(Color color) => GUI.color = color;
        public static void RestoreAllColors() => GUI.color = DefaultGUIColor;

        public static void SetBackgroundColor(Color color) => GUI.backgroundColor = color;
        public static void RestoreBackgroundColor() => GUI.backgroundColor = DefaultBackgroundColor;

        public static void SetLabelWidth(float value) => EditorGUIUtility.labelWidth = value;
        public static void RevertLabelWidth() => EditorGUIUtility.labelWidth = DefaultLabelWidth;

        #endregion

        #region Modules

        public static bool ShouldShowBlock(string label, SerializedProperty property)
        {
            var isExpanded = property.isExpanded;
            var result = ShouldShowBlock(label, ref isExpanded, Colors.Console.Dark);
            property.isExpanded = result;

            return result;
        }

        public static bool ShouldShowBlock(string label, ref bool switchValue)
            => ShouldShowBlock(label, ref switchValue, Colors.Console.Dark);

        public static bool ShouldShowBlock(string label, string saveKey)
        {
            var switchValue = EditorPrefs.GetBool(saveKey);
            ShouldShowBlock(label, ref switchValue, Colors.Console.Dark);
            EditorPrefs.SetBool(saveKey, switchValue);

            return switchValue;
        }

        public static bool ShouldShowBlock(string label, ref bool switchValue, Color color)
        {
            Separator(color, SkyxStyles.DefaultSeparatorSize, new Vector2(-3, -2));

            if (PlainBGHeaderButton(label, color)) switchValue = !switchValue;
            GUILayout.Space(3);

            return switchValue;
        }

        public static void LeftPadding(Action drawer, int padding = 10)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(padding);
            EditorGUILayout.BeginVertical();

            drawer();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawWithBGColor(Color backgroundColor, Action drawer)
        {
            SetBackgroundColor(backgroundColor);
            drawer();
            RestoreBackgroundColor();
        }

        #endregion
    }
}