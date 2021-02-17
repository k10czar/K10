using UnityEditor;
using UnityEngine;
using CPD = UnityEditor.CustomPropertyDrawer;

[CPD( typeof( WeightedPolynom ) )]
public class WeightedEquationPropertyDrawer : PropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		EditorGUI.LabelField( area.RequestLeft( EditorGUIUtility.labelWidth ), label );
		var restArea = area.CutLeft( EditorGUIUtility.labelWidth );

		EquationPropertyDrawer.DrawDebugFields( ref restArea, property );
		DrawWeightField( ref restArea, property );
		EquationPropertyDrawer.DrawParameters( restArea, property );
	}

	public static void DrawWeightField( ref Rect area, SerializedProperty property )
	{
		var max = property.FindPropertyRelative( "_weight" );

		var maxWidth = Mathf.Min( area.width / 4 );
		var fieldWidth = Mathf.Min( 100, maxWidth * .75f );
		var labelWidth = Mathf.Min( 50, maxWidth - fieldWidth );

		EquationPropertyDrawer.DrawFloatField( ref area, max, fieldWidth, labelWidth, " weight: " );
	}
}
