using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IntRange))]
public class IntRangePropertyDrawer : PropertyDrawer
{
    const float RANGE_BUTTON_WIDTH = 64;
    const float DASH_WIDTH = 5;

    static ToggleButton rangeToggle = new ToggleButton( "Range", UnityIcons._Menu );
    static GUIContent maxLabel = new GUIContent( "-" );

    public override void OnGUI(Rect area, SerializedProperty property, GUIContent label) => Draw( area, property, label );

    public static void Draw( Rect area, SerializedProperty property, GUIContent label, int minRange = int.MinValue, int maxRange = int.MaxValue, string minOverlayText = null, string maxOverlayText = null, Color? minColor = null, Color? maxColor = null, string minPropertyName = "min", string maxPropertyName = "max" )
    {
        var min = property.FindPropertyRelative( minPropertyName);
        var max = property.FindPropertyRelative(maxPropertyName);

        var slh = EditorGUIUtility.singleLineHeight;
        var rangeRect = area.GetLineTop( slh );

        var isRange = min.intValue < max.intValue;

        if( isRange )
        {
            var w = rangeRect.width;
            var labelW = EditorGUIUtility.labelWidth;
            var fieldsWidth = w - ( labelW + DASH_WIDTH );
            var minRect = rangeRect.GetColumnLeft( labelW + fieldsWidth / 2 );

            var minInitialVal = min.intValue;
            var minIsMax = minInitialVal >= maxRange;
            var minIsMin = minInitialVal <= minRange;
            var colorMinMin = minColor.HasValue && minIsMin;
            var colorMinMax = maxColor.HasValue && minIsMax;

            if( colorMinMax ) GuiColorManager.New( maxColor.Value );
            if( colorMinMin ) GuiColorManager.New( minColor.Value );
            EditorGUI.PropertyField( minRect, min, label );
            if( colorMinMin ) GuiColorManager.Revert();
            if( colorMinMax ) GuiColorManager.Revert();
            if( minIsMin && minOverlayText != null ) GUI.Label( minRect, minOverlayText, K10GuiStyles.unitStyle );
            else if( minIsMax && maxOverlayText != null ) GUI.Label( minRect, maxOverlayText, K10GuiStyles.unitStyle );

            if( min.intValue < minRange ) min.intValue = minRange;
            if( min.intValue > maxRange ) min.intValue = maxRange;
            if( min.intValue > max.intValue ) max.intValue = min.intValue;

            GuiLabelWidthManager.New( DASH_WIDTH );

            var maxInitialVal = max.intValue;
            var maxIsMax = maxInitialVal >= maxRange;
            var maxIsMin = maxInitialVal <= minRange;
            var colorMaxMin = minColor.HasValue && maxIsMin;
            var colorMaxMax = maxColor.HasValue && maxIsMax;

            if( colorMaxMax ) GuiColorManager.New( maxColor.Value );
            if( colorMaxMin ) GuiColorManager.New( minColor.Value );
            EditorGUI.PropertyField( rangeRect, max, maxLabel );
            if( colorMaxMin ) GuiColorManager.Revert();
            if( colorMaxMax ) GuiColorManager.Revert();
            if( maxIsMax && maxOverlayText != null ) GUI.Label( rangeRect, maxOverlayText, K10GuiStyles.unitStyle );
            else if( maxIsMin && minOverlayText != null ) GUI.Label( rangeRect, minOverlayText, K10GuiStyles.unitStyle );

            if( max.intValue > maxRange ) max.intValue = maxRange;
            if( max.intValue < minRange ) max.intValue = minRange;
            if( max.intValue < min.intValue ) min.intValue = max.intValue;

            GuiLabelWidthManager.Revert();
        }
        else
        {
            var rangeButtonRect = rangeRect.GetColumnRight( RANGE_BUTTON_WIDTH );
            var minInitialVal = min.intValue;
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

            if( min.intValue < minRange ) min.intValue = minRange;
            if( min.intValue > maxRange ) min.intValue = maxRange;

            if( rangeToggle.Draw( rangeButtonRect, ref isRange ) ) 
            {
                if( min.intValue + 1 < maxRange ) max.intValue = min.intValue + 1;
                else
                {
                    max.intValue = min.intValue;
                    min.intValue--;
                }
            }
            else max.intValue = min.intValue;
        }
    }
}
