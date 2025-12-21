using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Skyx.SkyxEditor
{
    public class HeaderScope : ILayoutScope
    {
        #region Interface

        public static HeaderScope Open(SerializedProperty property, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(new SkopeInfo(EScopeType.Header, property, color, size));

        public static HeaderScope Open(SerializedProperty property, string title, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(new SkopeInfo(EScopeType.Header, property, title, color, size));

        public static HeaderScope Open(SkopeInfo info)
        {
            var scope = pool.Get();

            scope.usesLayout = true;
            scope.IsExpanded = Begin(info);

            return scope;
        }

        public static HeaderScope Open(string title, ref bool isExpandedRef, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(ref isExpandedRef, new SkopeInfo(EScopeType.Header, null, title, color, size));

        public static HeaderScope Open(ref bool isExpandedRef, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.IsExpanded = isExpandedRef;
            scope.usesLayout = true;

            Begin(ref isExpandedRef, info);

            return scope;
        }

        public static HeaderScope Open(ref Rect rect, SerializedProperty property, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(ref rect, new SkopeInfo(EScopeType.Header, property, color, size));

        public static HeaderScope Open(ref Rect rect, SerializedProperty property, string title, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(ref rect, new SkopeInfo(EScopeType.Header, property, title, color, size));

        public static HeaderScope Open(ref Rect rect, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.usesLayout = false;
            scope.IsExpanded = Begin(ref rect, info);

            return scope;
        }

        public static HeaderScope Open(ref Rect rect, string title, ref bool isExpandedRef, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary)
            => Open(ref rect, ref isExpandedRef, new SkopeInfo(EScopeType.Header, null, title, color, size));

        public static HeaderScope Open(ref Rect rect, ref bool isExpandedRef, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.usesLayout = false;
            scope.IsExpanded = Begin(ref rect, ref isExpandedRef, info);

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
            var initialExpanded = info.property.isExpanded;
            var isExpanded = info.property.isExpanded;

            Begin(ref isExpanded, info);

            info.property.isExpanded = isExpanded;

            return initialExpanded;
        }

        private static bool Begin(ref bool isExpandedRef, SkopeInfo info)
        {
            var headerHeight = SkyxStyles.HeaderHeight(info.size);
            var headerRect = EditorGUILayout.GetControlRect(false, headerHeight);
            var boxRect = headerRect;

            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            if (isExpandedRef)
            {
                var drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            return ReallyDraw(headerRect, boxRect, ref isExpandedRef, info);
        }

        private static bool Begin(ref Rect initialRect, SkopeInfo info)
        {
            var isExpanded = info.property.isExpanded;
            Begin(ref initialRect, ref isExpanded, info);
            info.property.isExpanded = isExpanded;

            return info.property.isExpanded;
        }

        private static bool Begin(ref Rect initialRect, ref bool isExpandedRef, SkopeInfo info)
        {
            initialRect.height -= SkyxStyles.ElementsMargin;

            var headerHeight = SkyxStyles.HeaderHeight(info.size);
            var headerRect = initialRect;
            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var boxRect = initialRect;

            initialRect.ApplyBoxMargin(headerHeight);

            return ReallyDraw(headerRect, boxRect, ref isExpandedRef, info);
        }

        private static bool ReallyDraw(Rect headerRect, Rect boxRect, ref bool isExpandedRef, SkopeInfo info)
        {
            Skope.DrawBox(ref boxRect, info);

            var prop = info.property;
            DrawButtons(headerRect, info, false, ref isExpandedRef);

            if (headerRect.TryUseClick(false))
                isExpandedRef = !isExpandedRef;

            if (prop != null)
                PropertyContextMenu.ContextGUI(ref headerRect, prop);

            GUI.Button(headerRect, GUIContent.none); // This forces repaint on hover
            SkyxGUI.Button(headerRect, info.title, info.color, info.size, EButtonType.Plain);

            DrawButtons(headerRect, info, true, ref isExpandedRef);

            return isExpandedRef;
        }

        private static void DrawButtons(Rect rect, SkopeInfo info, bool reallyDraw, ref bool isExpandedRef)
        {
            if (info.buttons == null) return;

            rect.y += 5;
            rect.x -= 4;
            rect.height = SkyxStyles.LineHeight;

            foreach (var (label, color, action) in info.buttons)
            {
                if (reallyDraw) SkyxGUI.MiniButton(ref rect, label, color, null, true);
                else
                {
                    var buttonRect = rect.ExtractMiniButton(true);
                    if (buttonRect.TryUseClick(false))
                    {
                        action();
                        isExpandedRef = true;
                    }
                }
            }
        }

        #endregion

        #region Pool

        private static readonly ObjectPool<HeaderScope> pool = new(CreateScope);
        private static HeaderScope CreateScope() => new();

        #endregion
    }
}