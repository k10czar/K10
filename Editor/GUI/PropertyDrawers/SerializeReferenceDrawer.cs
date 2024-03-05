using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer( typeof( SerializeReference ) )]
public sealed class SerializeReferenceDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return property.CalcSerializedReferenceHeight();
    }

    public override void OnGUI( Rect rect, SerializedProperty property, GUIContent label )
    {
        property.DrawSerializedReference( rect );
        // GUI.Label( rect, "SerializeReferenceDrawer" );
    }
}
