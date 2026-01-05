using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IntRng))]
public class IntRngPropertyDrawer : PropertyDrawer
{
    const float SPACING = 2;
    const float MARGIN = 5;
    const float MARGIN2 = MARGIN * 2;
    const float BIAS_BUTTON_WIDTH = 55;

    static ToggleButton biasToggle = new ToggleButton( "BIAS", UnityIcons.align_horizontally_left_active, UnityIcons.align_horizontally_left );

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var weights = property.FindPropertyRelative("_weights");
        var count = weights.arraySize;
        var slh = EditorGUIUtility.singleLineHeight;
        return slh + count * ( SPACING + slh ) + MARGIN2;
    }

    public override void OnGUI(Rect area, SerializedProperty property, GUIContent label) => Draw( area, property, label );

    public static void Draw(Rect area, SerializedProperty property ) => Draw( area, property, new GUIContent(property.displayName) );
    public static void Draw(Rect area, SerializedProperty property, GUIContent label, int minRange = int.MinValue, int maxRange = int.MaxValue, string minOverlayText = null, string maxOverlayText = null, Color? minColor = null, Color? maxColor = null, string minPropertyName = "min", string maxPropertyName = "max")
    {
        var range = property.FindPropertyRelative("_range");
        var min = range.FindPropertyRelative("min");
        var max = range.FindPropertyRelative("max");

        GUI.Box( area, GUIContent.none );

        area = area.Shrink( MARGIN2 );

        var slh = EditorGUIUtility.singleLineHeight;
        var rangeRect = area.GetLineTop( slh );

        var minV = min.intValue;
        var maxV = max.intValue;
        var isRange = minV < maxV;
        var weights = property.FindPropertyRelative("_weights");

        if( isRange )
        {
            var buttonRect = rangeRect.GetColumnRight( BIAS_BUTTON_WIDTH );
            IntRangePropertyDrawer.Draw( rangeRect, range, label );

            var delta = maxV + 1 - minV;
            
            var wCount = weights.arraySize;
            bool hasBias = wCount > 0;
            var bias = hasBias;

            biasToggle.Draw( buttonRect, ref bias );
            if( !bias ) delta = 0;

            weights.arraySize = delta;
            if( wCount < delta )
            {
                for( int i = wCount; i < delta; i++ )
                {
                    weights.GetArrayElementAtIndex( i ).floatValue = 1;
                }
            }

            var sumOfWeights = 0f;
            for( int i = 0; i < delta; i++ ) sumOfWeights += weights.GetArrayElementAtIndex( i ).floatValue;
            if( MathAdapter.Approximately( sumOfWeights, 0f ) ) sumOfWeights = 1;
            
            if( bias )
            {
                for ( int i = 0; i < delta; i++ )
                {
                    var picks = minV + i;
                    var element = weights.GetArrayElementAtIndex(i);

                    area.GetLineTop( SPACING );

                    var lineRect = area.GetLineTop( slh );

                    GuiLabelWidthManager.New(32);
                    var newVal = EditorGUI.FloatField( lineRect.GetColumnLeft( 120 ), $"{picks}]", element.floatValue );
                    GuiLabelWidthManager.Revert();
                    element.floatValue = newVal;
                    GUIProgressBar.Draw( lineRect, element.floatValue / sumOfWeights );
                }
            }
        }
        else
        {
            weights.arraySize = 0;
        }
        
        IntRangePropertyDrawer.Draw( rangeRect, range, label, minRange, maxRange, minOverlayText, maxOverlayText, minColor, maxColor, minPropertyName, maxPropertyName );
    }
}
