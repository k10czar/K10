using UnityEngine;

public static class Colorfy
{
	public static string OpenTag( Color color, bool evenOutEditor = false )
	{
		#if UNITY_EDITOR
		bool isEditor = !TestUtility.IsRunning;
		#else
		bool isEditor = false;
		#endif
		if( !isEditor && !evenOutEditor ) return string.Empty;
		return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
	}

	public static string OpenTag( string colorName, bool evenOutEditor = false )
	{
		#if UNITY_EDITOR
		bool isEditor = !TestUtility.IsRunning;
		#else
		bool isEditor = false;
		#endif
		if( !isEditor && !evenOutEditor ) return string.Empty;
        return $"<color={colorName}>";
	}

	public static string CloseTag( bool evenOutEditor = false )
	{
		#if UNITY_EDITOR
		bool isEditor = !TestUtility.IsRunning;
		#else
		bool isEditor = false;
		#endif
		if( !isEditor && !evenOutEditor ) return string.Empty;
        return $"</color>";
	}
}
