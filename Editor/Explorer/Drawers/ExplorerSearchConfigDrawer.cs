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

            properties.DrawIncluding(ref rect, "name", "sourcesProvider");

            var modeRect = rect;
            modeRect.AdjustToLine();
            modeRect = modeRect.ExtractSmallButton(true);
            modeRect.x -= 3;
            EnumTreeGUI.DrawEnum<EExplorerSearchMode>(modeRect, properties["searchMode"], EColor.Secondary, "How should we deal when multiple child properties are found?");

            HeaderList.Draw(ref rect, properties["filters"], "Filters", EColor.Secondary, EElementSize.SingleLine, EScopeType.Foldout, newText: "Add Filter", canMoveElements: false);

            EnumTreeGUI.DrawEnum<EExplorerSearchMode>(modeRect, properties["searchMode"], EColor.Secondary, "How should we deal when multiple child properties are found?");

            properties.DrawIncluding(ref rect, "batchExecute");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var properties = GetProperties(property);

            return properties.GetTotalHeightIncluding("name", "sourcesProvider", "batchExecute") +
                   HeaderList.GetPropertyHeight(properties["filters"], EScopeType.Foldout, EElementSize.SingleLine);
        }
    }
}