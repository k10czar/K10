using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( UnitAttribute ) )]
public sealed class UnitPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return EditorGUI.GetPropertyHeight( property, label );
    }

    public override void OnGUI( Rect rect, SerializedProperty property, GUIContent label )
    {
        UnitAttribute att = (UnitAttribute)attribute;
        EditorGUI.PropertyField( rect, property, label, true );
        GUI.Label( rect.Shrink( 4, 4 ), att.UnitName, K10GuiStyles.unitStyle );
    }
}
