using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class FoldoutBoxScope : GUI.Scope
    {
        public readonly bool isExpanded;

        private static bool DrawFoldoutHeader(Rect headerRect, string title, bool expanded, EConsoleColor color)
        {
            EditorGUI.DrawRect(headerRect, SkyxStyles.HeaderColor(color));

            // Define and draw foldout toggle
            Rect foldoutRect = new Rect(headerRect.x + SkyxStyles.BoxMargin, headerRect.y, SkyxStyles.SmallButtonSize, headerRect.height);
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

        private static bool BeginFoldout(string title, ref bool expanded, EConsoleColor color, EHeaderSize size)
            => BeginFoldout(title, ref expanded, out _, color, size);

        private static bool BeginFoldout(string title, ref bool expanded, out Rect headerRect, EConsoleColor color, EHeaderSize size)
        {
            headerRect = EditorGUILayout.GetControlRect(false, SkyxStyles.HeaderHeight(size));
            var boxRect = headerRect;

            var initialExpanded = expanded;

            if (expanded)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            BoxGUI.DrawBox(boxRect);
            expanded = DrawFoldoutHeader(headerRect, title, expanded, color);

            return initialExpanded;
        }

        private static bool BeginFoldout(string title, SerializedProperty property, EConsoleColor color, EHeaderSize size)
        {
            var expanded = property.isExpanded;
            property.isExpanded = BeginFoldout(title, ref expanded, out _, color, size);

            return expanded;
        }

        public FoldoutBoxScope(SerializedProperty property, EConsoleColor color = EConsoleColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
            : this(property, ObjectNames.NicifyVariableName(property.name), color, size) {}

        public FoldoutBoxScope(SerializedProperty property, string title, EConsoleColor color = EConsoleColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
        {
            isExpanded = BeginFoldout(title, property, color, size);
        }

        public FoldoutBoxScope(string title, ref bool isExpandedRef, EConsoleColor color = EConsoleColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
        {
            isExpanded = BeginFoldout(title, ref isExpandedRef, color, size);
        }

        protected override void CloseScope()
        {
            if (isExpanded) BoxGUI.EndBox();
        }
    }
}