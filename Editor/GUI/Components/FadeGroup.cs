using UnityEngine;
using UnityEditor;

namespace K10.EditorGUIExtention 
{
	public class FadeGroup
	{
		string _name = "";
		bool _show = false;
		
		System.Action _insideDraw;
		
		public System.Action InsideDraw { set { _insideDraw = value; } }
		public bool Show { set { _show = value; } }
		public string Name { set { _name = value; } }
		
		public FadeGroup( string name ) { _name = name; }
		
		public bool BeginLayout( Editor e ) { bool b; return BeginLayout( e, _name, false, out b ); }
		public bool BeginLayout( Editor e, out bool remove ) { return BeginLayout( e, _name, true, out remove ); }
		public bool BeginLayout( Editor e, string name, out bool remove ) { return BeginLayout( e, name, true, out remove ); }
		
		bool BeginLayout( Editor e, string name, bool removeButton, out bool remove )
		{
			remove = false;
			
			EditorGUILayout.BeginHorizontal();
			_show = EditorGUILayout.Foldout( _show, name, K10GuiStyles.foldStyle );
			
			if( _insideDraw != null )
				_insideDraw();
			
			if( removeButton ) remove = IconButton.Layout( "Remove", 22, 'X', null, Color.red );
			EditorGUILayout.EndHorizontal();
			
			if( _show )
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space( 10 );
				EditorGUILayout.BeginVertical();
			}
			
			return _show;
		}
		
		public void EndLayout()
		{
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
	}
}
