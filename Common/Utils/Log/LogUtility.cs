#define COLOR_ON_BUILD
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

[NoAutoStaticsCleanup]
public static class LogUtility
{
	private static readonly System.Text.StringBuilder _stringBuilder = new();

	public static string Colorfy( this string str, Color color, bool evenOutEditor = false )
	{
		#if UNITY_EDITOR
		bool isEditor = !TestUtility.IsRunning;
		#else
		bool isEditor = false;
		#endif
		if( !isEditor && !evenOutEditor ) return str;
		return str.Colorfy( $"#{ColorUtility.ToHtmlStringRGB(color)}" );
	}

	public static string Colorfy( this string str, string colorName, bool evenOutEditor = false )
	{
		#if UNITY_EDITOR
		bool isEditor = !TestUtility.IsRunning;
		#else
		bool isEditor = false;
		#endif

		#if !COLOR_ON_BUILD
		if( !isEditor && !evenOutEditor ) return str;
		#endif

		_stringBuilder.Clear();
        _stringBuilder.Append( $"<color={colorName}>" );
		_stringBuilder.Append( str );
        _stringBuilder.Append( $"</color>" );

		var ret = _stringBuilder.ToString();
		_stringBuilder.Clear();
		return ret;
	}
}