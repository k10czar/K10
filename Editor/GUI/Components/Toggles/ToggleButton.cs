using UnityEngine;
using UnityEditor;
using System;

public class ToggleButton<T> where T : Enum
{
    static ToggleStatesButton _staticButton;

    public static ToggleStatesButton Instance
    {
        get
        {
			if( _staticButton == null )
            {
				var t = typeof( T );
				var names = Enum.GetNames( t );
				if( IconCache.ExistAny( names ) ) _staticButton = new ToggleStatesButton( names, names );
				else _staticButton  = new ToggleStatesButton( names, new string[]{ t.ToString() } );
            }
            return _staticButton;
        }
    }
}

public struct ToggleButton
{
	private string _label;
	private string _iconNameOn;
	private string _iconNameOff;

	GUIContent _onContent;
	GUIContent _offContent;

	GUIContent TrueContent => _onContent ??= new GUIContent( _label, IconCache.Get( _iconNameOn ).Texture );
	GUIContent FalseContent => _offContent ??= ( _iconNameOff == null ) ? TrueContent : new GUIContent( _label, IconCache.Get( _iconNameOn ).Texture );
	
    public ToggleButton( string label,  string iconNameOn, string iconNameOff = null )
    {
        _label = label;
        _iconNameOn = iconNameOn;
        _iconNameOff = iconNameOff;
		_onContent = null;
		_offContent = null;
    }

	public float GetHeight() => EditorGUIUtility.singleLineHeight + 3;

    public bool Layout( ref bool value, float margin = 3, GUIStyle style = null )
    {
		var lh = EditorGUIUtility.singleLineHeight;
		var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + margin ) );
		rect.RequestTop( lh + margin );
		GUILayout.Space( lh + margin );
		var ret = Draw( rect, ref value, style );

		EditorGUILayout.EndHorizontal();

		return ret;
    }
	
    public bool Draw( Rect rect, ref bool value, GUIStyle style = null )
    {
		if( style == null ) style = K10GuiStyles.smallBoldCenterStyle;
		var toggle = value;

		GUIContent content = toggle ? TrueContent : FalseContent;
		GUI.Box( rect, GUIContent.none );
		var change = GUI.Button( rect, content, style );

		if( change )
        {
			toggle = !toggle;
			value = toggle;
        }

		return toggle;
    }

    public bool DrawOnTop( ref Rect rect, ref bool value )
    {
		var h = GetHeight();
		var area = rect.GetLineTop( h );
		return Draw( area, ref value );
    }
}
