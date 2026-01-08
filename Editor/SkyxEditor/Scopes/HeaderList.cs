using System;
using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class HeaderList
    {
        private const float ExtraElementHeight = SkyxStyles.ElementsMargin + 3; // from separator
        private const float NewElementHeight = SkyxStyles.FullLineHeight;
        private const float HorizontalThreshold = SkyxStyles.ListControlButtonSize * 3;

        public static void DrawLayout(SerializedProperty property, string title = null, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary, EScopeType scopeType = EScopeType.Header, string newText = null, Action<SerializedProperty> onNewElement = null, bool canMoveElements = true)
        {
            var rect = EditorGUILayout.GetControlRect(false, GetPropertyHeight(property, scopeType, size));
            Draw(ref rect, property, title, color, size, scopeType, newText, onNewElement, canMoveElements, false);
        }

        public static void Draw(ref Rect rect, SerializedProperty property, string title = null, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary, EScopeType scopeType = EScopeType.Header, string newText = null, Action<SerializedProperty> onNewElement = null, bool canMoveElements = true, bool resetHeight = true)
        {
            var drawingRect = rect;
            if (resetHeight) drawingRect.height = GetPropertyHeight(property, scopeType, size);
            rect.y += drawingRect.height + SkyxStyles.ElementsMargin;

            title = string.IsNullOrEmpty(title) ? property.PrettyName() : title;
            newText = string.IsNullOrEmpty(newText) ? "New Entry" : newText;

            using var scope = Skope.Open(scopeType, ref drawingRect, property, title, color, size);
            if (!scope.IsExpanded) return;

            DrawElements(ref drawingRect, property, canMoveElements);
            DrawNewElement(drawingRect, property, newText, onNewElement);
        }

        public static void DrawHeaderlessLayout(SerializedProperty property, string newText = null, Action<SerializedProperty> onNewElement = null, bool canMoveElements = true)
        {
            var rect = EditorGUILayout.GetControlRect(false, GetPropertyHeight(property, EScopeType.Inline, EElementSize.Primary));
            DrawHeaderless(ref rect, property, newText, onNewElement, canMoveElements, false);
        }

        public static void DrawHeaderless(ref Rect rect, SerializedProperty property, string newText = null, Action<SerializedProperty> onNewElement = null, bool canMoveElements = true, bool resetHeight = true)
        {
            if (resetHeight) rect.height = GetPropertyHeight(property, EScopeType.Inline, EElementSize.Primary);
            newText = string.IsNullOrEmpty(newText) ? "New Entry" : newText;

            DrawElements(ref rect, property, canMoveElements);
            DrawNewElement(rect, property, newText, onNewElement);

            rect.y += SkyxStyles.FullLineHeight + SkyxStyles.ElementsMargin;
        }

        private static void DrawNewElement(Rect rect, SerializedProperty property, string newText, Action<SerializedProperty> onNewElement)
        {
            rect.height = SkyxStyles.LineHeight;

            var excess = rect.width - 150;
            if (excess > 0)
            {
                rect.width = 150;
                rect.x += excess / 2;
            }

            if (GUI.Button(rect, newText))
            {
                var index = property.arraySize;
                property.InsertArrayElementAtIndex(index);
                property.Apply($"New array element: {property.propertyPath}");

                var newElement = property.GetArrayElementAtIndex(index);
                newElement.ResetDefaultValues(onNewElement, false, true);
            }
        }

        private static void DrawElements(ref Rect rect, SerializedProperty property, bool canMoveElements)
        {
            for (int i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);

                var elementRect = rect;
                elementRect.height = EditorGUI.GetPropertyHeight(element, true);

                var isHorizontalControl = canMoveElements && elementRect.height < HorizontalThreshold;
                var controlSize = (isHorizontalControl ? GetControlButtonCount(property, i, true) : 1) * SkyxStyles.ListControlButtonSize;
                var buttonsRect = elementRect.ExtractRect(controlSize);

                if (DrawElementControlButtons(buttonsRect, property, i, isHorizontalControl, canMoveElements)) return;

                EditorGUI.PropertyField(elementRect, element, GUIContent.none);

                rect.y += elementRect.height + 2;
                var separator = rect;
                separator.height = 1;
                EditorGUI.DrawRect(separator, Colors.Transparent02);

                rect.y += SkyxStyles.ElementsMargin;
            }
        }

        private static int GetControlButtonCount(SerializedProperty property, int index, bool isHorizontal)
        {
            if (isHorizontal) return 3;
            return 1 + (index > 0 ? 1 : 0) + (index < property.arraySize - 1 ? 1 : 0);
        }

        private static bool DrawElementControlButtons(Rect rect, SerializedProperty property, int index, bool isHorizontal, bool canMoveElements)
        {
            var count = canMoveElements ? GetControlButtonCount(property, index, isHorizontal) : 1;

            rect.height = (isHorizontal ? 1 : count) * SkyxStyles.ListControlButtonSize;

            BoxGUI.DrawBox(ref rect, EColor.Secondary);
            rect.ApplyMargin(3, true, true);

            if (isHorizontal) rect.DivideRect(count);
            else rect.DivideVertically(count);

            if (canMoveElements && (index > 0 || isHorizontal))
            {
                if (index > 0 && GUI.Button(rect, "▲", SkyxStyles.CenterLabel))
                {
                    property.MoveArrayElement(index, --index);
                    property.Apply();
                    return true;
                }

                if (isHorizontal) rect.SlideSame();
                else rect.SlideSameVertically();
            }

            if (GUI.Button(rect, "X", SkyxStyles.CenterLabel.With(EColor.Warning)))
            {
                property.DeleteArrayElementAtIndex(index);
                property.Apply();
                return true;
            }

            if (canMoveElements && (index < property.arraySize - 1))
            {
                if (isHorizontal) rect.SlideSame();
                else rect.SlideSameVertically();

                if (GUI.Button(rect, "▼", SkyxStyles.CenterLabel))
                {
                    property.MoveArrayElement(index, ++index);
                    property.Apply();
                    return true;
                }
            }

            return false;
        }

        private static float GetElementsHeight(SerializedProperty property)
        {
            var total = property.arraySize * ExtraElementHeight;
            for (int i = 0; i < property.arraySize; i++)
                total += EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i), true);

            return total;
        }

        public static float GetPropertyHeight(SerializedProperty property, EScopeType scopeType, EElementSize size)
        {
            if (scopeType is EScopeType.Inline)
                return GetElementsHeight(property) + NewElementHeight;

            var height = Skope.ScopeHeight(scopeType, size, property.isExpanded);
            if (property.isExpanded) height += GetElementsHeight(property) + NewElementHeight;

            return height;
        }
    }
}