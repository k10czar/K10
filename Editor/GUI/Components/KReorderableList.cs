using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace K10.EditorGUIExtention 
{
	public class KReorderableList
	{
		private ReorderableList _list;
		
		public KReorderableList( SerializedObject obj, SerializedProperty property, string title, Texture2D icon, bool draggable = true, bool displayHeader = true, bool addButton = true, bool removeButton = true )
		{
			_list = new ReorderableList( obj, property, draggable, displayHeader, addButton, removeButton );
			
			if( icon != null ) _list.drawHeaderCallback = ( Rect r ) => { HeaderWithIcon( r, icon, title ); };
			else _list.drawHeaderCallback = ( Rect r ) => { GUI.Label( r, title ); };
		}
		
		public static void HeaderWithIcon( Rect r, Texture icon, string text )
		{
			GUI.DrawTexture( new Rect( r.x, r.y + ( r.height - icon.height ) / 2, icon.width, icon.height ), icon );
			GUI.Label( new Rect( r.x + icon.width, r.y, r.width - icon.width, r.height ), text );
		}
		
		public ReorderableList List { get { return _list; } }

		public float Height => GetHeight( _list.serializedProperty );
		public float GetHeight( SerializedProperty prop ) 
		{ 
			var baseHeight = _list.headerHeight + _list.footerHeight + 10;
			if( prop == null ) return baseHeight + _list.elementHeight;
			var count = prop.arraySize;
			if( count == 0 ) return baseHeight + _list.elementHeight;
			
			var heightCalcFunc =  _list.elementHeightCallback;
			if( heightCalcFunc == null ) return baseHeight + count * _list.elementHeight;
			for( int i = 0; i < count; i++ ) baseHeight += heightCalcFunc( i );
			return baseHeight;
		}

		public void DoLayoutList() { _list.DoLayoutList(); }
		public void Draw( Rect rect ) { _list.DoList( rect ); }
		public void DrawOnTop( ref Rect rect ) 
		{
			var h = Height;
			var listArea = rect.RequestTop( h );
			_list.DoList( listArea );
			rect = rect.CutTop( h );
		}
	}
}