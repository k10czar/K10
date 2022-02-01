using UnityEngine;

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
		var color = _style.normal.textColor;
		_style.normal.textColor = _shadowColor;
		GUI.Label( _rect.Move( _shadowOffset ), log, _style );
		_style.normal.textColor = color;
		GUI.Label( _rect, log, _style );
	}
}
