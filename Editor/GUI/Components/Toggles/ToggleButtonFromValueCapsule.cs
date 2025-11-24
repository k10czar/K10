using UnityEngine;

public struct ToggleButtonFromValueCapsule
{
	private IValueCapsule<bool> _value;
	private ToggleButton _button;
	
    public bool Enabled => _value.Get;

    public ToggleButtonFromValueCapsule( IValueCapsule<bool> value,  string label,  string iconNameOn, string iconNameOff = null )
    {
		_value = value;
		_button = new ToggleButton( label, iconNameOn, iconNameOff );
    }
	public float GetHeight() => _button.GetHeight();

    public bool Layout(float margin = 3, GUIStyle style = null)
    {
		bool toggle = _value.Get;
        var ret = _button.Layout(ref toggle, margin, style);
		_value.Set = toggle;
		return ret;
    }

    public bool Draw(Rect rect, GUIStyle style = null)
    {
		bool toggle = _value.Get;
		var ret = _button.Draw( rect, ref toggle, style );
		_value.Set = toggle;
		return ret;
    }

    public bool DrawOnTop(ref Rect rect)
    {
		bool toggle = _value.Get;
        var ret = _button.DrawOnTop(ref rect, ref toggle);
		_value.Set = toggle;
		return ret;
    }
}
