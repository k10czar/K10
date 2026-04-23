using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Rogue.REditor
{
    public class InlineHeaderScope : ILayoutScope
    {
        #region Interface

        public static InlineHeaderScope Open(SerializedProperty property,  EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(new SkopeInfo(EScopeType.InlineHeader, property, color, size));

        public static InlineHeaderScope Open(SerializedProperty property, string title,  EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(new SkopeInfo(EScopeType.InlineHeader, property, title, color, size));

        public static InlineHeaderScope Open(SkopeInfo info)
        {
            var scope = pool.Get();

            scope.usesLayout = true;
            scope.IsExpanded = Begin(info);

            return scope;
        }

        public static InlineHeaderScope Open(string title, ref bool isExpandedRef,  EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(ref isExpandedRef, new SkopeInfo(EScopeType.InlineHeader, null, title, color, size));

        public static InlineHeaderScope Open(ref bool isExpandedRef, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.IsExpanded = isExpandedRef;
            scope.usesLayout = true;

            Begin(ref isExpandedRef, info);

            return scope;
        }

        public static InlineHeaderScope Open(ref Rect rect, SerializedProperty property,  EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(ref rect, new SkopeInfo(EScopeType.InlineHeader, property, color, size));

        public static InlineHeaderScope Open(ref Rect rect, SerializedProperty property, string title,  EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(ref rect, new SkopeInfo(EScopeType.InlineHeader, property, title, color, size));

        public static InlineHeaderScope Open(ref Rect rect, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.usesLayout = false;
            scope.IsExpanded = Begin(ref rect, info);

            return scope;
        }

        public static InlineHeaderScope Open(ref Rect rect, string title, ref bool isExpandedRef,  EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(ref rect, ref isExpandedRef, new SkopeInfo(EScopeType.InlineHeader, null, title, color, size));

        public static InlineHeaderScope Open(ref Rect rect, ref bool isExpandedRef, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.usesLayout = false;
            scope.IsExpanded = isExpandedRef;

            Begin(ref rect, ref isExpandedRef, info);

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

        private static bool Begin(SkopeInfo info)
        {
            var isExpanded = info.property.isExpanded;
            var initialExpanded = isExpanded;

            Begin(ref isExpanded, info);
            info.property.isExpanded = isExpanded;

            return initialExpanded;
        }

        private static bool Begin(ref bool isExpandedRef, SkopeInfo info)
        {
            var headerHeight = Skope.HeaderHeight(info.size);
            var boxRect = EditorGUILayout.GetControlRect(false, headerHeight);

            var headerRect = boxRect;
            headerRect.height = headerHeight;

            if (isExpandedRef)
            {
                var drawingRect = EditorGUILayout.BeginVertical();
                boxRect.yMax = drawingRect.yMax;
            }

            return ReallyDraw(ref headerRect, ref boxRect, ref isExpandedRef, info);
        }

        private static bool Begin(ref Rect initialRect, SkopeInfo info)
        {
            var isExpanded = info.property.isExpanded;
            var initialExpanded = isExpanded;

            Begin(ref initialRect, ref isExpanded, info);
            info.property.isExpanded = isExpanded;

            return initialExpanded;
        }

        private static bool Begin(ref Rect initialRect, ref bool isExpandedRef, SkopeInfo info)
        {
            if (isExpandedRef) initialRect.height -= SkyxStyles.ElementsMargin;

            // Reversing outer box margin
            initialRect.x -= SkyxStyles.BoxMargin - 1;
            initialRect.width += (SkyxStyles.BoxMargin - 1) * 2;

            var headerHeight = Skope.HeaderHeight(info.size);
            var headerRect = initialRect;
            headerRect.height = headerHeight;

            var boxRect = initialRect;
            initialRect.ApplyBoxMargin(headerHeight);

            return ReallyDraw(ref headerRect, ref boxRect, ref isExpandedRef, info);
        }

        private static bool ReallyDraw(ref Rect headerRect, ref Rect boxRect, ref bool isExpandedRef, SkopeInfo info)
        {
            var current = Event.current;
            var isHovered = headerRect.Contains(current.mousePosition);

            if (headerRect.TryUseClick(false))
                isExpandedRef = !isExpandedRef;

            if (info.property != null)
                PropertyContextMenu.ContextGUI(ref headerRect, info.property);

            using (AllColorsScope.Set(Color.clear))
                GUI.Button(headerRect, GUIContent.none); // This forces repaint on hover

            var headerColor = isHovered || !isExpandedRef ? info.color : EColor.Backdrop;
            SkyxGUI.Button(headerRect, info.title, headerColor, info.size, EButtonType.Plain);

            SkyxGUI.Separator(ref boxRect, 0);
            boxRect.ExtractVertical(headerRect.height, -2);
            SkyxGUI.Separator(ref boxRect, 0, size: 2);

            if (isExpandedRef)
            {
                boxRect.SlideVertically(1, -2);
                SkyxGUI.Separator(ref boxRect, 0, info.color);
            }

            return isExpandedRef;
        }

        #endregion

        #region Pool

        private static readonly ObjectPool<InlineHeaderScope> pool = new(CreateScope);
        private static InlineHeaderScope CreateScope() => new();

        #endregion
    }
}