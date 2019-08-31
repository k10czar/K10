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

		public float Height { get { return _list.headerHeight + Mathf.Max( 1, _list.serializedProperty.arraySize ) * _list.elementHeight + _list.footerHeight + 10; } }

		public void DoLayoutList() { _list.DoLayoutList(); }
		public void Draw( Rect rect ) { _list.DoList( rect ); }
	}
}