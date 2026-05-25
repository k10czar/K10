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

    public bool Layout( bool isExpanded, float margin = 3, GUIStyle style = null)
    {
        _button.Layout(ref isExpanded, margin, style);
		return isExpanded;
    }

    public bool Layout( IValueCapsule<bool> isExpanded, float margin = 3, GUIStyle style = null)
    {
        var isExpandedBool = isExpanded.Get;
        _button.Layout(ref isExpandedBool, margin, style);
        isExpanded.Set = isExpandedBool;
		return isExpandedBool;
    }

    public bool Layout( ref bool isExpanded, float margin = 3, GUIStyle style = null)
    {
        _button.Layout(ref isExpanded, margin, style);
		return isExpanded;
    }

    public bool Layout( SerializedProperty prop, float margin = 3, GUIStyle style = null)
    {
        var open = Layout( prop.isExpanded, margin, style );
		prop.isExpanded = open;
        return open;
    }

    public bool Draw( SerializedProperty prop, Rect rect, GUIStyle style = null)
    {
		bool toggle = prop.isExpanded;
		_button.Draw( rect, ref toggle, style );
		prop.isExpanded = toggle;
		return toggle;
    }

    public bool DrawOnTop( SerializedProperty prop, ref Rect rect)
    {
		bool toggle = prop.isExpanded;
        _button.DrawOnTop(ref rect, ref toggle);
		prop.isExpanded = toggle;
		return toggle;
    }
}
