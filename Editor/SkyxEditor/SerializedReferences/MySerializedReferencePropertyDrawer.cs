using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    [CustomPropertyDrawer(typeof(MySerializedReferenceAttribute))]
    public class MySerializedReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label) => SerializedRefLib.DrawDefaultInspector(rect, property, true, label);
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SerializedRefLib.GetPropertyHeight(property);
    }
}