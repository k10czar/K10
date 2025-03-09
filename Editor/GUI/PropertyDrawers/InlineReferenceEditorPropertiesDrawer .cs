
using System.Collections.Generic;
using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer( typeof( InlinePropertiesAttribute ) )]
public class InlinePropertiesDrawer : PropertyDrawer
{
	private const string SCRIPT_FIELD = "m_Script";

	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		var attrib = (InlinePropertiesAttribute)attribute;
		DrawGUI.InlineEditor( area, property, GetFieldType(), label, attrib.boxed );
	}

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		var attrib = (InlinePropertiesAttribute)attribute;
		return DrawGUI.CalculateInlineEditorHeight( property, label, attrib.boxed );
	}

	System.Type GetFieldType()
	{
		System.Type type = fieldInfo.FieldType;
		if( type.IsArray ) type = type.GetElementType();
		else if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( List<> ) ) type = type.GetGenericArguments()[0];
		return type;
	}
}