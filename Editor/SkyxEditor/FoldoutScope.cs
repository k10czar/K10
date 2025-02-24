using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class FoldoutScope : GUI.Scope
    {
        public readonly bool isExpanded;
        private readonly bool usesLayout;

        private static bool ReallyDraw(Rect headerRect, Rect boxRect, string title, ref bool isExpandedRef, EConsoleColor color, EHeaderSize size)
        {
            BoxGUI.DrawBox(boxRect, color);
            EditorGUI.DrawRect(headerRect, SkyxStyles.HeaderColor(color));

            // Define and draw foldout toggle
            Rect foldoutRect = new Rect(headerRect.x + SkyxStyles.BoxMargin, headerRect.y, SkyxStyles.MiniButtonSize, headerRect.height);
            GUI.Toggle(foldoutRect, isExpandedRef, GUIContent.none, EditorStyles.foldout);

            // Define and draw title label
            Rect labelRect = new Rect(foldoutRect.xMax, headerRect.y, headerRect.width - foldoutRect.xMax + SkyxStyles.BoxMargin, headerRect.height);
            EditorGUI.LabelField(labelRect, title, SkyxStyles.BoldStyle);

            // Handle mouse events for foldout interaction
            headerRect.xMax -= SkyxStyles.LineHeight + EditorGUIUtility.standardVerticalSpacing;
            Event e = Event.current;
            if (headerRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                isExpandedRef = !isExpandedRef;
                e.Use();
            }

            return isExpandedRef;
        }

        private static bool GetDrawingRects(string title, ref bool isExpandedRef, EConsoleColor color, EHeaderSize size)
        {
            var headerHeight = SkyxStyles.HeaderHeight(size);
            var headerRect = EditorGUILayout.GetControlRect(false, headerHeight);
            var boxRect = headerRect;

            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var initialExpanded = isExpandedRef;

            if (isExpandedRef)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color, size);

            return initialExpanded;
        }

        private static bool AdjustAvailableRect(ref Rect initialRect, string title, ref bool isExpandedRef, EConsoleColor color, EHeaderSize size)
        {
            initialRect.height -= SkyxStyles.ElementsMargin;

            var headerHeight = SkyxStyles.HeaderHeight(size);
            var headerRect = initialRect;
            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var boxRect = initialRect;

            initialRect.ApplyBoxMargin(headerHeight);

            return ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color, size);
        }

        private static bool BeginWrapper(string title, SerializedProperty property, EConsoleColor color, EHeaderSize size)
        {
            var isExpandedRef = property.isExpanded;
            property.isExpanded = GetDrawingRects(title, ref isExpandedRef, color, size);

            return isExpandedRef;
        }

        private static bool BeginWrapper(ref Rect initialRect, string title, SerializedProperty property, EConsoleColor color, EHeaderSize size)
        {
            var isExpanded = property.isExpanded;
            property.isExpanded = AdjustAvailableRect(ref initialRect, title, ref isExpanded, color, size);

            return property.isExpanded;
        }

        public FoldoutScope(SerializedProperty property, EConsoleColor color = EConsoleColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
            : this(property, ObjectNames.NicifyVariableName(property.name), color, size) {}

        public FoldoutScope(SerializedProperty property, string title, EConsoleColor color = EConsoleColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
        {
            usesLayout = true;
            isExpanded = BeginWrapper(title, property, color, size);
        }

        public FoldoutScope(string title, ref bool isExpandedRef, EConsoleColor color = EConsoleColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
        {
            usesLayout = true;
            isExpanded = GetDrawingRects(title, ref isExpandedRef, color, size);
        }

        public FoldoutScope(ref Rect rect, SerializedProperty property, EConsoleColor color = EConsoleColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
            : this(ref rect, property, property.PrettyName(), color, size) {}

        public FoldoutScope(ref Rect rect, SerializedProperty property, string title, EConsoleColor color = EConsoleColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
        {
            isExpanded = BeginWrapper(ref rect, title, property, color, size);
        }

        protected override void CloseScope()
        {
            if (isExpanded && usesLayout) EditorGUILayout.EndVertical();
        }
    }
}