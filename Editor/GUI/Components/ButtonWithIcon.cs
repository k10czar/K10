using UnityEngine;
using UnityEditor;
using System;

public struct ButtonWithIcon
{
	static GUIStyle _defaultStyle = null;
	public static GUIStyle DefaultStyle => _defaultStyle ??= new GUIStyle(EditorStyles.miniButton) { fontSize = 18, fontStyle = FontStyle.Bold, fixedHeight = 72, padding = new RectOffset(8, 8, 12, 12) };

	GUIStyle _style;
	string _label;
	string _iconName;
	GUIContent _content;
	/*GUILayoutOption[] _options;*/

	public ButtonWithIcon( string label, string iconName, GUIStyle style = null/*, params GUILayoutOption[] options*/ )
    {
		_label = label;
		_iconName = iconName;
        _style = style;
		_content = null;
		/*_options = options;*/
    }

    public bool Layout()
    {
		if( _style == null ) _style = DefaultStyle;
		if( _content == null ) _content = new GUIContent( _label, IconCache.Get( _iconName ).Texture );
		return GUILayout.Button( _content, _style/*, _options*/ );
    }

    public float GetHeight()
    {
		return 72;
    }

    public bool Draw( Rect rect )
    {
		if( _style == null ) _style = DefaultStyle;
		if( _content == null ) _content = new GUIContent( _label, IconCache.Get( _iconName ).Texture );
		return GUI.Button( rect, _content, _style/*, _options*/ );
    }

    public bool DrawOnTop(ref Rect rect)
    {
		var h = GetHeight();
		var area = rect.RequestTop( h );
		rect = rect.CutTop( h );
		if( _style == null ) _style = DefaultStyle;
		if( _content == null ) _content = new GUIContent( _label, IconCache.Get( _iconName ).Texture );
		return GUI.Button( area, _content, _style/*, _options*/ );
    }
}
