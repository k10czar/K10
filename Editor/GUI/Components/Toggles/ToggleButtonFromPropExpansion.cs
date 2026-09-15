using UnityEngine;
using UnityEditor;

public struct ToggleButtonFromPropExpansion
{
	private SerializedProperty _prop;
	private ToggleButtonFromPropExpansionLazy _button;

    public ToggleButtonFromPropExpansion( SerializedProperty prop, string label,  string iconNameOn, string iconNameOff = null )
    {
		_prop = prop;
		_button = new ToggleButtonFromPropExpansionLazy( label, iconNameOn, iconNameOff );
    }

	public float GetHeight() => _button.GetHeight(_prop);

    public bool Layout( float margin = 3, GUIStyle style = null) => _button.Layout( _prop, margin, style );

    public bool Draw( Rect rect, GUIStyle style = null) => _button.Draw( _prop, rect, style );
    public bool DrawOnTop( ref Rect rect) => _button.DrawOnTop( _prop, ref rect );
}
