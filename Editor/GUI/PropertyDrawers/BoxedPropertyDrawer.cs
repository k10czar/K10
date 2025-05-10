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
        var att = (BoxedAttribute)attribute;
        var hasColor = !string.IsNullOrEmpty( att.ColorName );
        if( hasColor ) GuiColorManager.New( Colors.Get( att.ColorName ) );
        GUI.Box( rect, GUIContent.none );
        if( hasColor ) GuiColorManager.Revert();
        EditorGUI.PropertyField( rect.Shrink( 6, 6 ), property, label, true );
    }
}
