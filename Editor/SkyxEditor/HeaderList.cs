using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class HeaderList
    {
        private const float NewElementHeight = SkyxStyles.FullLineHeight + 5; // From separator
        private const float HorizontalThreshold = SkyxStyles.ListControlButtonSize * 3;

        public static void DrawLayout(SerializedProperty property, EConsoleColor color = EConsoleColor.Primary, string title = null, string newText = null)
        {
            var rect = EditorGUILayout.GetControlRect(false, GetPropertyHeight(property));
            Draw(rect, property, color, title);
        }

        public static void Draw(Rect rect, SerializedProperty property, EConsoleColor color = EConsoleColor.Primary, string title = null, string newText = null)
        {
            title = string.IsNullOrEmpty(title) ? property.PrettyName() : title;
            newText = string.IsNullOrEmpty(newText) ? "New Entry" : newText;

            using var scope = new HeaderScope(ref rect, property, title, color);
            if (!scope.isExpanded) return;

            DrawElements(ref rect, property);
            DrawNewElement(rect, property, newText);
        }

        private static void DrawNewElement(Rect rect, SerializedProperty property, string newText)
        {
            rect.height = SkyxStyles.LineHeight;
            var excess = rect.width - 150;
            if (excess > 0)
            {
                rect.width = 150;
                rect.x += excess / 2;
            }

            if (GUI.Button(rect, newText))
                property.InsertArrayElementAtIndex(property.arraySize);
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

            BoxGUI.DrawBox(rect, EConsoleColor.Secondary);
            rect.ApplyMargin(3);

            if (isHorizontal) rect.DivideRect(count);
            else rect.DivideVertically(count);

            if (index > 0 || isHorizontal)
            {
                if (index > 0 && GUI.Button(rect, "▲", SkyxStyles.CenterLabel))
                {
                    property.MoveArrayElement(index, --index);
                    return true;
                }

                if (isHorizontal) rect.SlideSameRect();
                else rect.SlideSameVertically();
            }


            if (GUI.Button(rect, "X", SkyxStyles.CenterLabel.WarningText()))
            {
                property.DeleteArrayElementAtIndex(index);
                return true;
            }

            if (index < property.arraySize - 1)
            {
                if (isHorizontal) rect.SlideSameRect();
                else rect.SlideSameVertically();

                if (GUI.Button(rect, "▼", SkyxStyles.CenterLabel))
                {
                    property.MoveArrayElement(index, ++index);
                    return true;
                }
            }

            return false;
        }

        private static float GetElementsHeight(SerializedProperty property)
        {
            if (!property.isExpanded) return 0;

            var total = (property.arraySize - 1) * SkyxStyles.ListElementsMargin;
            for (int i = 0; i < property.arraySize; i++)
                total += EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i));

            return total;
        }

        public static float GetPropertyHeight(SerializedProperty property)
        {
            if (!property.isExpanded) return SkyxStyles.ClosedScopeHeight();
            return SkyxStyles.ScopeTotalExtraHeight() + GetElementsHeight(property) + NewElementHeight;
        }
    }
}