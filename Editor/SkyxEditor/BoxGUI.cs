using UnityEngine;
using UnityEditor;

namespace Skyx.SkyxEditor
{
    public static class BoxGUI
    {
        #region ====== RECT BOXES ======

        public static void DrawBorderBox(Rect rect, RectOffset border, Color color)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Color orgColor = GUI.color;

            GUI.color *= color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, border.top), EditorGUIUtility.whiteTexture); //top
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - border.bottom, rect.width, border.bottom), EditorGUIUtility.whiteTexture); //bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + border.left, border.left, rect.height - 2 * border.left), EditorGUIUtility.whiteTexture); //left
            GUI.DrawTexture(new Rect(rect.xMax - border.right, rect.y + border.right, border.right, rect.height - 2 * border.right), EditorGUIUtility.whiteTexture); //right

            GUI.color = orgColor;
        }

        private static void DrawCorners(Rect rect, Vector2 cornerSize, Color cornerColor)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, cornerSize.x, cornerSize.y), cornerColor);

            // Draw top-right corner
            EditorGUI.DrawRect(new Rect(rect.xMax - cornerSize.x, rect.y, cornerSize.x, cornerSize.y), cornerColor);

            // Draw bottom-left corner
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - cornerSize.y, cornerSize.x, cornerSize.y), cornerColor);

            // Draw bottom-right corner
            EditorGUI.DrawRect(new Rect(rect.xMax - cornerSize.x, rect.yMax - cornerSize.y, cornerSize.x, cornerSize.y), cornerColor);
        }

        private static void DrawCorners(Rect rect, Vector2 cornerSize, float thickness, Color cornerColor)
        {
            // Draw top-left corner
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, cornerSize.x, thickness), cornerColor); // Horizontal part of L
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, cornerSize.y), cornerColor); // Vertical part of L

            // Draw top-right corner
            EditorGUI.DrawRect(new Rect(rect.xMax - cornerSize.x, rect.y, cornerSize.x, thickness), cornerColor); // Horizontal part of L
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, cornerSize.y), cornerColor); // Vertical part of L

            // Draw bottom-left corner
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, cornerSize.x, thickness), cornerColor); // Horizontal part of L
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - cornerSize.y, thickness, cornerSize.y), cornerColor); // Vertical part of L

            // Draw bottom-right corner
            EditorGUI.DrawRect(new Rect(rect.xMax - cornerSize.x, rect.yMax - thickness, cornerSize.x, thickness), cornerColor); // Horizontal part of L
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.yMax - cornerSize.y, thickness, cornerSize.y), cornerColor); // Vertical part of L
        }

        public static void DrawHeader(Rect headerRect, GUIContent title, GUIStyle labelStyle = null)
        {
            EditorGUI.DrawRect(headerRect, SkyxStyles.DefaultHeaderColor);

            Rect labelRect = new Rect(headerRect.x + SkyxStyles.BoxMargin, headerRect.y - 1f, headerRect.width - SkyxStyles.BoxMargin, headerRect.height);
            EditorGUI.LabelField(labelRect, title, labelStyle ?? SkyxStyles.BoldStyle);
        }

        public static bool DrawToggleHeader(Rect headerRect, GUIContent title, bool toggle)
        {
            EditorGUI.DrawRect(headerRect, SkyxStyles.DefaultHeaderColor);

            Rect toggleRect = new Rect(headerRect.x + SkyxStyles.BoxMargin, headerRect.y, EditorGUIUtility.singleLineHeight, headerRect.height);
            toggle = GUI.Toggle(toggleRect, toggle, new GUIContent("", "Enabled"), EditorStyles.toggle);

            Rect labelRect = new Rect(toggleRect.xMax, headerRect.y - 1f, headerRect.width - toggleRect.xMax, headerRect.height);
            EditorGUI.LabelField(labelRect, title, SkyxStyles.BoldStyle);

            return toggle;
        }

        public static void DrawFoldoutToggleHeader(Rect headerRect, GUIContent title, ref bool expanded, ref bool toggle)
        {
            // Draw header background
            EditorGUI.DrawRect(headerRect, SkyxStyles.DefaultHeaderColor);

            // Set up initial positions
            Rect foldoutRect = headerRect;
            foldoutRect.width = SkyxStyles.LineHeight;
            foldoutRect.x += SkyxStyles.BoxMargin;

            GUI.Toggle(foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);
            foldoutRect.x += SkyxStyles.LineHeight;

            // Draw toggle
            Rect toggleRect = new Rect(foldoutRect.x, headerRect.y, SkyxStyles.LineHeight, headerRect.height);
            toggle = GUI.Toggle(toggleRect, toggle, new GUIContent("", "Enabled"), EditorStyles.toggle);

            // Draw title
            Rect labelRect = new Rect(toggleRect.xMax, headerRect.y - 1f, headerRect.width - toggleRect.xMax, headerRect.height);
            EditorGUI.LabelField(labelRect, title, SkyxStyles.BoldStyle);

            // Handle events
            headerRect.xMax -= SkyxStyles.LineHeight + 2f;
            Event e = Event.current;
            if (headerRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                expanded = !expanded;
                e.Use();
            }
        }

        public static Rect DrawHeaderWithBorder(ref Rect rect, GUIContent title, float headerHeight = SkyxStyles.BoxHeaderHeight)
        {
            DrawBox(rect);
            rect.x += 1;
            rect.y += 1;
            rect.height -= 1;
            rect.width -= 2;

            Rect headerRect = rect;
            headerRect.height = headerHeight + EditorGUIUtility.standardVerticalSpacing;

            rect.y += headerRect.height;
            rect.height -= headerRect.height;

            Rect titleRect = headerRect;
            titleRect.x += 2f;

            using (new IconSizeScope(14))
            {
                EditorGUI.DrawRect(headerRect, SkyxStyles.DefaultHeaderColor);
                EditorGUI.LabelField(titleRect, title, EditorStyles.miniBoldLabel);
            }

            return headerRect;
        }

        public static void DrawBox(Rect rect) => DrawBox(rect, EColor.Primary);
        public static void DrawBox(Rect rect, EColor color)
        {
            using var scope = new BackgroundColorScope(SkyxStyles.BoxColor(color));
            GUI.Box(rect, GUIContent.none, SkyxStyles.BoxStyle(color));
        }

        public static void ShrinkHeaderRect(ref Rect headerRect, float headerHeight)
        {
            headerRect.height = headerHeight - 2;
            headerRect.y += 1;
            headerRect.x += 1;
            headerRect.width -= 2;
        }

        #endregion

        #region ====== LAYOUT BOXES ======

        public static void BeginBox()
        {
            var drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxStyle);
            DrawBox(drawingRect);
        }

        public static Rect BeginHeaderBox(GUIContent title, float headerHeight = SkyxStyles.BoxHeaderHeight)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + SkyxStyles.BoxMargin);
            Rect drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
            drawingRect.yMin -= headerHeight + 6f;

            DrawBox(drawingRect);
            DrawHeader(headerRect, title);
            return headerRect;
        }

        public static void EndBox() => EditorGUILayout.EndVertical();

        #region Toggle Boxes

        public static bool BeginToggle(GUIContent title, ref bool toggle, float headerHeight = SkyxStyles.BoxHeaderHeight)
            => BeginToggle(title, ref toggle, out _, headerHeight);

        public static bool BeginToggle(GUIContent title, ref bool toggle, out Rect headerRect, float headerHeight = SkyxStyles.BoxHeaderHeight)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + SkyxStyles.BoxMargin);
            Rect drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
            drawingRect.yMin -= headerHeight + 6f;

            DrawBox(drawingRect);
            return DrawToggleHeader(headerRect, title, toggle);
        }

        #endregion

        #region Foldout & Toggle Boxes

        public static bool BeginFoldoutToggle(GUIContent title, SerializedProperty foldoutProperty, ref bool toggle, float headerHeight = SkyxStyles.BoxHeaderHeight)
            => BeginFoldoutToggle(title, foldoutProperty, ref toggle, out _, headerHeight);

        public static bool BeginFoldoutToggle(GUIContent title, SerializedProperty foldoutProperty, ref bool toggle, out Rect headerRect, float headerHeight = SkyxStyles.BoxHeaderHeight)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + SkyxStyles.BoxMargin);
            Rect boxRect = headerRect;

            bool expanded = foldoutProperty.isExpanded;
            if (expanded)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            DrawBox(boxRect);
            DrawFoldoutToggleHeader(headerRect, title, ref expanded, ref toggle);
            foldoutProperty.isExpanded = expanded;
            return expanded;
        }

        #endregion

        #endregion
    }
}