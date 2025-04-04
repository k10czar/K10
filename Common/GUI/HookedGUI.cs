// #define DEBUG_POSITIONS
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
	static Vector2 _cursorSize = new Vector2( 42, 42 );

	static Vector2 _cursorSize1 = new Vector2( _cursorSize.x, _cursorSize.y * .125f );
	static Vector2 _cursorSize2 = new Vector2( _cursorSize.x * .75f, _cursorSize.y * .25f );
	static Vector2 _cursorSize3 = new Vector2( _cursorSize.x * .5f, _cursorSize.y * .375f );
	static Vector2 _cursorSize4 = new Vector2( _cursorSize.x * .25f, _cursorSize.y );

	static Vector2 _cursorOffset1 = new Vector2( - _cursorSize.x * .5f, _cursorSize.y  * .375f );
	static Vector2 _cursorOffset2 = new Vector2( - _cursorSize.x * .375f, _cursorSize.y  * .25f );
	static Vector2 _cursorOffset3 = new Vector2( - _cursorSize.x * .25f, _cursorSize.y  * .125f );
	static Vector2 _cursorOffset4 = new Vector2( - _cursorSize.x * .125f, 0 );

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

    // static readonly GUIContent CursorContent = new GUIContent( "⬆" ); //GUIContent.none;
    static readonly GUIContent CursorContent = GUIContent.none;

    

    public static void DrawCursor()
    {
        var cPos = GuiCursorPosition;
        var s = _cursorSize;
		GuiColorManager.New( Colors.Black );
        GUI.Box( new Rect( cPos +_cursorOffset1, _cursorSize1 ), CursorContent, GUI.skin.button );
        GuiColorManager.Revert();
		GuiColorManager.New( Colors.Red );
        GUI.Box( new Rect( cPos + _cursorOffset2, _cursorSize2 ), CursorContent, GUI.skin.button );
        GuiColorManager.Revert();
		GuiColorManager.New( Colors.Orange );
        GUI.Box( new Rect( cPos + _cursorOffset3, _cursorSize3 ), CursorContent, GUI.skin.button );
        GuiColorManager.Revert();
		GuiColorManager.New( Colors.Yellow );
        GUI.Box( new Rect( cPos + _cursorOffset4, _cursorSize4 ), CursorContent, GUI.skin.button );
        GuiColorManager.Revert();
#if DEBUG_POSITIONS
        GUI.Label( new Rect( cPos, new Vector2( 250, 50 ) ), $"{GuiCursorPosition}" );
#endif
    }

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public static bool Button( Rect rt, string text )
    {
        return Button( rt, new GUIContent( text ), GUI.skin.button );
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
        
#if DEBUG_POSITIONS
        GUI.Button( rt, $"({rt.x:N0},{rt.y:N0})[{rt.width:N0},{rt.height:N0}]", buttonStyle );
#endif

        if( hovering ) GuiColorManager.Revert();
        
        return clicked;
    }

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public static bool LayoutButton( string text, params GUILayoutOption[] options )
    {
        return LayoutButton( new GUIContent( text ), GUI.skin.button, options );
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

        if( rt.Contains( GuiCursorPosition ) ) AddScroll( ref scroll );

        var pos = scroll - rt.position;
        pos.y -= 15;
        _scrollsStack.Add( pos );
    }

    static void AddScroll( ref Vector2 scroll )
    {
        scroll.x = scroll.x + _aditionallScrollDelta.x;
        scroll.y = scroll.y - _aditionallScrollDelta.y;
    }

    public static void EndScrollView()
    {
        GUI.EndScrollView();
        var stack = _scrollsStack;
        stack.RemoveAt( stack.Count - 1 );
        
        var data = _debugStack[_debugStack.Count - 1];
#if DEBUG_POSITIONS
        GUI.Label( data.r, $"({data.r.x:N0},{data.r.y:N0})[{data.r.width:N0},{data.r.height:N0}]\n{data.s:N0}\n({data.v.x:N0},{data.v.y:N0})[{data.v.width:N0},{data.v.height:N0}]" );
#endif

        _debugStack.RemoveAt( _debugStack.Count - 1 );
    }

    static List<(Rect r,Vector2 s,Rect v)> _debugStack = new();

    public static Vector2 BeginScrollView( Rect rect, Vector2 scroll, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical )
    {
        scroll = GUI.BeginScrollView( rect, scroll, viewRect, alwaysShowHorizontal, alwaysShowVertical );

        if( rect.Contains( GuiCursorPosition ) ) AddScroll( ref scroll );

        var pos = scroll + viewRect.position - rect.position;
        pos.y -= 15;
        _scrollsStack.Add( pos );

        _debugStack.Add( (rect, scroll, viewRect) );

        return scroll;
    }
}
