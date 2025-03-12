using System;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public abstract class PropertyEditor : PropertyDrawer
    {
        private static readonly ProfilerMarker drawMarker = new("PropertyEditor.Draw");

        protected T GetTarget<T>(SerializedProperty property) where T: class => property.GetValue<T>();
        protected PropertyCollection GetProperties(SerializedProperty property) => PropertyCollection.Get(property);

        protected abstract void Draw(Rect rect, SerializedProperty property, GUIContent label);

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            using var profilerMarker = drawMarker.Auto();

            EditorGUI.BeginProperty(rect, label, property);
            Draw(rect, property, label);
            EditorGUI.EndProperty();
        }

        protected static void ApplyDirectTargetChanges(SerializedProperty property)
        {
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.Update();
        }
    }
}