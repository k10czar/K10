using UnityEngine;
using System;

public class ToggleButtonInstance<T> : IDrawable where T : Enum
{
	T _value;
	public T Value => _value;

	ToggleStatesButton _buttonDrawer = ToggleButton<T>.Instance;

	public void SetValue( T value )
    {
        _value = value;
    }

    public ToggleButtonInstance( T initialValue )
    {
        _value = initialValue;
    }

	public float GetHeight() => _buttonDrawer.GetHeight();

    public void Layout( float margin = 3, GUIStyle style = null )
    {
		int state = ((IConvertible)_value).ToInt32( null );
		_buttonDrawer.Layout( ref state, margin, style );
		_value = (T)(object)state;
    }
	
    public void Draw( Rect rect, GUIStyle style = null )
    {
		int state = ((IConvertible)_value).ToInt32( null );
		_buttonDrawer.Draw( rect, ref state, style );
		_value = (T)(object)state;
    }
}
