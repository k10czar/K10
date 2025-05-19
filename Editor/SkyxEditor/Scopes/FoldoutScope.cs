﻿using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Skyx.SkyxEditor
{
    public class FoldoutScope : IDisposable
    {
        #region Interface

        public static FoldoutScope Open(SerializedProperty property, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
            => Open(property, ObjectNames.NicifyVariableName(property.name), color, size, indent);

        public static FoldoutScope Open(SerializedProperty property, string title, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
        {
            var scope = pool.Get();

            scope.indent = indent;
            scope.usesLayout = true;
            scope.isExpanded = scope.BeginWrapper(title, property, color, size);

            return scope;
        }

        public static FoldoutScope Open(string title, ref bool isExpandedRef, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
        {
            var scope = pool.Get();

            scope.indent = indent;
            scope.usesLayout = true;
            scope.isExpanded = scope.GetDrawingRects(title, ref isExpandedRef, color, size, null);

            return scope;
        }

        public static FoldoutScope Open(ref Rect rect, string title, ref bool isExpandedRef, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
        {
            var scope = pool.Get();

            scope.indent = indent;
            scope.usesLayout = false;
            scope.isExpanded = scope.AdjustAvailableRect(ref rect, title, ref isExpandedRef, color, size, null);

            return scope;
        }

        public static FoldoutScope Open(ref Rect rect, SerializedProperty property, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
            => Open(ref rect, property, property.PrettyName(), color, size, indent);

        public static FoldoutScope Open(ref Rect rect, SerializedProperty property, string title, EColor color = EColor.Secondary, EElementSize size = EElementSize.SingleLine, bool indent = false)
        {
            var scope = pool.Get();

            scope.indent = indent;
            scope.usesLayout = false;
            scope.isExpanded = scope.BeginWrapper(ref rect, title, property, color, size);

            return scope;
        }

        #endregion

        #region Instance Info

        public bool isExpanded;
        private bool usesLayout;
        private bool indent;

        public void Dispose()
        {
            if (isExpanded)
            {
                if (usesLayout) EditorGUILayout.EndVertical();
                if (indent) EditorGUI.indentLevel--;
            }
            pool.Release(this);
        }

        #endregion

        #region Drawers

        private bool ReallyDraw(Rect headerRect, Rect boxRect, string title, ref bool isExpandedRef, EColor color, EElementSize size, SerializedProperty property)
        {
            BoxGUI.DrawBox(boxRect, color);

            var drawingRect = headerRect;
            drawingRect.ApplyStartMargin(10);
            GUI.Toggle(drawingRect.ExtractMiniButton(), isExpandedRef, GUIContent.none, EditorStyles.foldout);
            EditorGUI.LabelField(drawingRect, title, SkyxStyles.DefaultLabel);

            if (isExpandedRef)
            {
                var separator = new Rect(boxRect.x + 4, boxRect.y + drawingRect.height, boxRect.width - 8, 1);
                SkyxGUI.Separator(ref separator);

                if (indent) EditorGUI.indentLevel++;
            }

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

        private bool GetDrawingRects(string title, ref bool isExpandedRef, EColor color, EElementSize size, SerializedProperty property)
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

        private bool AdjustAvailableRect(ref Rect initialRect, string title, ref bool isExpandedRef, EColor color, EElementSize size, SerializedProperty property)
        {
            initialRect.height -= SkyxStyles.ElementsMargin;

            var headerHeight = SkyxStyles.HeaderHeight(size);
            var headerRect = initialRect;
            BoxGUI.ShrinkHeaderRect(ref headerRect, headerHeight);

            var boxRect = initialRect;

            initialRect.ApplyBoxMargin(headerHeight);

            return ReallyDraw(headerRect, boxRect, title, ref isExpandedRef, color, size, property);
        }

        private bool BeginWrapper(string title, SerializedProperty property, EColor color, EElementSize size)
        {
            var isExpandedRef = property.isExpanded;
            property.isExpanded = GetDrawingRects(title, ref isExpandedRef, color, size, property);

            return isExpandedRef;
        }

        private bool BeginWrapper(ref Rect initialRect, string title, SerializedProperty property, EColor color, EElementSize size)
        {
            var isExpandedRef = property.isExpanded;
            property.isExpanded = AdjustAvailableRect(ref initialRect, title, ref isExpandedRef, color, size, property);

            return property.isExpanded;
        }

        #endregion

        #region Pool

        private static readonly ObjectPool<FoldoutScope> pool = new(CreateScope);
        private static FoldoutScope CreateScope() => new();

        #endregion
    }
}