using UnityEditor;
using UnityEngine;
using CPD = UnityEditor.CustomPropertyDrawer;

[CPD( typeof( PercentageRangedFloatState ) )]
public class PercentageRangedFloatStatePropertyDrawer : RangedFloatStatePropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		EditorGUI.LabelField( area.RequestLeft( EditorGUIUtility.labelWidth ), label );
		var restArea = area.CutLeft( EditorGUIUtility.labelWidth );
		DrawRangedFloatVariable( property, restArea.VerticalSlice( 3, 13, 10 ) );
		EditorGUI.BeginDisabledGroup( true );
		var percentageArea = restArea.VerticalSlice( 0, 13, 3 );
		var obj = property.GetInstance( out var objType );
		var percentageProp = objType.GetProperty( PercentageRangedFloatState.PERCENTAGE_PROPERTY_NAME );
		var percentagePropInst = percentageProp.GetValue( obj ) as IValueStateObserver<float>;
		var percentageValue = percentagePropInst.Value;
		EditorGUI.FloatField( percentageArea.CutLeft( 24 ), percentageValue * 100 );
		EditorGUI.EndDisabledGroup();
		EditorGUI.LabelField( percentageArea.Shrink( 2 ), "%", K10GuiStyles.unitStyle );
		var keyCode = property.GetKey() + ".Percentage";
		SerializedPropertyExtensions.DebugWatcherField<float>( keyCode, percentageProp.GetValue( obj ), percentageProp.PropertyType, percentageArea.RequestLeft( 20 ) );
	}
	public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) => EditorGUIUtility.singleLineHeight;
}