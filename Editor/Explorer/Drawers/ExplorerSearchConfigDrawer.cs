using Skyx.RuntimeEditor;
using Rogue.REditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.Explorer
{
    [CustomPropertyDrawer(typeof(ExplorerSearchConfig<>), true)]
    public class ExplorerSearchConfigDrawer : PropertyEditor
    {
        protected override void Draw(Rect rect, SerializedProperty property, GUIContent label)
        {
            var properties = GetProperties(property);

            properties.DrawIncluding(ref rect, "name");

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.Toggle(rect, "Highlight Editor Properties", EditorPropertyHighlights.IsEnabled);
            if (EditorGUI.EndChangeCheck()) EditorPropertyHighlights.IsEnabled = newValue;

            rect.NextSameLine();
            rect.y += 5;

            properties.DrawIncluding(ref rect,"sourcesProvider");

            var modeRect = rect;
            modeRect.AdjustToLine();
            modeRect = modeRect.ExtractSmallButton(true);
            modeRect.x -= 3;
            EnumTreeGUI.DrawEnum<EExplorerSearchMode>(modeRect, properties["searchMode"], EColor.Support, "How should we deal when multiple child properties are found?");

            HeaderList.Draw(ref rect, properties["filters"], "Filters", EColor.Secondary, EElementSize.SingleLine, EScopeType.Foldout, newText: "Add Filter", canMoveElements: false);

            EnumTreeGUI.DrawEnum<EExplorerSearchMode>(modeRect, properties["searchMode"], EColor.Support, "How should we deal when multiple child properties are found?");

            properties.DrawIncluding(ref rect, "batchExecute");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var properties = GetProperties(property);

            return properties.GetTotalHeightIncluding("name", "sourcesProvider", "batchExecute") +
                   SkyxStyles.FullLineHeight +
                   HeaderList.GetPropertyHeight(properties["filters"], EScopeType.Foldout, EElementSize.SingleLine);
        }
    }
}