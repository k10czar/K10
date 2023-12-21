using UnityEngine;

public static class LogUtility
{
	static readonly System.Text.StringBuilder SB = new System.Text.StringBuilder();

	public static string Colorfy( this string str, Color color, bool evenOutEditor = false )
	{
		#if UNITY_EDITOR
		bool isEditor = true;
		#else
		bool isEditor = false;
		#endif
		if( !isEditor && !evenOutEditor ) return str;
		return str.Colorfy( $"#{ColorUtility.ToHtmlStringRGB(color)}" );
	}

	public static string Colorfy( this string str, string colorName, bool evenOutEditor = false )
	{
		#if UNITY_EDITOR
		bool isEditor = true;
		#else
		bool isEditor = false;
		#endif
		if( !isEditor && !evenOutEditor ) return str;

		SB.Clear();
        SB.Append( $"<color={colorName}>" );
		SB.Append( str );
        SB.Append( $"</color>" );
		
		var ret = SB.ToString();
		SB.Clear();
		return ret;
	}
}
