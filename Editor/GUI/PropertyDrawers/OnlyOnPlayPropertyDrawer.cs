using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( OnlyOnPlayAttribute ) )]
public sealed class OnlyOnPlayPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        if( Application.isPlaying ) return EditorGUI.GetPropertyHeight( property, label );
        else return 0;
    }

    public override void OnGUI( Rect rect, SerializedProperty property, GUIContent label )
    {
        if( !Application.isPlaying ) return;
        EditorGUI.PropertyField( rect, property, label, true );
    }
}
