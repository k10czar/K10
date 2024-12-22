using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class FoldoutBoxScope : GUI.Scope
    {
        public readonly bool isExpanded;

        public FoldoutBoxScope(SerializedProperty property)
            : this(property, ObjectNames.NicifyVariableName(property.name)) {}

        public FoldoutBoxScope(SerializedProperty property, string title)
        {
            isExpanded = BoxGUI.BeginFoldout(title, property);
        }

        public FoldoutBoxScope(string title, ref bool isExpandedRef)
        {
            isExpanded = BoxGUI.BeginFoldout(title, ref isExpandedRef);
        }

        protected override void CloseScope()
        {
            if (isExpanded) BoxGUI.EndBox();
        }
    }
}