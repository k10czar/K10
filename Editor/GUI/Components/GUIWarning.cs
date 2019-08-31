using UnityEditor;
using UnityEngine;


namespace K10.EditorGUIExtention
{
    public static class GUIWarning
	{		
		public static void Layout( string msg )
		{
			EditorGUILayout.BeginHorizontal();
				GuiColorManager.New( Color.white );
				GUILayout.Label( IconCache.Get( "warning" ).Texture, GUILayout.Width( 64 ), GUILayout.Height( 64 ) );
				GuiColorManager.Revert();
				GuiColorManager.New( Color.LerpUnclamped( Color.red, Color.white, .75f ) );
				GUILayout.Box( msg, GUILayout.ExpandWidth( true ), GUILayout.MinHeight( 59 ) );
				GuiColorManager.Revert();
			EditorGUILayout.EndHorizontal();
		}
	}
}