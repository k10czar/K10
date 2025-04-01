using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class FoldoutScope : GUI.Scope
    {
        public readonly bool isExpanded;
        private readonly bool usesLayout;

        private static bool ReallyDraw(Rect headerRect, Rect boxRect, string title, ref bool isExpandedRef, EColor color, EHeaderSize size, SerializedProperty property)
        {
            BoxGUI.DrawBox(boxRect, color);

            var drawingRect = headerRect;
            drawingRect.ApplyStartMargin(10);
            GUI.Toggle(drawingRect.ExtractMiniButton(), isExpandedRef, GUIContent.none, EditorStyles.foldout);
            EditorGUI.LabelField(drawingRect, title, SkyxStyles.BoldStyle);

            var current = Event.current;
            if (current.type == EventType.MouseDown && headerRect.Contains(current.mousePosition))
            {
                if (current.button == 0)
                {
                    isExpandedRef = !isExpandedRef;
                    current.Use();
                }
                else if (current.button == 1 && property != null)
                {
                    PropertyContextMenu.Open(property);
                    current.Use();
                }
            }

            return isExpandedRef;
        }

        private static bool GetDrawingRects(string title, ref bool isExpandedRef, EColor color, EHeaderSize size, SerializedProperty property)
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

            ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color, size, property);

            return initialExpanded;
        }

        private static bool AdjustAvailableRect(ref Rect initialRect, string title, ref bool isExpandedRef, EColor color, EHeaderSize size, SerializedProperty property)
        {
            initialRect.height -= SkyxStyles.ElementsMargin;

            var headerHeight = SkyxStyles.HeaderHeight(size);
            var headerRect = initialRect;
            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var boxRect = initialRect;

            initialRect.ApplyBoxMargin(headerHeight);

            return ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color, size, property);
        }

        private static bool BeginWrapper(string title, SerializedProperty property, EColor color, EHeaderSize size)
        {
            var isExpandedRef = property.isExpanded;
            property.isExpanded = GetDrawingRects(title, ref isExpandedRef, color, size, property);

            return isExpandedRef;
        }

        private static bool BeginWrapper(ref Rect initialRect, string title, SerializedProperty property, EColor color, EHeaderSize size)
        {
            var isExpanded = property.isExpanded;
            property.isExpanded = AdjustAvailableRect(ref initialRect, title, ref isExpanded, color, size, property);

            return property.isExpanded;
        }

        public FoldoutScope(SerializedProperty property, EColor color = EColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
            : this(property, ObjectNames.NicifyVariableName(property.name), color, size) {}

        public FoldoutScope(SerializedProperty property, string title, EColor color = EColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
        {
            usesLayout = true;
            isExpanded = BeginWrapper(title, property, color, size);
        }

        public FoldoutScope(string title, ref bool isExpandedRef, EColor color = EColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
        {
            usesLayout = true;
            isExpanded = GetDrawingRects(title, ref isExpandedRef, color, size, null);
        }

        public FoldoutScope(ref Rect rect, string title, ref bool isExpandedRef, EColor color = EColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
        {
            isExpanded = AdjustAvailableRect(ref rect, title, ref isExpandedRef, color, size, null);
        }

        public FoldoutScope(ref Rect rect, SerializedProperty property, EColor color = EColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
            : this(ref rect, property, property.PrettyName(), color, size) {}

        public FoldoutScope(ref Rect rect, SerializedProperty property, string title, EColor color = EColor.Secondary, EHeaderSize size = EHeaderSize.SingleLine)
        {
            isExpanded = BeginWrapper(ref rect, title, property, color, size);
        }

        protected override void CloseScope()
        {
            if (isExpanded && usesLayout) EditorGUILayout.EndVertical();
        }
    }
}