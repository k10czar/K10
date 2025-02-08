using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class HeaderScope : GUI.Scope
    {
        public readonly bool isExpanded;
        private readonly bool usesLayout;

        private static bool Begin(string title, SerializedProperty property, EConsoleColor color, EHeaderSize size)
        {
            var initialExpanded = property.isExpanded;
            var isExpanded = property.isExpanded;
            property.isExpanded = Begin(title, ref isExpanded, color, size);

            return initialExpanded;
        }

        private static bool Begin(string title, ref bool isExpandedRef, EConsoleColor color, EHeaderSize size)
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

            return ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color, size);
        }

        private static bool Begin(ref Rect initialRect, string title, SerializedProperty property, EConsoleColor color, EHeaderSize size)
        {
            var isExpanded = property.isExpanded;
            property.isExpanded = Begin(ref initialRect, title, ref isExpanded, color, size);

            return property.isExpanded;
        }

        private static bool Begin(ref Rect initialRect, string title, ref bool isExpandedRef, EConsoleColor color, EHeaderSize size)
        {
            initialRect.height -= SkyxStyles.ElementsMargin;

            var headerHeight = SkyxStyles.HeaderHeight(size);
            var headerRect = initialRect;
            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var boxRect = initialRect;

            initialRect.ApplyBoxMargin(headerHeight);

            return ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color, size);
        }

        private static bool ReallyDraw(Rect headerRect, Rect boxRect, string title, ref bool isExpandedRef, EConsoleColor color, EHeaderSize size)
        {
            BoxGUI.DrawBox(boxRect, color);

            if (SkyxGUI.RectButton(headerRect, title, color, size))
                isExpandedRef = !isExpandedRef;

            return isExpandedRef;
        }

        public HeaderScope(SerializedProperty property, EConsoleColor color = EConsoleColor.Primary, EHeaderSize size = EHeaderSize.Primary)
            : this(property, property.PrettyName(), color, size) {}

        public HeaderScope(SerializedProperty property, string title, EConsoleColor color = EConsoleColor.Primary, EHeaderSize size = EHeaderSize.Primary)
        {
            usesLayout = true;
            isExpanded = Begin(title, property, color, size);
        }

        public HeaderScope(string title, ref bool isExpandedRef, EConsoleColor color = EConsoleColor.Primary, EHeaderSize size = EHeaderSize.Primary)
        {
            usesLayout = true;
            isExpanded = Begin(title, ref isExpandedRef, color, size);
        }

        public HeaderScope(ref Rect rect, SerializedProperty property, EConsoleColor color = EConsoleColor.Primary, EHeaderSize size = EHeaderSize.Primary)
            : this(ref rect, property, property.PrettyName(), color, size) {}

        public HeaderScope(ref Rect rect, SerializedProperty property, string title, EConsoleColor color = EConsoleColor.Primary, EHeaderSize size = EHeaderSize.Primary)
        {
            isExpanded = Begin(ref rect, title, property, color, size);
        }

        public HeaderScope(ref Rect rect, string title, ref bool isExpandedRef, EConsoleColor color = EConsoleColor.Primary, EHeaderSize size = EHeaderSize.Primary)
        {
            isExpanded = Begin(ref rect, title, ref isExpandedRef, color, size);
        }

        protected override void CloseScope()
        {
            if (isExpanded && usesLayout) EditorGUILayout.EndVertical();
        }
    }
}