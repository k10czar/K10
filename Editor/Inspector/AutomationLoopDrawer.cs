using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( AutomationLoop ) )]
public class AutomationLoopDrawer : PropertyDrawer
{
	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		return EditorGUI.GetPropertyHeight( property, label, true );
	}

	public override void OnGUI( Rect rect, SerializedProperty property, GUIContent label )
	{
		// EditorGUI.PropertyField( rect, property, label, true );
		GUI.Box( rect, "Test" );
	}
}
