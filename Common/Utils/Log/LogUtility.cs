using UnityEngine;

public static class LogUtility
{
	static readonly System.Text.StringBuilder SB = new System.Text.StringBuilder();

	public static string Colorfy( this string str, Color color )
	{
		SB.Clear();
        SB.Append( $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>" );
		SB.Append( str );
        SB.Append( $"</color>" );
		
		var ret = SB.ToString();
		SB.Clear();
		return ret;
	}
}
