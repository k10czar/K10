using UnityEngine;
using UnityEditor;
using System.Collections;

namespace K10.EditorGUIExtention 
{
	public static class Toggle
	{
		public static void Layout( string label, string tooltip, float labelWidth, SerializedProperty prop, params GUILayoutOption[] options )
		{
			var val = prop.boolValue;
			
			GuiLabelWidthManager.New( labelWidth );
			var newVal = EditorGUILayout.Toggle( new GUIContent( label, tooltip ), val, options );
			GuiLabelWidthManager.Revert();
			if( val != newVal ) prop.boolValue = newVal;
		}
	}
}