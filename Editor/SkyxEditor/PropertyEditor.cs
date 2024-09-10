using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public class PropertyEditor<T> : PropertyDrawer where T : class
    {
        protected T GetTarget(SerializedProperty property) => property.GetValue<T>();
        protected PropertyCollection GetProperties(SerializedProperty property) => PropertyCollection.Get(property);

        protected virtual float ExtraHeight => 0;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.GetPropertyChildHeight() + ExtraHeight;
        }
    }
}