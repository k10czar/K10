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

		K10.EditorGUIExtention.EditorGuiIndentManager.New( 0 );
		var valueRect = area.VerticalSlice( 0, 9, 3 );
		var secondsRect = area.VerticalSlice( 3, 9, 2 );
		var inpterpolationRect = area.VerticalSlice( 5, 9, 4 );
		EditorGUI.PropertyField( valueRect, value, GUIContent.none );
		EditorGUI.PropertyField( secondsRect, seconds, GUIContent.none );
		GUI.Label( secondsRect, "seconds", K10GuiStyles.unitStyle );
		EditorGUI.PropertyField( inpterpolationRect, inpterpolation, GUIContent.none );
		GUI.Label( inpterpolationRect, "inpterpolation", K10GuiStyles.unitStyle );
		K10.EditorGUIExtention.EditorGuiIndentManager.Revert();
	}

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) => EditorGUIUtility.singleLineHeight;
}