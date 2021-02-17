using UnityEditor;
using UnityEngine;
using CPD = UnityEditor.CustomPropertyDrawer;

[CPD( typeof( ClampedWeightedStepedPolynom ) )]
public class ClampedWeightedStepedEquationPropertyDrawer : PropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		EditorGUI.LabelField( area.RequestLeft( EditorGUIUtility.labelWidth ), label );
		var restArea = area.CutLeft( EditorGUIUtility.labelWidth );

		EquationPropertyDrawer.DrawDebugFields( ref restArea, property );
		ClampedEquationPropertyDrawer.DrawMinMaxFields( ref restArea, property );
		DrawStepField( ref restArea, property );
		WeightedEquationPropertyDrawer.DrawWeightField( ref restArea, property );
		EquationPropertyDrawer.DrawParameters( restArea, property );
	}

	public static void DrawStepField( ref Rect area, SerializedProperty property )
	{
		var max = property.FindPropertyRelative( "_xStep" );

		var maxWidth = Mathf.Min( area.width / 5 );
		var fieldWidth = Mathf.Min( 100, maxWidth * .75f );
		var labelWidth = Mathf.Min( 50, maxWidth - fieldWidth );

		EquationPropertyDrawer.DrawFloatField( ref area, max, fieldWidth, labelWidth, " step: " );
	}
}
