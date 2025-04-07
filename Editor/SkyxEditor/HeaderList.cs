using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class HeaderList
    {
        public delegate void OnNewElementCallback(SerializedProperty element);

        private const float ExtraElementHeight = SkyxStyles.ElementsMargin + 3; // from separator
        private const float NewElementHeight = SkyxStyles.FullLineHeight;
        public const float HorizontalThreshold = SkyxStyles.ListControlButtonSize * 3;

        public static void DrawLayout(SerializedProperty property, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary, string title = null, string newText = null, OnNewElementCallback onNewElement = null)
        {
            var rect = EditorGUILayout.GetControlRect(false, GetPropertyHeight(property, false, size));
            Draw(ref rect, property, color, size, title, newText, onNewElement, false);
        }

        public static void Draw(ref Rect rect, SerializedProperty property, EColor color = EColor.Primary, EElementSize size = EElementSize.Primary, string title = null, string newText = null, OnNewElementCallback onNewElement = null, bool resetHeight = true)
        {
            if (resetHeight) rect.height = GetPropertyHeight(property, true);

            title = string.IsNullOrEmpty(title) ? property.PrettyName() : title;
            newText = string.IsNullOrEmpty(newText) ? "New Entry" : newText;

            using var scope = new HeaderScope(ref rect, property, title, color, size);
            if (!scope.isExpanded) return;

            DrawElements(ref rect, property);
            DrawNewElement(rect, property, newText, onNewElement);
        }

        public static void DrawHeaderlessLayout(SerializedProperty property, string newText = null, OnNewElementCallback onNewElement = null)
        {
            var rect = EditorGUILayout.GetControlRect(false, GetPropertyHeight(property, true));
            DrawHeaderless(ref rect, property, newText, onNewElement, false);
        }

        public static void DrawHeaderless(ref Rect rect, SerializedProperty property, string newText = null, OnNewElementCallback onNewElement = null, bool resetHeight = true)
        {
            if (resetHeight) rect.height = GetPropertyHeight(property, true);
            newText = string.IsNullOrEmpty(newText) ? "New Entry" : newText;

            DrawElements(ref rect, property);
            DrawNewElement(rect, property, newText, onNewElement);
        }

        private static void DrawNewElement(Rect rect, SerializedProperty property, string newText, OnNewElementCallback onNewElement)
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
                onNewElement?.Invoke(property.GetArrayElementAtIndex(index));
                property.Apply($"New array element: {property.propertyPath}");
            }
        }

        private static void DrawElements(ref Rect rect, SerializedProperty property)
        {
            for (int i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);

                var elementRect = rect;
                elementRect.height = EditorGUI.GetPropertyHeight(element);

                var isHorizontalControl = elementRect.height < HorizontalThreshold;
                var controlSize = (isHorizontalControl ? GetControlButtonCount(property, i, true) : 1) * SkyxStyles.ListControlButtonSize;
                var buttonsRect = elementRect.ExtractRect(controlSize);

                if (DrawElementControlButtons(buttonsRect, property, i, isHorizontalControl)) return;

                EditorGUI.PropertyField(elementRect, element);

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

        private static bool DrawElementControlButtons(Rect rect, SerializedProperty property, int index, bool isHorizontal)
        {
            var count = GetControlButtonCount(property, index, isHorizontal);

            rect.height = (isHorizontal ? 1 : count) * SkyxStyles.ListControlButtonSize;

            BoxGUI.DrawBox(rect, EColor.Secondary);
            rect.ApplyMargin(3);

            if (isHorizontal) rect.DivideRect(count);
            else rect.DivideVertically(count);

            if (index > 0 || isHorizontal)
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

            if (index < property.arraySize - 1)
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
                total += EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i));

            return total;
        }

        public static float GetPropertyHeight(SerializedProperty property, bool isHeaderless, EElementSize size = EElementSize.Primary)
        {
            if (!isHeaderless && !property.isExpanded) return SkyxStyles.ClosedScopeHeight(size);

            return (isHeaderless ? SkyxStyles.ElementsMargin : SkyxStyles.ScopeTotalExtraHeight(size)) +
                    GetElementsHeight(property) +
                    NewElementHeight;
        }
    }
}