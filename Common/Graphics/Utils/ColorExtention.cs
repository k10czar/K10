using UnityEngine;

public struct ColorHSV
{
	float saturation;
	float value;
	float hue;

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


public static class ColorExtention
{
	#region Extention
	public static ColorHSV ToHSV( this Color color ) { return new ColorHSV( color ); }

	public static string ToHex( this Color color ) { return ToHex( color, false ); }
	public static string ToHex( this Color color, bool includeHash )
	{
		string red = Mathf.FloorToInt( color.r * 255 ).ToString( "X2" );
		string green = Mathf.FloorToInt( color.g * 255 ).ToString( "X2" );
		string blue = Mathf.FloorToInt( color.b * 255 ).ToString( "X2" );
		string alpha = Mathf.FloorToInt( color.a * 255 ).ToString( "X2" );
		return ( includeHash ? "#" : "" ) + red + green + blue + alpha;
	}
	#endregion Extention

	public static Color FromHex( string color )
	{
		color = color.TrimStart( '#' );

		int r = ( HexToInt( color[1] ) + HexToInt( color[0] ) * 16 );
		int g = ( HexToInt( color[3] ) + HexToInt( color[2] ) * 16 );
		int b = ( HexToInt( color[5] ) + HexToInt( color[4] ) * 16 );

		int a = 255;
		if( color.Length > 7 ) a = ( HexToInt( color[7] ) + HexToInt( color[6] ) * 16 );

		return FromInt( r, g, b, a );
	}

	public static Color FromInt( int r, int g, int b ) { return FromInt( r, g, b, 255 ); }
	public static Color FromInt( int r, int g, int b, int a ) { return new Color( r / 255f, g / 255f, b / 255f, a / 255f ); }

	private static int HexToInt( char hexValue ) { return int.Parse( hexValue.ToString(), System.Globalization.NumberStyles.HexNumber ); }
	private static float HexToFloat( char hexValue ) { return HexToInt( hexValue ) / 255f; }
}