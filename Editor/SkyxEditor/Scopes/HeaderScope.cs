using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Skyx.SkyxEditor
{
    public class HeaderScope : IDisposable
    {
        #region Interface

        public static HeaderScope Open(SerializedProperty property, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(property, property.PrettyName(), color, size);

        public static HeaderScope Open(SerializedProperty property, string title, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
        {
            var scope = pool.Get();

            scope.usesLayout = true;
            scope.isExpanded = Begin(title, property, color, size);

            return scope;
        }

        public static HeaderScope Open(string title, ref bool isExpandedRef, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
        {
            var scope = pool.Get();

            scope.isExpanded = isExpandedRef;
            scope.usesLayout = true;

            Begin(title, ref isExpandedRef, color, size, null);

            return scope;
        }

        public static HeaderScope Open(ref Rect rect, SerializedProperty property, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(ref rect, property, property.PrettyName(), color, size);

        public static HeaderScope Open(ref Rect rect, SerializedProperty property, string title, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
        {
            var scope = pool.Get();

            scope.usesLayout = false;
            scope.isExpanded = Begin(ref rect, title, property, color, size);

            return scope;
        }

        public static HeaderScope Open(ref Rect rect, string title, ref bool isExpandedRef, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
        {
            var scope = pool.Get();

            scope.usesLayout = false;
            scope.isExpanded = Begin(ref rect, title, ref isExpandedRef, color, size, null);

            return scope;
        }

        #endregion

        #region Instance Info

        public bool isExpanded;
        private bool usesLayout;

        public void Dispose()
        {
            if (isExpanded && usesLayout) EditorGUILayout.EndVertical();
            pool.Release(this);
        }

        #endregion

        #region Drawers

        private static bool Begin(string title, SerializedProperty property, EColor color, EElementSize size)
        {
            var initialExpanded = property.isExpanded;
            var isExpanded = property.isExpanded;
            property.isExpanded = Begin(title, ref isExpanded, color, size, property);

            return initialExpanded;
        }

        private static bool Begin(string title, ref bool isExpandedRef, EColor color, EElementSize size, SerializedProperty property)
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

        private static bool Begin(ref Rect initialRect, string title, SerializedProperty property, EColor color, EElementSize size)
        {
            var isExpanded = property.isExpanded;
            property.isExpanded = Begin(ref initialRect, title, ref isExpanded, color, size, property);

            return property.isExpanded;
        }

        private static bool Begin(ref Rect initialRect, string title, ref bool isExpandedRef, EColor color, EElementSize size, SerializedProperty property)
        {
            initialRect.height -= SkyxStyles.ElementsMargin;

            var headerHeight = SkyxStyles.HeaderHeight(size);
            var headerRect = initialRect;
            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var boxRect = initialRect;

            initialRect.ApplyBoxMargin(headerHeight);

            return ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color, size, property);
        }

        private static bool ReallyDraw(Rect headerRect, Rect boxRect, string title, ref bool isExpandedRef, EColor color, EElementSize size, SerializedProperty property)
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

        #endregion

        #region Pool

        private static readonly ObjectPool<HeaderScope> pool = new(CreateScope);
        private static HeaderScope CreateScope() => new();

        #endregion
    }
}