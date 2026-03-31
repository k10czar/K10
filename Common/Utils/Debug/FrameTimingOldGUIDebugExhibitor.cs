using UnityEngine;
using System.Text.RegularExpressions;

[DefaultExecutionOrder(EXECUTION_ORDER)]
public sealed class FrameTimingOldGUIDebugExhibitor : FrameTimingDebugExhibitor
{
	[SerializeField] Rect _rect = new Rect( 0, 0, 400, 600 );
	[SerializeField] Vector2 _shadowOffset = new Vector2( 2, 2 );
	[SerializeField] Color _shadowColor = Color.black;
	[SerializeField] GUIStyle _style = GUIStyle.none;
	string log;
	string uncoloredLog;

	protected override void SetLog( string log ) { this.log = log; uncoloredLog = log.WithoutColorTags(); }
	protected override void OnEnableChange( bool enabled ) { }

	public void Start()
	{
#if UNITY_ANDROID && UNITY_IOS
		_style.fontSize = MathAdapter.RoundToInt( _style.fontSize * 1.3f );
#endif
	}

	public void OnGUI()
	{
		_style.richText = true;
		var color = _style.normal.textColor;
		_style.normal.textColor = _shadowColor;
		GUI.Label( _rect.Move( _shadowOffset ), uncoloredLog, _style );
		_style.normal.textColor = color;
		GUI.Label( _rect, log, _style );
	}

	public static string RemoveRichTextTags(string input)
	{
		if (string.IsNullOrEmpty( input ))
			return string.Empty;

		return Regex.Replace(input, "<.*?>", string.Empty);
	}

    public void Set(Rect rect, Vector2 shadowOffset, Color shadowColor, GUIStyle style)
    {
		_rect = rect;
		_shadowOffset = shadowOffset;
		_shadowColor = shadowColor;
        _style = style; 
    }
}
