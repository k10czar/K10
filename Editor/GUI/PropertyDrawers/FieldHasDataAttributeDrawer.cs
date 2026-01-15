using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( FieldHasDataAttribute ) )]
public class FieldHasDataAttributeDrawer : PropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		var attrib = (FieldHasDataAttribute)attribute;
		var prop = property.FindPropertyRelative( attrib.PropName );;
		var isValid = PropIsValid( prop );
		GuiColorManager.New( isValid ? Colors.LightCyan : Colors.LightCoral );
        EditorGUI.PropertyField( area, property );
		GuiColorManager.Revert();
    }

	public static bool PropIsValid( SerializedProperty prop )
	{
		if( prop == null ) return false;
		switch (prop.propertyType)
		{
			case SerializedPropertyType.String: return !string.IsNullOrEmpty( prop.stringValue );
			case SerializedPropertyType.ObjectReference: return prop.objectReferenceValue != null;
			case SerializedPropertyType.ArraySize: return prop.arraySize > 0;
			// case SerializedPropertyType.Generic:
			// case SerializedPropertyType.Integer:
			// case SerializedPropertyType.Boolean:
			// case SerializedPropertyType.Float:
			// case SerializedPropertyType.Color:
			// case SerializedPropertyType.LayerMask:
			// case SerializedPropertyType.Enum:
			// case SerializedPropertyType.Vector2:
			// case SerializedPropertyType.Vector3:
			// case SerializedPropertyType.Vector4:
			// case SerializedPropertyType.Rect:
			// case SerializedPropertyType.Character:
			// case SerializedPropertyType.AnimationCurve:
			// case SerializedPropertyType.Bounds:
			// case SerializedPropertyType.Gradient:
			// case SerializedPropertyType.Quaternion:
			// case SerializedPropertyType.ExposedReference:
			// case SerializedPropertyType.FixedBufferSize:
			// case SerializedPropertyType.Vector2Int:
			// case SerializedPropertyType.Vector3Int:
			// case SerializedPropertyType.RectInt:
			// case SerializedPropertyType.BoundsInt:
			// case SerializedPropertyType.ManagedReference:
			// case SerializedPropertyType.Hash128:
			// case SerializedPropertyType.RenderingLayerMask:
		}
		return true;
	}

    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		var attrib = (FieldHasDataAttribute)attribute;
		return EditorGUI.GetPropertyHeight( property );
	}
}