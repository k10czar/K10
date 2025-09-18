using System;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class HeaderList
    {
        private const float ExtraElementHeight = SkyxStyles.ElementsMargin + 3; // from separator
        private const float NewElementHeight = SkyxStyles.FullLineHeight + SkyxStyles.ElementsMargin;
        private const float HorizontalThreshold = SkyxStyles.ListControlButtonSize * 3;

        public static void DrawLayout(SerializedProperty property, string title = null, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary, string newText = null, Action<SerializedProperty> onNewElement = null)
        {
            var rect = EditorGUILayout.GetControlRect(false, GetPropertyHeight(property, false, size));
            Draw(ref rect, property, title, color, size, newText, onNewElement, false);
        }

        public static void Draw(ref Rect rect, SerializedProperty property, string title = null, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary, string newText = null, Action<SerializedProperty> onNewElement = null, bool resetHeight = true)
        {
            if (resetHeight) rect.height = GetPropertyHeight(property, true);

            title = string.IsNullOrEmpty(title) ? property.PrettyName() : title;
            newText = string.IsNullOrEmpty(newText) ? "New Entry" : newText;

            using var scope = HeaderScope.Open(ref rect, property, title, color, size);
            if (!scope.isExpanded) return;

            DrawElements(ref rect, property, true);
            DrawNewElement(rect, property, newText, onNewElement);
        }

        public static void DrawHeaderlessLayout(SerializedProperty property, string newText = null, Action<SerializedProperty> onNewElement = null, bool canMoveElements = true)
        {
            var rect = EditorGUILayout.GetControlRect(false, GetPropertyHeight(property, true));
            DrawHeaderless(ref rect, property, newText, onNewElement, canMoveElements, false);
        }

        public static void DrawHeaderless(ref Rect rect, SerializedProperty property, string newText = null, Action<SerializedProperty> onNewElement = null, bool canMoveElements = true, bool resetHeight = true)
        {
            if (resetHeight) rect.height = GetPropertyHeight(property, true);
            newText = string.IsNullOrEmpty(newText) ? "New Entry" : newText;

            DrawElements(ref rect, property, canMoveElements);
            DrawNewElement(rect, property, newText, onNewElement);

            rect.y += SkyxStyles.FullLineHeight + 2;
        }

        private static void DrawNewElement(Rect rect, SerializedProperty property, string newText, Action<SerializedProperty> onNewElement)
        {
            rect.y += 2;
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
                newElement.ResetDefaultValues(onNewElement, false);
            }
        }

        private static void DrawElements(ref Rect rect, SerializedProperty property, bool canMoveElements)
        {
            for (int i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);

                var elementRect = rect;
                elementRect.height = SerializedRefLib.GetPropertyHeight(element, true);

                var isHorizontalControl = canMoveElements && elementRect.height < HorizontalThreshold;
                var controlSize = (isHorizontalControl ? GetControlButtonCount(property, i, true) : 1) * SkyxStyles.ListControlButtonSize;
                var buttonsRect = elementRect.ExtractRect(controlSize);

                if (DrawElementControlButtons(buttonsRect, property, i, isHorizontalControl, canMoveElements)) return;

                SerializedRefLib.DrawDefaultInspector(elementRect, element, true);

                rect.y += elementRect.height + 2;
                var separator = rect;
                separator.height = 1;
                EditorGUI.DrawRect(separator, SkyxStyles.defaultSeparatorColor);

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

            BoxGUI.DrawBox(rect, EColor.Secondary);
            rect.ApplyMargin(3);

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

                if (isHorizontal) rect.SlideSameRect();
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
                if (isHorizontal) rect.SlideSameRect();
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
                total += SerializedRefLib.GetPropertyHeight(property.GetArrayElementAtIndex(i), true);

            return total;
        }

        public static float GetPropertyHeight(SerializedProperty property, bool isHeaderless, EElementSize size = EElementSize.Primary)
        {
            if (!isHeaderless && !property.isExpanded) return SkyxStyles.ClosedScopeHeight(size);

            return (isHeaderless ? 0 : SkyxStyles.ScopeTotalExtraHeight(size)) +
                    GetElementsHeight(property) +
                    NewElementHeight;
        }
    }
}