using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Skyx.SkyxEditor
{
    public class InlineScope : ILayoutScope
    {
        #region Interface

        public static InlineScope Open(SerializedProperty property, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(property, property.PrettyName(), color, size);

        public static InlineScope Open(SerializedProperty property, string title, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
        {
            var scope = pool.Get();

            scope.usesLayout = true;
            scope.IsExpanded = Begin(title, property, color, size);

            return scope;
        }

        public static InlineScope Open(string title, ref bool isExpandedRef, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
        {
            var scope = pool.Get();

            scope.IsExpanded = isExpandedRef;
            scope.usesLayout = true;

            Begin(title, ref isExpandedRef, color, size, null);

            return scope;
        }

        public static InlineScope Open(ref Rect rect, SerializedProperty property, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(ref rect, property, property.PrettyName(), color, size);

        public static InlineScope Open(ref Rect rect, SerializedProperty property, string title, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
        {
            var scope = pool.Get();

            scope.usesLayout = false;
            scope.IsExpanded = Begin(ref rect, title, property, color, size);

            return scope;
        }

        public static InlineScope Open(ref Rect rect, string title, ref bool isExpandedRef, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
        {
            var scope = pool.Get();

            scope.usesLayout = false;
            scope.IsExpanded = Begin(ref rect, title, ref isExpandedRef, color, size, null);

            return scope;
        }

        #endregion

        #region Instance Info

        public bool IsExpanded { get; private set; }
        private bool usesLayout;

        public void Dispose()
        {
            if (IsExpanded && usesLayout) EditorGUILayout.EndVertical();
            pool.Release(this);
        }

        #endregion

        #region Drawers

        private static bool Begin(string title, SerializedProperty property, EColor color, EElementSize size)
        {
            var initialExpanded = property.isExpanded;
            var isExpanded = property.isExpanded;
            Begin(title, ref isExpanded, color, size, property);
            property.isExpanded = isExpanded;

            return initialExpanded;
        }

        private static bool Begin(string title, ref bool isExpandedRef, EColor color, EElementSize size, SerializedProperty property)
        {
            var headerHeight = SkyxStyles.HeaderHeight(size);
            var boxRect = EditorGUILayout.GetControlRect(false, headerHeight);

            var headerRect = boxRect;
            headerRect.height = headerHeight;

            if (isExpandedRef)
            {
                var drawingRect = EditorGUILayout.BeginVertical();
                boxRect.yMax = drawingRect.yMax;
            }

            return ReallyDraw(ref headerRect, ref boxRect, title, ref isExpandedRef, color, size, property);
        }

        private static bool Begin(ref Rect initialRect, string title, SerializedProperty property, EColor color, EElementSize size)
        {
            var isExpanded = property.isExpanded;
            Begin(ref initialRect, title, ref isExpanded, color, size, property);
            property.isExpanded = isExpanded;

            return property.isExpanded;
        }

        private static bool Begin(ref Rect initialRect, string title, ref bool isExpandedRef, EColor color, EElementSize size, SerializedProperty property)
        {
            initialRect.height -= 2f * SkyxStyles.ElementsMargin;

            // Reversing outer box margin
            initialRect.x -= SkyxStyles.BoxMargin - 1;
            initialRect.width += (SkyxStyles.BoxMargin - 1) * 2;

            var headerHeight = SkyxStyles.HeaderHeight(size);
            var headerRect = initialRect;
            headerRect.height = headerHeight;

            var boxRect = initialRect;
            initialRect.ApplyBoxMargin(headerHeight);

            return ReallyDraw(ref headerRect, ref boxRect, title, ref isExpandedRef, color, size, property);
        }

        private static bool ReallyDraw(ref Rect headerRect, ref Rect boxRect, string title, ref bool isExpandedRef, EColor color, EElementSize size, SerializedProperty property)
        {
            var current = Event.current;
            var isHovered = headerRect.Contains(current.mousePosition);

            if (headerRect.TryUseClick(false))
                isExpandedRef = !isExpandedRef;

            if (property != null)
                PropertyContextMenu.ContextGUI(ref headerRect, property);

            using (AllColorsScope.Set(Color.clear))
                GUI.Button(headerRect, GUIContent.none); // This forces repaint on hover

            var headerColor = isHovered || !isExpandedRef ? color : EColor.Backdrop;
            SkyxGUI.Button(headerRect, title, headerColor, size, EButtonType.Plain);

            SkyxGUI.Separator(ref boxRect, 0);
            boxRect.ExtractVertical(headerRect.height, -2);
            SkyxGUI.Separator(ref boxRect, 0, size: 2);

            if (isExpandedRef)
            {
                boxRect.SlideVertically(1, 0);
                SkyxGUI.Separator(ref boxRect, 0, color);
            }

            return isExpandedRef;
        }

        #endregion

        #region Pool

        private static readonly ObjectPool<InlineScope> pool = new(CreateScope);
        private static InlineScope CreateScope() => new();

        #endregion
    }
}