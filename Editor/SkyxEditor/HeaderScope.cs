using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class HeaderScope : GUI.Scope
    {
        public readonly bool isExpanded;
        private readonly bool usesLayout;

        private static bool Begin(string title, SerializedProperty property, EColor color, EHeaderSize size)
        {
            var initialExpanded = property.isExpanded;
            var isExpanded = property.isExpanded;
            property.isExpanded = Begin(title, ref isExpanded, color, size, property);

            return initialExpanded;
        }

        private static bool Begin(string title, ref bool isExpandedRef, EColor color, EHeaderSize size, SerializedProperty property)
        {
            var headerHeight = SkyxStyles.HeaderHeight(size);
            var headerRect = EditorGUILayout.GetControlRect(false, headerHeight);
            var boxRect = headerRect;

            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            if (isExpandedRef)
            {
                var drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            return ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color, size, property);
        }

        private static bool Begin(ref Rect initialRect, string title, SerializedProperty property, EColor color, EHeaderSize size)
        {
            var isExpanded = property.isExpanded;
            property.isExpanded = Begin(ref initialRect, title, ref isExpanded, color, size, property);

            return property.isExpanded;
        }

        private static bool Begin(ref Rect initialRect, string title, ref bool isExpandedRef, EColor color, EHeaderSize size, SerializedProperty property)
        {
            initialRect.height -= SkyxStyles.ElementsMargin;

            var headerHeight = SkyxStyles.HeaderHeight(size);
            var headerRect = initialRect;
            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var boxRect = initialRect;

            initialRect.ApplyBoxMargin(headerHeight);

            return ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color, size, property);
        }

        private static bool ReallyDraw(Rect headerRect, Rect boxRect, string title, ref bool isExpandedRef, EColor color, EHeaderSize size, SerializedProperty property)
        {
            BoxGUI.DrawBox(boxRect, color);

            SkyxGUI.PlainBGLabel(headerRect, title, color, size);

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

        public HeaderScope(SerializedProperty property, EColor color = EColor.Primary, EHeaderSize size = EHeaderSize.Primary)
            : this(property, property.PrettyName(), color, size) {}

        public HeaderScope(SerializedProperty property, string title, EColor color = EColor.Primary, EHeaderSize size = EHeaderSize.Primary)
        {
            usesLayout = true;
            isExpanded = Begin(title, property, color, size);
        }

        public HeaderScope(string title, ref bool isExpandedRef, EColor color = EColor.Primary, EHeaderSize size = EHeaderSize.Primary)
        {
            isExpanded = isExpandedRef;
            usesLayout = true;
            Begin(title, ref isExpandedRef, color, size, null);
        }

        public HeaderScope(ref Rect rect, SerializedProperty property, EColor color = EColor.Primary, EHeaderSize size = EHeaderSize.Primary)
            : this(ref rect, property, property.PrettyName(), color, size) {}

        public HeaderScope(ref Rect rect, SerializedProperty property, string title, EColor color = EColor.Primary, EHeaderSize size = EHeaderSize.Primary)
        {
            isExpanded = Begin(ref rect, title, property, color, size);
        }

        public HeaderScope(ref Rect rect, string title, ref bool isExpandedRef, EColor color = EColor.Primary, EHeaderSize size = EHeaderSize.Primary)
        {
            isExpanded = Begin(ref rect, title, ref isExpandedRef, color, size, null);
        }

        protected override void CloseScope()
        {
            if (isExpanded && usesLayout) EditorGUILayout.EndVertical();
        }
    }
}