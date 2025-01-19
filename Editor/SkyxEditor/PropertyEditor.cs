using System;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public abstract class PropertyEditor<T> : PropertyDrawer where T : class
    {
        private static readonly ProfilerMarker drawMarker = new("PropertyEditor.Draw");

        protected T GetTarget(SerializedProperty property) => property.GetValue<T>();
        protected PropertyCollection GetProperties(SerializedProperty property) => PropertyCollection.Get(property);

        protected virtual float ExtraHeight => 0;
        protected virtual string[] ExcludeFieldsFromHeight { get; } = Array.Empty<string>();

        protected abstract void Draw(Rect rect, SerializedProperty property, GUIContent label);

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            using var profilerMarker = drawMarker.Auto();

            EditorGUI.BeginProperty(rect, label, property);

            Draw(rect, property, label);
            PropertyCollection.TryApply(property.serializedObject);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetProperties(property).GetHeight(ExcludeFieldsFromHeight) + ExtraHeight;
        }
    }
}