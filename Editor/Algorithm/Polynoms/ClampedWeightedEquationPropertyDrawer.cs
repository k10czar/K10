using UnityEditor;
using UnityEngine;
using CPD = UnityEditor.CustomPropertyDrawer;

[CPD( typeof( ClampedWeightedPolynom ) )]
public class ClampedWeightedEquationPropertyDrawer : PropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		EditorGUI.LabelField( area.RequestLeft( EditorGUIUtility.labelWidth ), label );
		var restArea = area.CutLeft( EditorGUIUtility.labelWidth );

		EquationPropertyDrawer.DrawDebugFields( ref restArea, property );
		ClampedEquationPropertyDrawer.DrawMinMaxFields( ref restArea, property );
		WeightedEquationPropertyDrawer.DrawWeightField( ref restArea, property );
		EquationPropertyDrawer.DrawParameters( restArea, property );
	}
}
