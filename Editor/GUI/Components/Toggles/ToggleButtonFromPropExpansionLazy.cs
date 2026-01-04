using UnityEngine;
using UnityEditor;

public struct ToggleButtonFromPropExpansionLazy
{
	private ToggleButton _button;

    public ToggleButtonFromPropExpansionLazy( string label,  string iconNameOn, string iconNameOff = null )
    {
		_button = new ToggleButton( label, iconNameOn, iconNameOff );
    }

	public float GetHeight( SerializedProperty prop ) => _button.GetHeight();

    public bool Layout( SerializedProperty prop, float margin = 3, GUIStyle style = null)
    {
		bool toggle = prop.isExpanded;
        var ret = _button.Layout(ref toggle, margin, style);
		prop.isExpanded = toggle;
		return ret;
    }

    public bool Draw( SerializedProperty prop, Rect rect, GUIStyle style = null)
    {
		bool toggle = prop.isExpanded;
		var ret = _button.Draw( rect, ref toggle, style );
		prop.isExpanded = toggle;
		return ret;
    }

    public bool DrawOnTop( SerializedProperty prop, ref Rect rect)
    {
		bool toggle = prop.isExpanded;
        var ret = _button.DrawOnTop(ref rect, ref toggle);
		prop.isExpanded = toggle;
		return ret;
    }
}
