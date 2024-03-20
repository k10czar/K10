using UnityEngine;

public static class ColorUtils
{
    public static Color Lerp( Color[] colors, float value, float[] sortedRanges )
    {
		for( int i = 0; i < sortedRanges.Length; i++ )
		{
			var currentRange = sortedRanges[i];
			if( value < currentRange )
			{
				if( i == 0 ) return colors[0];
				var lastRange = sortedRanges[i-1];
				return Color.Lerp( colors[i-1], colors[i], ( value - sortedRanges[i-1] ) / ( currentRange - lastRange ) );
			}
		}
		return colors[colors.Length-1];
    }
}