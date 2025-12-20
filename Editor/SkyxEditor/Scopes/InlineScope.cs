using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Skyx.SkyxEditor
{
    public class InlineScope : ILayoutScope
    {
        #region Interface

        public static InlineScope Open(SerializedProperty property, EColor color = EColor.Clear, bool indent = false)
            => Open(new SkopeInfo(EScopeType.Inline, property, color, EElementSize.SingleLine, indent));

        public static InlineScope Open(SerializedProperty property, string title, EColor color = EColor.Clear, bool indent = false)
            => Open(new SkopeInfo(EScopeType.Inline, property, title, color, EElementSize.SingleLine, indent));

        public static InlineScope Open(SkopeInfo info)
        {
            var scope = pool.Get();

            scope.indent = info.indent;
            scope.usesLayout = true;
            scope.IsExpanded = scope.BeginWrapper(info);

            return scope;
        }

        public static InlineScope Open(string title, ref bool isExpandedRef, EColor color = EColor.Secondary, bool indent = false)
            => Open(ref isExpandedRef, new SkopeInfo(EScopeType.Inline, null, title, color, EElementSize.SingleLine, indent));

        public static InlineScope Open(ref bool isExpandedRef, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.indent = info.indent;
            scope.usesLayout = true;
            scope.IsExpanded = scope.GetDrawingRects(ref isExpandedRef, info);

            return scope;
        }

        public static InlineScope Open(ref Rect rect, SerializedProperty property, EColor color = EColor.Secondary, bool indent = false)
            => Open(ref rect, new SkopeInfo(EScopeType.Inline, property, color, EElementSize.SingleLine, indent));

        public static InlineScope Open(ref Rect rect, SerializedProperty property, string title, EColor color = EColor.Secondary, bool indent = false)
            => Open(ref rect, new SkopeInfo(EScopeType.Inline, property, title, color, EElementSize.SingleLine, indent));

        public static InlineScope Open(ref Rect rect, SkopeInfo info)
        {
            var scope = pool.Get();

            scope.indent = info.indent;
            scope.usesLayout = false;
            scope.IsExpanded = scope.BeginWrapper(ref rect, info);

            return scope;
        }

        public static InlineScope Open(ref Rect rect, string title, ref bool isExpandedRef, EColor color = EColor.Secondary, bool indent = false)
            => Open(ref rect, ref isExpandedRef, new SkopeInfo(EScopeType.Inline, null, title, color, EElementSize.SingleLine, indent));

        public static InlineScope Open(ref Rect rect, ref bool isExpandedRef, SkopeInfo info)
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

            var canExpand = info.property.CanExpand();
            if (!canExpand) isExpandedRef = false;

            if (info.buttons != null)
            {
                foreach (var (label, color, action) in info.buttons)
                {
                    if (SkyxGUI.MiniButton(ref headerRect, label, color, null, true))
                        action();
                }
            }

            var extractedSize = 0;
            var clickRect = headerRect;

            if (canExpand)
            {
                extractedSize = 15;
                var toggleRect = SkyxGUI.ExtractRect(ref headerRect, extractedSize, false, 0);
                GUI.Toggle(toggleRect, isExpandedRef, GUIContent.none, EditorStyles.foldout);
            }

            var prop = info.property;

            if (prop == null || !prop.IsManagedRef())
                EditorGUI.LabelField(headerRect, info.title, SkyxStyles.DefaultLabel);
            else
            {
                var labelRect = SkyxGUI.ExtractRect(ref headerRect, EditorGUIUtility.labelWidth - extractedSize, false, 0);
                EditorGUI.LabelField(labelRect, prop.displayName, SkyxStyles.DefaultLabel);

                var (text, color) = prop.managedReferenceValue == null
                    ? ("MISSING REFERENCE!", EColor.Danger)
                    : (info.title, EColor.Support);

                if (SkyxGUI.Button(headerRect, text, color))
                    SerializedRefLib.DrawTypePickerMenu(headerRect, prop);
            }

            if (canExpand && clickRect.TryUseClick(false))
                isExpandedRef = !isExpandedRef;

            if (info.property != null)
                PropertyContextMenu.ContextGUI(ref clickRect, info.property);

            if (isExpandedRef && indent) EditorGUI.indentLevel++;

            return isExpandedRef;
        }

        private bool GetDrawingRects(ref bool isExpandedRef, SkopeInfo info)
        {
            var headerHeight = Skope.HeaderHeight(info.size);
            var headerRect = EditorGUILayout.GetControlRect(false, headerHeight);
            var boxRect = headerRect;

            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var initialExpanded = isExpandedRef;

            if (isExpandedRef)
            {
                var drawingRect = EditorGUILayout.BeginVertical();
                boxRect.yMax = drawingRect.yMax;
            }

            ReallyDraw(headerRect, boxRect, ref isExpandedRef, info);

            return initialExpanded;
        }

        private bool AdjustAvailableRect(ref Rect initialRect, ref bool isExpandedRef, SkopeInfo info)
        {
            initialRect.height -= SkyxStyles.ElementsMargin;

            var headerHeight = Skope.HeaderHeight(info.size);
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

        private static readonly ObjectPool<InlineScope> pool = new(CreateScope);
        private static InlineScope CreateScope() => new();

        #endregion
    }
}