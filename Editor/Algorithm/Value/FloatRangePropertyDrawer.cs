using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FloatRange))]
public class FloatRangePropertyDrawer : PropertyDrawer
{
    const float RANGE_BUTTON_WIDTH = 64;
    const float DASH_WIDTH = 5;

    static ToggleButton rangeToggle = new ToggleButton( "Range", UnityIcons._Menu );
    static GUIContent maxLabel = new GUIContent( "-" );

    public override void OnGUI(Rect area, SerializedProperty property, GUIContent label) => Draw( area, property, label );

    public static void Draw( Rect area, SerializedProperty property, GUIContent label, float minRange = float.MinValue, float maxRange = float.MaxValue, string minOverlayText = null, string maxOverlayText = null, Color? minColor = null, Color? maxColor = null, string minPropertyName = "min", string maxPropertyName = "max" )
    {
        var min = property.FindPropertyRelative( minPropertyName );
        var max = property.FindPropertyRelative( maxPropertyName );

        var slh = EditorGUIUtility.singleLineHeight;
        var rangeRect = area.GetLineTop( slh );

        var isRange = min.floatValue < max.floatValue;

        if( isRange )
        {
            var w = rangeRect.width;
            var labelW = EditorGUIUtility.labelWidth;
            var fieldsWidth = w - ( labelW + DASH_WIDTH );
            var minRect = rangeRect.GetColumnLeft( labelW + fieldsWidth / 2 );

            var minInitialVal = min.floatValue;
            var minIsMax = minInitialVal >= maxRange;
            var minIsMin = minInitialVal <= minRange;
            var colorMinMin = minColor.HasValue && minIsMin;
            var colorMinMax = maxColor.HasValue && minIsMax;

            if( colorMinMax ) GuiColorManager.New( maxColor.Value );
            if( colorMinMin ) GuiColorManager.New( minColor.Value );
            EditorGUI.DelayedFloatField( minRect, min, label );
            if( colorMinMin ) GuiColorManager.Revert();
            if( colorMinMax ) GuiColorManager.Revert();
            if( minIsMin && minOverlayText != null ) GUI.Label( minRect, minOverlayText, K10GuiStyles.unitStyle );
            else if( minIsMax && maxOverlayText != null ) GUI.Label( minRect, maxOverlayText, K10GuiStyles.unitStyle );

            if( min.floatValue < minRange ) min.floatValue = minRange;
            if( min.floatValue > maxRange ) min.floatValue = maxRange;
            if( min.floatValue > max.floatValue ) max.floatValue = min.floatValue;

            GuiLabelWidthManager.New( DASH_WIDTH );

            var maxInitialVal = max.floatValue;
            var maxIsMax = maxInitialVal >= maxRange;
            var maxIsMin = maxInitialVal <= minRange;
            var colorMaxMin = minColor.HasValue && maxIsMin;
            var colorMaxMax = maxColor.HasValue && maxIsMax;

            if( colorMaxMax ) GuiColorManager.New( maxColor.Value );
            if( colorMaxMin ) GuiColorManager.New( minColor.Value );
            EditorGUI.DelayedFloatField( rangeRect, max, maxLabel );
            if( colorMaxMin ) GuiColorManager.Revert();
            if( colorMaxMax ) GuiColorManager.Revert();
            if( maxIsMax && maxOverlayText != null ) GUI.Label( rangeRect, maxOverlayText, K10GuiStyles.unitStyle );
            else if( maxIsMin && minOverlayText != null ) GUI.Label( rangeRect, minOverlayText, K10GuiStyles.unitStyle );

            if( max.floatValue > maxRange ) max.floatValue = maxRange;
            if( max.floatValue < minRange ) max.floatValue = minRange;
            if( max.floatValue < min.floatValue ) min.floatValue = max.floatValue;

            GuiLabelWidthManager.Revert();
        }
        else
        {
            var rangeButtonRect = rangeRect.GetColumnRight( RANGE_BUTTON_WIDTH );
            var minInitialVal = min.floatValue;
            var minIsMax = minInitialVal >= maxRange;
            var minIsMin = minInitialVal <= minRange;
            var colorMinMin = minColor.HasValue && minIsMin;
            var colorMinMax = maxColor.HasValue && minIsMax;

            if( colorMinMin ) GuiColorManager.New( minColor.Value );
            if( colorMinMax ) GuiColorManager.New( maxColor.Value );
            EditorGUI.PropertyField( rangeRect, min, label );
            if( colorMinMax ) GuiColorManager.Revert();
            if( colorMinMin ) GuiColorManager.Revert();
            if( minIsMax && maxOverlayText != null ) GUI.Label( rangeRect, maxOverlayText, K10GuiStyles.unitStyle );
            else if( minIsMin && minOverlayText != null ) GUI.Label( rangeRect, minOverlayText, K10GuiStyles.unitStyle );

            if( min.floatValue < minRange ) min.floatValue = minRange;
            if( min.floatValue > maxRange ) min.floatValue = maxRange;

            if( rangeToggle.Draw( rangeButtonRect, ref isRange ) )
            {
                if( min.floatValue + 1f < maxRange ) max.floatValue = min.floatValue + 1f;
                else
                {
                    max.floatValue = min.floatValue;
                    min.floatValue--;
                }
            }
            else max.floatValue = min.floatValue;
        }
    }
}
