using UnityEditor;
using UnityEngine;
using CPD = UnityEditor.CustomPropertyDrawer;

[CPD( typeof( RangedFloatState ) )]
public class RangedFloatStatePropertyDrawer : PropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		EditorGUI.LabelField( area.RequestLeft( EditorGUIUtility.labelWidth ), label );
		DrawRangedFloatVariable( property, area.CutLeft( EditorGUIUtility.labelWidth ) );
	}

	protected static void DrawRangedFloatVariable( SerializedProperty property, Rect area )
	{
		var minArea = area.VerticalSlice( 0, 10, 3 );
		var maxArea = area.VerticalSlice( 7, 10, 3 );
		var minProp = property.FindPropertyRelative( "_min" );
		var maxProp = property.FindPropertyRelative( "_max" );
		var valueProp = property.FindPropertyRelative( "_value" );
		var valueFloatProp = valueProp.FindPropertyRelative( "_value" );
		var val = valueFloatProp.floatValue;
		var changed = false;
		changed |= minProp.ValueStateField<float>( minArea, GUIContent.none );
		changed |= maxProp.ValueStateField<float>( maxArea, GUIContent.none );
		var minVal = minProp.FindPropertyRelative( "_value" ).floatValue;
		var maxVal = maxProp.FindPropertyRelative( "_value" ).floatValue;
		bool invalid = ( val < minVal && !Mathf.Approximately( val, minVal ) ) || ( val > maxVal && !Mathf.Approximately( val, maxVal ) );
		if( invalid ) GuiColorManager.New( K10GuiStyles.RED_TINT_COLOR );
		changed |= valueProp.ValueStateField<float>( area.VerticalSlice( 3, 10, 4 ), GUIContent.none );
		if( invalid ) GuiColorManager.Revert();
		EditorGUI.LabelField( minArea.Shrink( 2 ), "min", K10GuiStyles.unitStyle );
		EditorGUI.LabelField( maxArea.Shrink( 2 ), "max", K10GuiStyles.unitStyle );
#if UNITY_EDITOR
		if( changed || invalid ) valueFloatProp.floatValue = Mathf.Clamp( valueFloatProp.floatValue, minVal, maxVal );
#endif

	}

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) => EditorGUIUtility.singleLineHeight;
}
