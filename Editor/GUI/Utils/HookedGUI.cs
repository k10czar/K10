using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

public static class HookedGUI
{
    static readonly Color SELECTION_COLOR = Colors.Orange;

    public static Vector2 CursorPosition
    {
        [MethodImpl(Optimizations.INLINE_IF_CAN)]
        get
        {
        	if( !Application.isFocused ) return _pos;

        	var ipos = Input.mousePosition;
        	if( MathAdapter.Approximately( _lastMousePos.x, ipos.x ) &&
            	MathAdapter.Approximately( _lastMousePos.y, ipos.y ) ) return _pos;

			if( ipos.x < 0 ) return _pos;
			if( ipos.x > Screen.width ) return _pos;
			if( ipos.y < 0 ) return _pos;
			if( ipos.y > Screen.height ) return _pos;
			
        	_lastMousePos = ipos;
        	_pos.Set( ipos.x, ipos.y );

            return _pos;
        }
    }

    public static Vector2 GuiCursorPosition
	{
        [MethodImpl(Optimizations.INLINE_IF_CAN)]
		get
		{
            var curr = CursorPosition;
            var mousePosition = new Vector2( curr.x, Screen.height - curr.y );
            var len = _scrollsStack.Count;
            if( len > 0 ) mousePosition += _scrollsStack[len-1];
			return mousePosition;
		}
	}

	static Vector2 _pos = new Vector2( Screen.width / 2, Screen.height / 2 );
	static Vector2 _cursorSize = new Vector2( 24, 24 );
	static Vector2 _lastMousePos;
    static List<Vector2> _scrollsStack = new();
	static bool _delayedClick = false;
	static Vector2 _aditionallScrollDelta;

	public static void UpdateHookData( Vector2 pos, bool isButtonDown, Vector2 aditionallScrollDelta )
	{
        var px = Mathf.Clamp( pos.x, 0, Screen.width );
        var py = Mathf.Clamp( pos.y, 0, Screen.height );
		_pos.Set( px, py );
		_delayedClick = isButtonDown;
		_aditionallScrollDelta = aditionallScrollDelta;
	}

	public static void UpdateHookDataAdditive( Vector2 pos, bool isButtonDown, Vector2 aditionallScrollDelta )
	{
        var px = Mathf.Clamp( _pos.x + pos.x, 0, Screen.width );
        var py = Mathf.Clamp( _pos.y + pos.y, 0, Screen.height );
		_pos.Set( px, py );
		_delayedClick = isButtonDown;
		_aditionallScrollDelta = aditionallScrollDelta;
	}

    static readonly GUIContent CursorContent = new GUIContent( "⬆" ); //GUIContent.none;
    public static void DrawCursor()
    {
        var cPos = GuiCursorPosition;
        cPos.Set( cPos.x - _cursorSize.x * .5f, cPos.y - _cursorSize.y * .5f );
        GUI.Label( new Rect( cPos, _cursorSize ), CursorContent, K10GuiStyles.buttonStyle );
    }

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public static bool Button( Rect rt, string text )
    {
        return Button( rt, new GUIContent( text ), K10GuiStyles.buttonStyle );
    }

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public static bool Button( Rect rt, string text, GUIStyle buttonStyle )
    {
        return Button( rt, new GUIContent( text ), buttonStyle );
    }

    public static bool Button( Rect rt, GUIContent buttonText, GUIStyle buttonStyle )
    {
        bool clicked = false;

		var curr = CursorPosition;
		var mousePosition = new Vector2( curr.x, Screen.height - curr.y );
		var stack = _scrollsStack;
		var len = stack.Count;
		if( len > 0 ) mousePosition += stack[len-1];

		var hovering = rt.Contains( mousePosition );

		if( hovering )
		{
			GuiColorManager.New( SELECTION_COLOR );
			if( _delayedClick )
			{
				clicked = true;
				_delayedClick = false;
			}
		}

        clicked |= GUI.Button(rt, buttonText, buttonStyle);

        if( hovering ) GuiColorManager.Revert();
        
        return clicked;
    }

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public static bool LayoutButton( string text, params GUILayoutOption[] options )
    {
        return LayoutButton( new GUIContent( text ), K10GuiStyles.buttonStyle, options );
    }

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public static bool LayoutButton( GUIContent buttonText, GUIStyle buttonStyle, params GUILayoutOption[] options )
    {
        Rect rt = GUILayoutUtility.GetRect( buttonText, buttonStyle, options );
        return Button( rt, buttonText, buttonStyle );
    }

    public static void LayoutEndScrollView()
    {
        GUILayout.EndScrollView();
        var stack = _scrollsStack;
        stack.RemoveAt( stack.Count - 1 );
    }

    public static void LayoutBeginScrollView(ref Vector2 scroll)
    {
        Rect rt = GUILayoutUtility.GetRect( GUIContent.none, GUI.skin.scrollView );
        scroll = GUILayout.BeginScrollView( scroll );

        var pos = scroll - rt.position;
        pos.y -= 15;
        _scrollsStack.Add( pos );

        if( rt.Contains( GuiCursorPosition ) ) scroll += _aditionallScrollDelta;
    }

    public static void EndScrollView()
    {
        GUI.EndScrollView();
        var stack = _scrollsStack;
        stack.RemoveAt( stack.Count - 1 );
    }

    public static Vector2 BeginScrollView( Rect rect, Vector2 scroll, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical )
    {
        scroll = GUI.BeginScrollView( rect, scroll, viewRect, alwaysShowHorizontal, alwaysShowVertical );

        var pos = scroll - rect.position;
        pos.y -= 15;
        _scrollsStack.Add( pos );

        if( rect.Contains( GuiCursorPosition ) ) scroll += _aditionallScrollDelta;

        return scroll;
    }
}
