using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( OnlyPropertyAttribute ) )]
public class OnlyPropertyDrawer : PropertyDrawer {
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
        var myAttribute = attribute as OnlyPropertyAttribute;
        var innerProp = property.FindPropertyRelative( myAttribute.Path );
        return EditorGUI.GetPropertyHeight(innerProp, label, true);
    }

    public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
    {
        var myAttribute = attribute as OnlyPropertyAttribute;
        var innerProp = property.FindPropertyRelative(myAttribute.Path);
        EditorGUI.PropertyField(area, innerProp, new GUIContent( property.displayName ), true);
    }
}
