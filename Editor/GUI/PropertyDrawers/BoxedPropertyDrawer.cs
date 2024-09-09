using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( BoxedAttribute ) )]
public sealed class BoxedPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return EditorGUI.GetPropertyHeight( property, label ) + 6;
    }

    public override void OnGUI( Rect rect, SerializedProperty property, GUIContent label )
    {
        GUI.Box( rect, GUIContent.none );
        EditorGUI.PropertyField( rect.Shrink( 6, 6 ), property, label, true );
    }
}
