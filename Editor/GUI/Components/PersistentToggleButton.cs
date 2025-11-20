using UnityEngine;
using UnityEditor;
using System;

public struct PersistentToggleButton
{
	private PersistentValue<bool> _value;

	private string _label;
	private string _iconNameOn;
	private string _iconNameOff;

	GUIContent _onContent;
	GUIContent _offContent;

	GUIContent TrueContent => _onContent ??= new GUIContent( _label, IconCache.Get( _iconNameOn ).Texture );
	GUIContent FalseContent => _offContent ??= ( _iconNameOff == null ) ? TrueContent : new GUIContent( _label, IconCache.Get( _iconNameOn ).Texture );

    public bool Enabled => _value.Get;

    public PersistentToggleButton( string toogleCode,  string label,  string iconNameOn, string iconNameOff = null, bool defaultValue = true )
    {
		_value = PersistentValue<bool>.At( $"Temp/PersistentToogles/{toogleCode}.tgg", defaultValue );
        _label = label;
        _iconNameOn = iconNameOn;
        _iconNameOff = iconNameOff;
		_onContent = null;
		_offContent = null;
    }
	
    public bool Layout( float margin = 3, GUIStyle style = null )
    {
		var lh = EditorGUIUtility.singleLineHeight;
		var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + margin ) );
		rect.RequestTop( lh + margin );
		GUILayout.Space( lh + margin );
		var ret = Draw( rect, style );

		EditorGUILayout.EndHorizontal();

		return ret;
    }

	public float GetHeight() => EditorGUIUtility.singleLineHeight + 3;
	
    public bool Draw( Rect rect, GUIStyle style = null )
    {
		if( style == null ) style = K10GuiStyles.smallBoldCenterStyle;
		var toggle = _value.Get;

		GUIContent content = toggle ? TrueContent : FalseContent;
		var change = GUI.Button( rect, content, style );

		if( change )
        {
			toggle = !toggle;
			_value.Set = toggle;
        }

		return toggle;
    }

    public bool DrawOnTop(ref Rect rect)
    {
		var h = GetHeight();
		var area = rect.RequestTop( h );
		rect = rect.CutTop( h );
		return Draw( area );
    }
}
