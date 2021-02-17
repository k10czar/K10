using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;
using CPD = UnityEditor.CustomPropertyDrawer;

[CPD( typeof( Polynom ) )]
public class EquationPropertyDrawer : PropertyDrawer
{
	static float testValue = 0;
	const float ICON_SIZE = 16;

	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label )
	{
		EditorGUI.LabelField( area.RequestLeft( EditorGUIUtility.labelWidth ), label );
		var restArea = area.CutLeft( EditorGUIUtility.labelWidth );

		DrawDebugFields( ref restArea, property );
		DrawParameters( restArea, property );
	}

	public static Rect DrawFloatField( ref Rect area, SerializedProperty min, float fieldWidth, float labelWidth, string label )
	{
		EditorGUI.LabelField( area.RequestLeft( labelWidth ), label );
		area = area.CutLeft( labelWidth );
		min.floatValue = EditorGUI.FloatField( area.RequestLeft( fieldWidth ), min.floatValue );
		area = area.CutLeft( fieldWidth );
		return area;
	}

	public static void DrawDebugFields( ref Rect area, SerializedProperty property )
	{
		var debug = property.isExpanded;
		var debugButtonArea = area.RequestLeft( ICON_SIZE );
		area = area.CutLeft( ICON_SIZE );

		if( debug )
		{
			var fieldWidth = Mathf.Min( 100, area.width / 9 );
			var labelWidth = 25;
			var totalWidth = ( fieldWidth + labelWidth ) * 2;

			var spacing = 10;

			var testArea = area.RequestLeft( totalWidth + spacing );
			area = area.CutLeft( totalWidth + spacing );
			GUI.Box( testArea.ExpandLeft( ICON_SIZE ), string.Empty );
			testArea = testArea.HorizontalShrink( spacing );

			EditorGUI.LabelField( testArea.RequestLeft( labelWidth ), " x = " );
			testArea = testArea.CutLeft( labelWidth );
			testValue = EditorGUI.FloatField( testArea.RequestLeft( fieldWidth ), testValue );
			testArea = testArea.CutLeft( fieldWidth );

			EditorGUI.LabelField( testArea.RequestLeft( labelWidth ), " y = " );
			testArea = testArea.CutLeft( labelWidth );

			string result = "Fail";
			try
			{
				result = ( (float)property.TriggerMethod( "Evaluate", testValue ) ).ToStringOrNull();
			}
			catch( System.Exception ex )
			{
				result = ex.Message;
			}

			// EditorGUI.BeginDisabledGroup( true );
			EditorGUI.TextField( testArea.RequestLeft( fieldWidth ), result );
			// EditorGUI.EndDisabledGroup();
		}
		else
		{
			GUI.Box( debugButtonArea, string.Empty );
		}

		if( IconButton.Draw( debugButtonArea, debug ? "DebugOn" : "DebugOff" ) ) property.isExpanded = !debug;
	}

	public static void DrawParameters( Rect area, SerializedProperty property )
	{
		var parameters = property.FindPropertyRelative( "_parameters" );

		var buttonsArea = area.RequestRight( ICON_SIZE * 2 );
		if( IconButton.Draw( buttonsArea.VerticalSlice( 0, 2 ), "minus" ) ) parameters.arraySize--;
		if( IconButton.Draw( buttonsArea.VerticalSlice( 1, 2 ), "add" ) ) parameters.arraySize++;
		area = area.CutRight( buttonsArea.width );


		if( parameters.arraySize == 0 )
		{
			EditorGUI.LabelField( area, " No parameters, equation will be always 0(zero)! ( Add more parameters --> )" );
		}
		else
		{
			var parSize = 7;
			EditorGUI.LabelField( area.RequestLeft( parSize ), "(" );
			EditorGUI.LabelField( area.RequestRight( parSize ), ")" );
			area = area.HorizontalShrink( 2 * parSize );

			for( int i = 0; i < parameters.arraySize; i++ )
			{
				var lastElement = ( i >= parameters.arraySize - 1 );
				var p = parameters.GetArrayElementAtIndex( i );
				var a = area.VerticalSlice( i, parameters.arraySize );

				float xPartW = 28;
				if( lastElement ) xPartW = 16;
				xPartW = Mathf.Min( xPartW, a.width / 2 );

				EditorGUI.PropertyField( a.CutRight( xPartW ), p, GUIContent.none );
				var str = string.Empty;
				EditorGUI.LabelField( a.RequestRight( xPartW ), $" x{GetPowerString( i )}" + ( ( lastElement ) ? "" : " + " ) );
			}
		}
	}

	public static string GetPowerString( int value )
	{
		if( value == 0 ) return "⁰";
		bool negative = value < 0;
		if( negative ) value = -value;
		var num = "";
		while( value > 0 )
		{
			num = GetSupCharLastDigitUnsafe( value ) + num;
			value = value / 10;
		}
		if( negative ) return '⁻' + num;
		else return num;
	}

	static string _sups = "⁰¹²³⁴⁵⁶⁷⁸⁹";
	public static char GetSupCharLastDigit( int value ) => _sups[Mathf.Abs( value ) % 10];
	public static char GetSupCharLastDigitUnsafe( int value ) => _sups[value % 10];
}
