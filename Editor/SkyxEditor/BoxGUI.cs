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
            EditorGUI.DrawRect(headerRect, SkyxStyles.HeaderColor);

            Rect labelRect = new Rect(headerRect.x + SkyxStyles.BoxMargin, headerRect.y - 1f, headerRect.width - SkyxStyles.BoxMargin, headerRect.height);
            EditorGUI.LabelField(labelRect, title, labelStyle ?? SkyxStyles.BoldStyle);
        }

        public static bool DrawToggleHeader(Rect headerRect, GUIContent title, bool toggle)
        {
            EditorGUI.DrawRect(headerRect, SkyxStyles.HeaderColor);

            Rect toggleRect = new Rect(headerRect.x + SkyxStyles.BoxMargin, headerRect.y, EditorGUIUtility.singleLineHeight, headerRect.height);
            toggle = GUI.Toggle(toggleRect, toggle, new GUIContent("", "Enabled"), EditorStyles.toggle);

            Rect labelRect = new Rect(toggleRect.xMax, headerRect.y - 1f, headerRect.width - toggleRect.xMax, headerRect.height);
            EditorGUI.LabelField(labelRect, title, SkyxStyles.BoldStyle);

            return toggle;
        }

        public static bool DrawFoldoutHeader(Rect headerRect, string title, bool expanded)
        {
            EditorGUI.DrawRect(headerRect, SkyxStyles.HeaderColor);

            // Define and draw foldout toggle
            Rect foldoutRect = new Rect(headerRect.x + SkyxStyles.BoxMargin, headerRect.y, SkyxStyles.LineHeight, headerRect.height);
            GUI.Toggle(foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);

            // Define and draw title label
            Rect labelRect = new Rect(foldoutRect.xMax, headerRect.y, headerRect.width - foldoutRect.xMax + SkyxStyles.BoxMargin, headerRect.height);
            EditorGUI.LabelField(labelRect, title, SkyxStyles.BoldStyle);

            // Handle mouse events for foldout interaction
            headerRect.xMax -= SkyxStyles.LineHeight + EditorGUIUtility.standardVerticalSpacing;
            Event e = Event.current;
            if (headerRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                expanded = !expanded;
                e.Use();
            }

            return expanded;
        }

        public static void DrawFoldoutToggleHeader(Rect headerRect, GUIContent title, ref bool expanded, ref bool toggle)
        {
            // Draw header background
            EditorGUI.DrawRect(headerRect, SkyxStyles.HeaderColor);

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

        public static Rect DrawHeaderWithBorder(ref Rect rect, GUIContent title, float headerHeight = SkyxStyles.BoxHeaderHeight, bool roundedBox = true)
        {
            DrawBox(ref rect, roundedBox);
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
                EditorGUI.DrawRect(headerRect, SkyxStyles.HeaderColor);
                EditorGUI.LabelField(titleRect, title, EditorStyles.miniBoldLabel);
            }

            return headerRect;
        }

        public static void DrawBox(ref Rect rect, bool roundedBox) => GUI.Box(rect, GUIContent.none, SkyxStyles.BoxStyle(roundedBox));

        #endregion

        #region ====== LAYOUT BOXES ======

        public static void BeginBox(bool roundedBox = true)
        {
            var drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxStyle);
            DrawBox(ref drawingRect, roundedBox);
        }

        public static Rect BeginHeaderBox(GUIContent title, float headerHeight = SkyxStyles.BoxHeaderHeight, bool roundedBox = true)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + SkyxStyles.BoxMargin);
            Rect drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
            drawingRect.yMin -= headerHeight + 6f;

            DrawBox(ref drawingRect, roundedBox);
            DrawHeader(headerRect, title);
            return headerRect;
        }

        public static void EndBox() => EditorGUILayout.EndVertical();

        #region Foldout Boxes

        public static bool BeginFoldout(string title, ref bool expanded, float headerHeight = SkyxStyles.BoxHeaderHeight, bool roundedBox = true)
            => BeginFoldout(title, ref expanded, out _, headerHeight, roundedBox);

        public static bool BeginFoldout(string title, ref bool expanded, out Rect headerRect, float headerHeight = SkyxStyles.BoxHeaderHeight, bool roundedBox = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + SkyxStyles.BoxMargin);
            var boxRect = headerRect;

            if (expanded)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            DrawBox(ref boxRect, roundedBox);
            expanded = DrawFoldoutHeader(headerRect, title, expanded);
            return expanded;
        }

        public static bool BeginFoldout(string title, SerializedProperty property, float headerHeight = SkyxStyles.BoxHeaderHeight, bool roundedBox = true)
            => BeginFoldout(title, property, out _, headerHeight, roundedBox);

        public static bool BeginFoldout(string title, SerializedProperty property, out Rect headerRect, float headerHeight = SkyxStyles.BoxHeaderHeight, bool roundedBox = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + SkyxStyles.BoxMargin);
            var boxRect = headerRect;
            var returnValue = property.isExpanded;

            if (property.isExpanded)
            {
                var drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            DrawBox(ref boxRect, roundedBox);
            property.isExpanded = DrawFoldoutHeader(headerRect, title, property.isExpanded);
            return returnValue;
        }

        #endregion

        #region Toggle Boxes

        public static bool BeginToggle(GUIContent title, ref bool toggle, float headerHeight = SkyxStyles.BoxHeaderHeight, bool roundedBox = true)
            => BeginToggle(title, ref toggle, out _, headerHeight, roundedBox);

        public static bool BeginToggle(GUIContent title, ref bool toggle, out Rect headerRect, float headerHeight = SkyxStyles.BoxHeaderHeight, bool roundedBox = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + SkyxStyles.BoxMargin);
            Rect drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
            drawingRect.yMin -= headerHeight + 6f;

            DrawBox(ref drawingRect, roundedBox);
            return DrawToggleHeader(headerRect, title, toggle);
        }

        #endregion

        #region Foldout & Toggle Boxes

        public static bool BeginFoldoutToggle(GUIContent title, SerializedProperty foldoutProperty, ref bool toggle, float headerHeight = SkyxStyles.BoxHeaderHeight, bool roundedBox = true)
            => BeginFoldoutToggle(title, foldoutProperty, ref toggle, out _, headerHeight, roundedBox);

        public static bool BeginFoldoutToggle(GUIContent title, SerializedProperty foldoutProperty, ref bool toggle, out Rect headerRect, float headerHeight = SkyxStyles.BoxHeaderHeight, bool roundedBox = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + SkyxStyles.BoxMargin);
            Rect boxRect = headerRect;

            bool expanded = foldoutProperty.isExpanded;
            if (expanded)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            DrawBox(ref boxRect, roundedBox);
            DrawFoldoutToggleHeader(headerRect, title, ref expanded, ref toggle);
            foldoutProperty.isExpanded = expanded;
            return expanded;
        }

        #endregion

        #endregion
    }
}