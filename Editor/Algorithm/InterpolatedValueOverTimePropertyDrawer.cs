using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( InterpolatedValueOverTime ) )]
public class InterpolatedValueOverTimePropertyDrawer : PropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		var value = property.FindPropertyRelative( "_value" );
		var seconds = property.FindPropertyRelative( "_seconds" );
		var inpterpolation = property.FindPropertyRelative( "_inpterpolation" );

		var w = area.width;
		var lw = EditorGUIUtility.labelWidth;

		EditorGUI.LabelField( area.CutRight( w - lw ), property.displayName );
		area = area.CutLeft( lw );

		K10.EditorGUIExtention.IdentLevelManager.New( 0 );
		EditorGUI.PropertyField( area.VerticalSlice( 0, 6, 2 ), value, GUIContent.none );
		EditorGUI.PropertyField( area.VerticalSlice( 2, 6 ), seconds, GUIContent.none );
		EditorGUI.PropertyField( area.VerticalSlice( 3, 6, 3 ), inpterpolation, GUIContent.none );
		GUI.Label( area.VerticalSlice( 2, 6 ), "seconds", K10GuiStyles.unitStyle );
		GUI.Label( area.VerticalSlice( 3, 6, 3 ), "inpterpolation", K10GuiStyles.unitStyle );
		K10.EditorGUIExtention.IdentLevelManager.Revert();
	}

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) => EditorGUIUtility.singleLineHeight;
}