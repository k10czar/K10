using System;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public abstract class PropertyEditor<T> : PropertyDrawer where T : class
    {
        protected T GetTarget(SerializedProperty property) => property.GetValue<T>();
        protected PropertyCollection GetProperties(SerializedProperty property, bool forceReset = false) => PropertyCollection.Get(property, forceReset);

        protected virtual float ExtraHeight => 0;
        protected virtual string[] ExcludeFieldsFromHeight { get; } = Array.Empty<string>();

        protected abstract void Draw(Rect rect, SerializedProperty property, GUIContent label);

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();

            Draw(rect, property, label);

            if (property.serializedObject.ApplyModifiedProperties())
                GetProperties(property, true);
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetProperties(property).GetHeight(ExcludeFieldsFromHeight) + ExtraHeight;
        }
    }
}