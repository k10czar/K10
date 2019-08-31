using UnityEngine;
using System.Collections;


namespace K10.EditorGUIExtention
{
    public static class SeparationLine
	{
		public const int HEIGHT = 3;
		public const int WIDTH = 4;
		static readonly Texture NULL_TEXTURE = null;

		public static void Horizontal() { GUILayout.Box( NULL_TEXTURE, GUILayout.ExpandWidth( true ), GUILayout.Height( HEIGHT ) ); }
		public static void Vertical() { GUILayout.Box( NULL_TEXTURE, GUILayout.ExpandHeight( true ), GUILayout.Width( WIDTH ) ); }
		public static void Vertical( int height ) { GUILayout.Box( NULL_TEXTURE, GUILayout.Width( WIDTH ), GUILayout.Height( height ) ); }

		public static Rect Horizontal( Rect area ) { GUI.Box( new Rect( area.x, area.y, area.width, HEIGHT ), NULL_TEXTURE ); return area.CutTop( HEIGHT ); }
		public static Rect Vertical( Rect area ) { GUI.Box( new Rect( area.x, area.y, WIDTH, area.height ), NULL_TEXTURE ); return area.CutLeft( WIDTH ); }
	}
}