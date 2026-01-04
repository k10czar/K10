using UnityEngine;
using System;

public struct PersistentToggleButton
{
	private ToggleButtonFromValueCapsule _button;

    public bool Enabled => _button.Enabled;

    public PersistentToggleButton( string toogleCode,  string label,  string iconNameOn, string iconNameOff = null, bool defaultValue = true )
    {
		  var value = PersistentValue<bool>.At( $"Temp/PersistentToogles/{toogleCode}.tgg", defaultValue );
		  _button = new( value, label, iconNameOn, iconNameOff );
    }
	
	public float GetHeight() => _button.GetHeight();
	
    public bool Layout( float margin = 3, GUIStyle style = null ) => _button.Layout( margin, style );
    public bool Draw( Rect rect, GUIStyle style = null ) => _button.Draw( rect, style );
    public bool DrawOnTop(ref Rect rect) => _button.DrawOnTop( ref rect );
}
