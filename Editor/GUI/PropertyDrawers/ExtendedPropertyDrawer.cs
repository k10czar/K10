using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( ExtendedDrawerAttribute ) )]
public sealed class ExtendedPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        // var h = EditorGUIUtility.singleLineHeight;
        var h = 0f;
        if( property.isArray ) h += EditorGUIUtility.singleLineHeight;
        else h += property.CalcSerializedReferenceHeight( true );
        return h;
    }

    public override void OnGUI( Rect rect, SerializedProperty property, GUIContent label )
    {
        // GUI.Label( rect.RequestTop( EditorGUIUtility.singleLineHeight ), "ExtendedSerializeReferenceDrawer" );
        // rect = rect.CutTop( EditorGUIUtility.singleLineHeight );
        if( property.isArray ) GUI.Label( rect, "Array not implemented" );
        if( !property.isArray ) property.DrawSerializedReference( rect, true );
    }
}
