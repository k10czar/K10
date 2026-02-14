using UnityEngine;
using UnityEditor;

public class ToggleStatesButton
{
	private string[] _labels;
	private string[] _icons;

	GUIContent[] _contents;

	int _maxStateCount;

	GUIContent GetContent( int state )
    {
		if( state < 0 || state >= _maxStateCount ) return GUIContent.none;
        if( _contents == null ) _contents = new GUIContent[ _maxStateCount ];
		if( _contents[state] == null ) _contents[state] = new GUIContent( GetLabel( state ), IconCache.Get( GetIconName( state ) ).Texture );
		return _contents[state];
    }

	string GetLabel( int state )
    {
		if( _labels == null ) return string.Empty;
		var len = _labels.Length;
		if( len == 0 ) return string.Empty;
		if( state < 0 || state >= _maxStateCount ) return string.Empty;
		if( state >= len ) return _labels[len - 1];
		return _labels[state];
    }

	string GetIconName( int state )
    {
		if( _icons == null ) return string.Empty;
		var len = _icons.Length;
		if( len == 0 ) return string.Empty;
		if( state < 0 || state >= _maxStateCount ) return string.Empty;
		if( state >= len ) return _icons[len - 1];
		return _icons[state];
    }
	
    public ToggleStatesButton( string[] labels,  string[] icons )
    {
        _labels = labels;
        _icons = icons;
    }

	public float GetHeight() => EditorGUIUtility.singleLineHeight + 3;

    public void Layout( ref int state, float margin = 3, GUIStyle style = null )
    {
		var lh = EditorGUIUtility.singleLineHeight;
		var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + margin ) );
		rect.RequestTop( lh + margin );
		GUILayout.Space( lh + margin );
		Draw( rect, ref state, style );

		EditorGUILayout.EndHorizontal();
    }
	
    public void Draw( Rect rect, ref int state, GUIStyle style = null )
    {
		if( style == null ) style = K10GuiStyles.smallBoldCenterStyle;

		GUIContent content = GetContent( state );
		GUI.Box( rect, GUIContent.none );
		var change = GUI.Button( rect, content, style );

		if( change )
        {
			state = ( state + 1 ) % _maxStateCount;
        }
    }

    public void DrawOnTop( ref Rect rect, ref int state )
    {
		var h = GetHeight();
		var area = rect.GetLineTop( h );
		Draw( area, ref state );
    }
}
