
using System;
using System.Collections.Generic;
using System.Reflection;
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
		DrawGUI.PropertyWithInlineProperties( area, property, GetFieldType(), label, attrib.boxed );
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


[CustomPropertyDrawer( typeof( InlineEditorAttribute ) )]
public class InlineEditorDrawer : PropertyDrawer
{
	private const string SCRIPT_FIELD = "m_Script";

	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		var attrib = (InlineEditorAttribute)attribute;
		DrawGUI.PropertyWithInlineEditor( area, property, GetFieldType(), label, attrib.boxed );
		// var attrib = (InlineEditorAttribute)attribute;
		// var drawer = PropertyDrawerUtility.GetPropertyDrawerTypeForType( GetFieldType(), attrib.ignores );
		// drawer.OnGUI( area, property, label );
		// GUI.Box( area, label );
	}

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		var attrib = (InlineEditorAttribute)attribute;
		return DrawGUI.CalculateInlineEditorHeight( property, label, attrib.boxed );
		// var attrib = (InlineEditorAttribute)attribute;
		// var drawer = PropertyDrawerUtility.GetPropertyDrawerTypeForType( GetFieldType(), attrib.ignores );
		// return drawer.GetPropertyHeight( property, label );
		// return EditorGUIUtility.singleLineHeight;
	}

	System.Type GetFieldType()
	{
		System.Type type = fieldInfo.FieldType;
		if( type.IsArray ) type = type.GetElementType();
		else if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( List<> ) ) type = type.GetGenericArguments()[0];
		return type;
	}
}