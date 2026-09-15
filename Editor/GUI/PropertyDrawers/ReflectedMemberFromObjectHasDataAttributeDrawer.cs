
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( FieldFromObjectHasDataAttribute ) )]
public class FieldFromObjectHasDataAttributeDrawer : ReflectedMemberFromObjectHasDataAttributeDrawer
{
    public override MemberInfo GetMemberInfo(Type type,string name, BindingFlags bindingFlags) => type.GetField(name, bindingFlags);
    public override object GetMemberValue(MemberInfo info, object obj) => (info as FieldInfo).GetValue(obj);
}

[CustomPropertyDrawer( typeof( PropertyFromObjectHasDataAttribute ) )]
public class PropertyFromObjectHasDataAttributeDrawer : ReflectedMemberFromObjectHasDataAttributeDrawer
{
    public override MemberInfo GetMemberInfo(Type type,string name, BindingFlags bindingFlags) => type.GetProperty(name, bindingFlags);
    public override object GetMemberValue(MemberInfo info, object obj) => (info as PropertyInfo).GetValue(obj);
}

public abstract class ReflectedMemberFromObjectHasDataAttributeDrawer : PropertyDrawer
{
	public abstract MemberInfo GetMemberInfo( Type type, string name, BindingFlags bindingFlags );
	public abstract object GetMemberValue( MemberInfo info, object obj );

	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		var obj = property.objectReferenceValue;
		var isValid = obj != null;
		var attrib = (ReflectedMemberFromObjectHasDataAttribute)attribute;
		string msg;
		if( isValid )
		{
			var info = GetMemberInfo(obj.GetType(),attrib.MemberName, attrib.BindingFlags);
			isValid = info != null;
			if (isValid)
			{
				var value = GetMemberValue( info, obj );
				isValid = value != null;
				msg = isValid ? attrib.ValidMessage : attrib.ErrorMessage;
				if( isValid )
				{
					if( value is ICollection col )
					{
						isValid = col.Count > 0;
						msg = isValid ? (attrib.ValidMessage ?? $"{attrib.MemberName} has {col.Count} elements") : attrib.ErrorMessage;
					}
				}
			}
			else msg = $"Member {attrib.MemberName} not found on {obj.ToStringOrNull()}"; 
		}
		else msg = "Null ref"; 
		var msgArea = area.GetLineTop( EditorGUIUtility.singleLineHeight );
		GuiColorManager.New( isValid ? Colors.LightCyan : Colors.PeachPuff );
		EditorGUI.HelpBox( msgArea, msg, isValid ? MessageType.None : MessageType.Error );
		GuiColorManager.Revert();
        EditorGUI.PropertyField( area, property );
    }

    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		// var attrib = (FieldHasDataAttribute)attribute;
		return EditorGUI.GetPropertyHeight( property ) + EditorGUIUtility.singleLineHeight;
	}
}
