using System;
using Rogue.REditor;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer( typeof( BitMaskAttribute ) )]
public class EnumBitMaskPropertyDrawer : PropertyDrawer
{
    public override void OnGUI( Rect position, SerializedProperty prop, GUIContent label )
    {
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var fieldType = fieldInfo.FieldType.IsArray
            ? prop.propertyType is SerializedPropertyType.Enum
                ? fieldInfo.FieldType.GetElementType()
                : throw new Exception($"Property is not an Enum! {prop} | {fieldInfo.FieldType}")
            : fieldInfo.FieldType;

        EnumTreeGUI.DrawEnumMask(position, prop, fieldType, EColor.Support);
    }
}

[CustomPropertyDrawer( typeof( BitMaskUnmaskedAttribute ) )]
public class EnumBitMaskUnmaskedPropertyDrawer : PropertyDrawer
{
	public override void OnGUI( Rect position, SerializedProperty prop, GUIContent label )
	{
		var typeAttr = attribute as BitMaskUnmaskedAttribute;
		// Add the actual int value behind the field name
		label.text = label.text + "(" + prop.intValue + ")";
		prop.intValue = EditorGUI.MaskField( position, label, prop.intValue, System.Enum.GetNames( typeAttr.propType ) );
	}
}