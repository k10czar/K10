using UnityEngine;
using UnityEditor;
using System.Collections;

namespace K10.EditorGUIExtention 
{
	public static class IntField
	{
		public static void Layout( string label, string tooltip, float labelWidth, SerializedProperty prop, params GUILayoutOption[] options )
		{
			var val = prop.intValue;
			
			GuiLabelWidthManager.New( labelWidth );
			var newVal = EditorGUILayout.IntField( new GUIContent( label, tooltip ), val, options );
			GuiLabelWidthManager.Revert();
			if( val != newVal ) prop.intValue = newVal;
		}
		
		public static void Layout( SerializedProperty prop, params GUILayoutOption[] options )
		{
			var val = prop.intValue;
			var newVal = EditorGUILayout.IntField( val, options );
			if( val != newVal ) prop.intValue = newVal;
		}
	}
}
