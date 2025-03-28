using UnityEngine;
using System.Text.RegularExpressions;

[DefaultExecutionOrder(EXECUTION_ORDER)]
public sealed class CodeTimingOldGUIDebugExhibitor : CodeTimingDebugExhibitor
{
	[SerializeField] Rect _rect = new Rect( 0, 0, 400, 600 );
	[SerializeField] Vector2 _shadowOffset = new Vector2( 2, 2 );
	[SerializeField] Color _shadowColor = Color.black;
	[SerializeField] GUIStyle _style = GUIStyle.none;
	string log;

	protected override void SetLog( string log ) { this.log = log; }
	protected override void OnEnableChange( bool enabled ) { }

	public void OnGUI()
	{
		_style.richText = true;
		var color = _style.normal.textColor;
		_style.normal.textColor = _shadowColor;
		GUI.Label( _rect.Move( _shadowOffset ), RemoveRichTextTags(log), _style );
		_style.normal.textColor = color;
		GUI.Label( _rect, log, _style );
	}

	public static string RemoveRichTextTags(string input)
	{
		if (string.IsNullOrEmpty( input ))
			return string.Empty;

		return Regex.Replace(input, "<.*?>", string.Empty);
	}
}
