using UnityEngine;
using UnityEditor;
using System.Collections;

namespace K10.EditorGUIExtention 
{
	public static class FloatField
	{
		
		public static void Layout( SerializedProperty prop, params GUILayoutOption[] options )
		{
			var val = prop.floatValue;
			var newVal = EditorGUILayout.FloatField( val, options );
			if( val != newVal ) prop.floatValue = newVal;
		}
	}
}

