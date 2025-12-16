using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public abstract class PropertyEditor : PropertyDrawer
    {
        private static readonly ProfilerMarker drawMarker = new("PropertyEditor.Draw");

        public virtual bool FlagPropertyBounds => true;

        protected static T GetTarget<T>(SerializedProperty property) where T: class => property.GetValue<T>();
        protected static PropertyCollection GetProperties(SerializedProperty property) => PropertyCollection.Get(property);

        protected abstract void Draw(Rect rect, SerializedProperty property, GUIContent label);

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            using var profilerMarker = drawMarker.Auto();

            if (FlagPropertyBounds) EditorGUI.BeginProperty(rect, label, property);
            Draw(rect, property, label);
            if (FlagPropertyBounds) EditorGUI.EndProperty();
        }

        protected static void ApplyDirectTargetChanges(SerializedProperty property)
        {
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.Update();
        }

        public virtual bool ResetNewObject(SerializedProperty newElement) => false;
        public virtual bool ResetDuplicatedObject(SerializedProperty newElement) => false;
    }
}