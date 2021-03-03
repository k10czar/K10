using UnityEditor;
using UnityEngine;
using CPD = UnityEditor.CustomPropertyDrawer;

[CPD( typeof( ClampedPolynom ) )]
public class ClampedEquationPropertyDrawer : PropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		EditorGUI.LabelField( area.RequestLeft( EditorGUIUtility.labelWidth ), label );
		var restArea = area.CutLeft( EditorGUIUtility.labelWidth );

		EquationPropertyDrawer.DrawDebugFields( ref restArea, property );
		DrawMinMaxFields( ref restArea, property );
		EquationPropertyDrawer.DrawParameters( restArea, property );
	}

	public static void DrawMinMaxFields( ref Rect area, SerializedProperty property )
	{
		var min = property.FindPropertyRelative( "_min" );
		var max = property.FindPropertyRelative( "_max" );

		var maxWidth = Mathf.Min( area.width / 6 );

		var fieldWidth = Mathf.Min( 100, maxWidth / 3 );
		var labelWidth = Mathf.Min( 32, ( maxWidth - 2 * fieldWidth ) / 2 );
		var totalWidth = ( fieldWidth + labelWidth ) * 2;

		var testArea = area.RequestLeft( totalWidth );
		area = area.CutLeft( totalWidth );

		EquationPropertyDrawer.DrawFloatField( ref testArea, min, fieldWidth, labelWidth, " min: " );
		EquationPropertyDrawer.DrawFloatField( ref testArea, max, fieldWidth, labelWidth, " max: " );
	}
}
