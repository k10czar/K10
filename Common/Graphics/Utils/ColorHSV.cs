using UnityEngine;

public struct ColorHSV
{
	float saturation;
	float value;
	float hue;

	public ColorHSV( float hue, float saturation, float value )
	{
		this.hue = hue;
		this.saturation = saturation;
		this.value = value;
	}

	public ColorHSV( Color rgb )
	{
		float r = rgb.r;
		float g = rgb.g;
		float b = rgb.b;

		float max = Mathf.Max( r, g, b );
		float min = Mathf.Min( r, g, b );
		float chroma = max - min;
		float hue2 = 0f;
		if( !Mathf.Approximately( chroma, 0 ) )
		{
			if( max == r ) hue2 = ( g - b ) / chroma;
			else if( max == g ) hue2 = ( b - r ) / chroma + 2;
			else hue2 = ( r - g ) / chroma + 4;
		}
		hue = hue2 * 60;

		if( hue < 0 )
			hue += 360;

		value = max;
		saturation = 0;

		if( chroma != 0 )
			saturation = chroma / value;
	}

	public ColorHSV Rotate( float rotation )
	{
		rotation = ( ( rotation % 360 ) + 360 ) % 360;
		hue = ( hue + rotation ) % 360;
		return this;
	}

	public ColorHSV Invert() { return Rotate( 180 ); }
	public ColorHSV Triad() { return Rotate( 120 ); }

	// public static Color Lerp( Color a, Color b, float percentage )
	// {
	// 	var clampedPercentage = Mathf.Clamp01( percentage );
	// 	var aHsv = new ColorHSV( a );
	// 	var bHsv = new ColorHSV( b );
	// 	var lerpedColor = new ColorHSV( aHsv.hue + clampedPercentage * ( bHsv.hue - aHsv.hue ),
	// 									aHsv.saturation + clampedPercentage * ( bHsv.saturation - aHsv.saturation ),
	// 									aHsv.value + clampedPercentage * ( bHsv.value - aHsv.value ) );
	// 	return lerpedColor.RGB;
	// }

	public Color RGB
	{
		get
		{
			float chroma = saturation * value;
			float hue2 = hue / 60;
			float x = chroma * ( 1 - Mathf.Abs( hue2 % 2 - 1 ) );
			float r1 = 0;
			float g1 = 0;
			float b1 = 0;
			if( hue2 >= 0 && hue2 < 1 ) { r1 = chroma; g1 = x; }
			else if( hue2 >= 1 && hue2 < 2 ) { r1 = x; g1 = chroma; }
			else if( hue2 >= 2 && hue2 < 3 ) { g1 = chroma; b1 = x; }
			else if( hue2 >= 3 && hue2 < 4 ) { g1 = x; b1 = chroma; }
			else if( hue2 >= 4 && hue2 < 5 ) { r1 = x; b1 = chroma; }
			else if( hue2 >= 5 && hue2 <= 6 ) { r1 = chroma; b1 = x; }
			float m = value - chroma;
			return new Color( r1 + m, g1 + m, b1 + m );
		}
	}
}
