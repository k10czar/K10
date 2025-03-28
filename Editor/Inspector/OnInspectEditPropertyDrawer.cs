

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ICallOnInspectEdit), true)]
public class OnInspectEditPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndProperty();

        var obj = property.GetInstance();
        if( obj is ICallOnInspectEdit edit ) edit.EDITOR_OnInspectEdit();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, true);
}