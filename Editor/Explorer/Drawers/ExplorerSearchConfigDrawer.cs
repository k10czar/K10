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
            HeaderList.Draw(ref rect, properties["filters"], "Filters", size: EElementSize.SingleLine, newText: "Add Filter", canMoveElements: false);
            properties.DrawIncluding(ref rect, "batchExecute");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var properties = GetProperties(property);

            return properties.GetTotalHeightIncluding("name", "sourcesProvider", "batchExecute") +
                   HeaderList.GetPropertyHeight(properties["filters"], EScopeType.Header, EElementSize.SingleLine);
        }
    }
}