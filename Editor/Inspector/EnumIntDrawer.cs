using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumIntAttribute))]
public class EnumIntDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        rect = EditorGUI.PrefixLabel(rect, label);
        var attr = (EnumIntAttribute)attribute;
        var names = System.Enum.GetNames(attr.EnumType);
        prop.intValue = EditorGUI.Popup(rect, prop.intValue, names);
    }
}