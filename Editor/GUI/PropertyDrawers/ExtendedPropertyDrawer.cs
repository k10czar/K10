using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( ExtendedDrawerAttribute ) )]
public sealed class ExtendedPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return property.CalcSerializedReferenceHeight( true );
    }

    public override void OnGUI( Rect rect, SerializedProperty property, GUIContent label )
    {
        ExtendedDrawerAttribute att = (ExtendedDrawerAttribute)attribute;
        property.DrawSerializedReference( rect, true, att.ShowName );
    }
}
