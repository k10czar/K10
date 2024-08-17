using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public interface IIconCache
{
	Texture2D Texture { get; }

	void Layout();
	void Layout( float size );
	void Layout( float w, float h );
	float LayoutWithHeight( float height );

	void Draw( Rect r );
	void Draw( Rect r, SpriteAlignment align );
	void Draw( Vector2 pos );
	void Draw( Vector2 pos, SpriteAlignment align );

	void Reset();
}

public class IconCache : IIconCache
{
	static Dictionary<string, IconCache> _icons = new Dictionary<string, IconCache>();
	public static IIconCache Get( string path )
	{
		IconCache cache;
		if( !_icons.TryGetValue( path, out cache ) )
		{
			cache = new IconCache( path );
			_icons[path] = cache;
		}
		return cache;
	}

	string _iconName;
	Texture2D _icon;
	public Texture2D Texture { get { return _icon; } }

	void ReadIcon() { _icon = (Texture2D)EditorGUIUtility.Load( "K10DefaultIcons/" + _iconName + ".png" ); }
	private IconCache( string iconName ) { _iconName = iconName; ReadIcon(); }
	public void Reset() { ReadIcon(); }

	public void Layout() { if( _icon != null ) GUILayout.Label( _icon, GUILayout.Width( _icon.width ), GUILayout.Height( _icon.height ) ); }
	public void Layout( float size ) { Layout( size, size ); }
	public float LayoutWithHeight( float height ) { if( _icon == null ) return 16; var w = _icon.width * height / _icon.height; Layout( w, height ); return w; }
	public void Layout( float w, float h ) { if( _icon != null ) GUILayout.Label( _icon, GUILayout.Width( w ), GUILayout.Height( h ) ); else GUILayout.Label( "", GUILayout.Width( w ), GUILayout.Height( h ) ); }

	public void Draw( Vector2 pos ) { Draw( pos, SpriteAlignment.Center ); }
	public void Draw( Vector2 pos, SpriteAlignment align ) { Draw( new Rect( pos.x, pos.y, 0, 0 ), align ); }
	public void Draw( Rect r ) { if( _icon != null ) GUI.Label( r, _icon ); }
	public void Draw( Rect r, SpriteAlignment align )
	{
		if( _icon != null )
		{
			var x = r.x;
			if( align == SpriteAlignment.TopCenter || align == SpriteAlignment.Center || align == SpriteAlignment.BottomCenter ) x += ( r.width - _icon.width ) / 2;
			if( align == SpriteAlignment.TopRight || align == SpriteAlignment.RightCenter || align == SpriteAlignment.BottomRight ) x += r.width - _icon.width;

			var y = r.y;
			if( align == SpriteAlignment.RightCenter || align == SpriteAlignment.Center || align == SpriteAlignment.BottomCenter ) y += ( r.height - _icon.height ) / 2;
			if( align == SpriteAlignment.TopRight || align == SpriteAlignment.TopCenter || align == SpriteAlignment.TopLeft ) y += r.height - _icon.height;

			GUI.Label( new Rect( x, y, _icon.width, _icon.height ), _icon );
		}
	}
}
