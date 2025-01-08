using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class HeaderScope : GUI.Scope
    {
        public readonly bool isExpanded;

        private readonly bool usesLayout;

        private static bool Begin(string title, SerializedProperty property, EConsoleColor color)
        {
            var initialExpanded = property.isExpanded;
            var isExpanded = property.isExpanded;
            property.isExpanded = Begin(title, ref isExpanded, color);

            return initialExpanded;
        }

        private static bool Begin(string title, ref bool isExpandedRef, EConsoleColor color)
        {
            var headerRect = EditorGUILayout.GetControlRect(false, SkyxStyles.HeaderButtonSize);
            var boxRect = headerRect;
            ShrinkHeaderRect(ref headerRect);

            if (isExpandedRef)
            {
                var drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            return ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color);
        }

        private static bool Begin(ref Rect initialRect, string title, SerializedProperty property, EConsoleColor color)
        {
            var isExpanded = property.isExpanded;
            property.isExpanded = Begin(ref initialRect, title, ref isExpanded, color);

            return property.isExpanded;
        }

        private static void ShrinkHeaderRect(ref Rect headerRect)
        {
            headerRect.height = SkyxStyles.HeaderButtonSize - 2;
            headerRect.y += 1;
            headerRect.x += 1;
            headerRect.width -= 2;
        }

        private static bool Begin(ref Rect initialRect, string title, ref bool isExpandedRef, EConsoleColor color)
        {
            initialRect.height -= SkyxStyles.ElementsMargin;

            var headerRect = initialRect;
            ShrinkHeaderRect(ref headerRect);

            var boxRect = initialRect;

            initialRect.ApplyBoxMargin(SkyxStyles.HeaderButtonSize);

            return ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color);
        }

        private static bool ReallyDraw(Rect headerRect, Rect boxRect, string title, ref bool isExpandedRef, EConsoleColor color)
        {
            BoxGUI.DrawBox(boxRect, color);

            if (SkyxGUI.PlainBGButton(headerRect, title, SkyxStyles.headerColors.GetLooping(color)))
                isExpandedRef = !isExpandedRef;

            return isExpandedRef;
        }

        public HeaderScope(SerializedProperty property, EConsoleColor color = EConsoleColor.Primary) : this(property, property.PrettyName(), color) {}

        public HeaderScope(SerializedProperty property, string title, EConsoleColor color = EConsoleColor.Primary)
        {
            usesLayout = true;
            isExpanded = Begin(title, property, color);
        }

        public HeaderScope(string title, ref bool isExpandedRef, EConsoleColor color = EConsoleColor.Primary)
        {
            usesLayout = true;
            isExpanded = Begin(title, ref isExpandedRef, color);
        }

        public HeaderScope(ref Rect rect, SerializedProperty property, EConsoleColor color = EConsoleColor.Primary)
            : this(ref rect, property, property.PrettyName(), color) {}

        public HeaderScope(ref Rect rect, SerializedProperty property, string title, EConsoleColor color = EConsoleColor.Primary)
        {
            isExpanded = Begin(ref rect, title, property, color);
        }

        public HeaderScope(ref Rect rect, string title, ref bool isExpandedRef, EConsoleColor color = EConsoleColor.Primary)
        {
            isExpanded = Begin(ref rect, title, ref isExpandedRef, color);
        }

        protected override void CloseScope()
        {
            if (isExpanded && usesLayout) EditorGUILayout.EndVertical();
        }
    }
}