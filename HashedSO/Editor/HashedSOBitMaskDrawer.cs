using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using K10.EditorGUIExtention;

[CustomPropertyDrawer( typeof( HashedSOBitMaskAttribute ) )]
public class HashedSOBitMaskDrawer : PropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty prop, GUIContent label )
	{
		var typeAttr = attribute as HashedSOBitMaskAttribute;

		var inst = ScriptableObject.CreateInstance( typeAttr.propType ) as HashedScriptableObject;

		if( inst == null ) return;

		var col = inst.GetCollection();
		var names = new string[col.Count];
		for( int i = 0; i < col.Count; i++ )
		{
			var e = col.GetElementBase( i );
			names[i] = e.ToStringOrNull();
		}
		
		label.text = label.text + "(" + prop.intValue + ")";
		prop.intValue = EditorGUI.MaskField( area, label, prop.intValue, names );
	}
}
