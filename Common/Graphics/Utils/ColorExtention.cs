using UnityEngine;


public static class ColorExtention
{
	#region Extention
	public static ColorHSV ToHSV( this Color color ) { return new ColorHSV( color ); }

	public static string ToHex( this Color color, bool includeHash = false )
	{
		string red = Mathf.FloorToInt( color.r * 255 ).ToString( "X2" );
		string green = Mathf.FloorToInt( color.g * 255 ).ToString( "X2" );
		string blue = Mathf.FloorToInt( color.b * 255 ).ToString( "X2" );
		string alpha = Mathf.FloorToInt( color.a * 255 ).ToString( "X2" );
		return ( includeHash ? "#" : "" ) + red + green + blue + alpha;
	}

	public static string ToHexRGB( this Color color, bool includeHash = true )
	{
		if( includeHash ) return $"#{ColorUtility.ToHtmlStringRGB( color )}";
		return ColorUtility.ToHtmlStringRGB( color );
	}

	public static string ToHexRGBA( this Color color, bool includeHash = true )
	{
		if( includeHash ) return $"#{ColorUtility.ToHtmlStringRGBA( color )}";
		return ColorUtility.ToHtmlStringRGBA( color );
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

	public static Color Saturate( this Color color, float amount )
	{
		Color.RGBToHSV(color, out var h, out var s, out var v);
		return Color.HSVToRGB(h, Mathf.Clamp01(s * amount), v);
	}

	public static Color AddSaturation( this Color color, float amount )
	{
		Color.RGBToHSV(color, out var h, out var s, out var v);
		return Color.HSVToRGB(h, Mathf.Clamp01(s + amount), v);
	}

	public static Color Enlight( this Color color, float amount )
	{
		Color.RGBToHSV(color, out var h, out var s, out var v);
		return Color.HSVToRGB(h, s, Mathf.Clamp01(v * amount));
	}

	public static Color AddLight( this Color color, float amount )
	{
		Color.RGBToHSV(color, out var h, out var s, out var v);
		return Color.HSVToRGB(h, s, Mathf.Clamp01(v + amount));
	}

    public static Color WithAlpha1(this Color color)
    {
        return color.WithAlpha(1);
    }

    public static Color WithAlpha0(this Color color)
    {
        return color.WithAlpha(0);
    }

	public static Color WithAlpha( this Color color, float amount ) 
	{ 
		color.a = amount;
		return color;
	}

	public static Color WithRed( this Color color, float amount )
	{
		color.r = amount;
		return color;
	}

	public static Color WithGreen( this Color color, float amount )
	{
		color.g = amount;
		return color;
	}

	public static Color WithBlue( this Color color, float amount )
	{
		color.b = amount;
		return color;
	}

	public static Color WithHue( this Color color, float amount )
	{
		Color.RGBToHSV(color, out var h, out var s, out var v);
		return Color.HSVToRGB(amount, s, v);
	}

	public static Color WithSaturation( this Color color, float amount )
	{
		Color.RGBToHSV(color, out var h, out var s, out var v);
		return Color.HSVToRGB(h, amount, v);
	}

	public static Color WithValue( this Color color, float amount )
	{
		Color.RGBToHSV(color, out var h, out var s, out var v);
		return Color.HSVToRGB(h, s, amount);
	}

	public static Color FromInt( int r, int g, int b ) { return FromInt( r, g, b, 255 ); }
	public static Color FromInt( int r, int g, int b, int a ) { return new Color( r / 255f, g / 255f, b / 255f, a / 255f ); }

	private static int HexToInt( char hexValue ) { return int.Parse( hexValue.ToString(), System.Globalization.NumberStyles.HexNumber ); }
	private static float HexToFloat( char hexValue ) { return HexToInt( hexValue ) / 255f; }
}