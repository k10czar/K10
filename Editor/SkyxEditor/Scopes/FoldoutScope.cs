using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Skyx.SkyxEditor
{
    public class FoldoutScope : ILayoutScope
    {
        #region Interface

        public static FoldoutScope Open(SerializedProperty property, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
            => Open(new SkopeInfo(EScopeType.Foldout, property, color, size, indent));

        public static FoldoutScope Open(SerializedProperty property, string title, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
            => Open(new SkopeInfo(EScopeType.Foldout, property, title, color, size, indent));

        public static FoldoutScope Open(SkopeInfo info)
        {
            var scope = pool.Get();

            scope.indent = info.indent;
            scope.usesLayout = true;
            scope.IsExpanded = scope.BeginWrapper(info);

            return scope;
        }

        public static FoldoutScope Open(string title, ref bool isExpandedRef, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
            => Open(ref isExpandedRef, new SkopeInfo(EScopeType.Foldout, null, title, color, size, indent));

        public static FoldoutScope Open(ref bool isExpandedRef, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.indent = info.indent;
            scope.usesLayout = true;
            scope.IsExpanded = scope.GetDrawingRects(ref isExpandedRef, info);

            return scope;
        }

        public static FoldoutScope Open(ref Rect rect, SerializedProperty property, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
            => Open(ref rect, new SkopeInfo(EScopeType.Foldout, property, color, size, indent));

        public static FoldoutScope Open(ref Rect rect, SerializedProperty property, string title, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
            => Open(ref rect, new SkopeInfo(EScopeType.Foldout, property, title, color, size, indent));

        public static FoldoutScope Open(ref Rect rect, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.indent = info.indent;
            scope.usesLayout = false;
            scope.IsExpanded = scope.BeginWrapper(ref rect, info);

            return scope;
        }

        public static FoldoutScope Open(ref Rect rect, string title, ref bool isExpandedRef, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
            => Open(ref rect, ref isExpandedRef, new SkopeInfo(EScopeType.Foldout, null, title, color, size, indent));

        public static FoldoutScope Open(ref Rect rect, ref bool isExpandedRef, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.indent = info.indent;
            scope.usesLayout = false;
            scope.IsExpanded = scope.AdjustAvailableRect(ref rect, ref isExpandedRef, info);

            return scope;
        }

        #endregion

        #region Instance Info

        public bool IsExpanded { get; private set; }
        private bool usesLayout;
        private bool indent;

        public void Dispose()
        {
            if (IsExpanded)
            {
                if (usesLayout) EditorGUILayout.EndVertical();
                if (indent) EditorGUI.indentLevel--;
            }
            pool.Release(this);
        }

        #endregion

        #region Drawers

        private bool ReallyDraw(Rect headerRect, Rect boxRect, ref bool isExpandedRef, SkopeInfo info)
        {
            Skope.DrawBox(ref boxRect, info);

            var canExpand = info.CanExpand();
            if (!canExpand) isExpandedRef = false;

            var drawingRect = headerRect;
            drawingRect.ApplyStartMargin(10);

            if (info.buttons != null)
            {
                var buttonsRect = headerRect;
                buttonsRect.y += 2;
                buttonsRect.x -= 4;
                buttonsRect.height = SkyxStyles.LineHeight;

                foreach (var (label, color, action) in info.buttons)
                {
                    if (SkyxGUI.MiniButton(ref buttonsRect, label, color, null, true))
                    {
                        action();
                        isExpandedRef = true;
                    }
                }
            }

            if (canExpand && headerRect.TryUseClick(false))
                isExpandedRef = !isExpandedRef;

            if (info.property != null)
                PropertyContextMenu.ContextGUI(ref headerRect, info.property);

            var toggleRect = drawingRect.ExtractMiniButton();
            if (canExpand) GUI.Toggle(toggleRect, isExpandedRef, GUIContent.none, EditorStyles.foldout);

            EditorGUI.LabelField(drawingRect, info.title, SkyxStyles.DefaultLabel);

            if (isExpandedRef)
            {
                var separator = new Rect(boxRect.x + 4, boxRect.y + drawingRect.height, boxRect.width - 8, 1);
                SkyxGUI.Separator(ref separator);

                if (indent) EditorGUI.indentLevel++;
            }

            return isExpandedRef;
        }

        private bool GetDrawingRects(ref bool isExpandedRef, SkopeInfo info)
        {
            var headerHeight = SkyxStyles.HeaderHeight(info.size);
            var headerRect = EditorGUILayout.GetControlRect(false, headerHeight);
            var boxRect = headerRect;

            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var initialExpanded = isExpandedRef;

            if (isExpandedRef)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(SkyxStyles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            ReallyDraw(headerRect, boxRect, ref isExpandedRef, info);

            return initialExpanded;
        }

        private bool AdjustAvailableRect(ref Rect initialRect, ref bool isExpandedRef, SkopeInfo info)
        {
            initialRect.height -= SkyxStyles.ElementsMargin;

            var headerHeight = SkyxStyles.HeaderHeight(info.size);
            var headerRect = initialRect;
            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var boxRect = initialRect;

            initialRect.ApplyBoxMargin(headerHeight);

            return ReallyDraw(headerRect, boxRect, ref isExpandedRef, info);
        }

        private bool BeginWrapper(SkopeInfo info)
        {
            var isExpandedRef = info.property.isExpanded;
            GetDrawingRects(ref isExpandedRef, info);
            info.property.isExpanded = isExpandedRef;

            return isExpandedRef;
        }

        private bool BeginWrapper(ref Rect initialRect, SkopeInfo info)
        {
            var isExpandedRef = info.property.isExpanded;
            info.property.isExpanded = AdjustAvailableRect(ref initialRect, ref isExpandedRef, info);

            return info.property.isExpanded;
        }

        #endregion

        #region Pool

        private static readonly ObjectPool<FoldoutScope> pool = new(CreateScope);
        private static FoldoutScope CreateScope() => new();

        #endregion
    }
}