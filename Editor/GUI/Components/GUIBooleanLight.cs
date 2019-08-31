using UnityEditor;
using UnityEngine;


namespace K10.EditorGUIExtention
{
    public static class GUIBooleanLight
	{
		public static void Layout( bool valid, float size = 32 )
		{
            var tex = ( valid ) ? "greenLight" : "redLight";
            GUILayout.Label( IconCache.Get( tex ).Texture, GUILayout.Width( size ), GUILayout.Height( size ) );
		}

		public static void LayoutTitle( string name, bool valid, float size = 32 )
        {
            SeparationLine.Horizontal();
            EditorGUILayout.BeginHorizontal();
                GUIBooleanLight.Layout( valid );
                EditorGUILayout.BeginVertical();
                    GUILayout.Space( 5 );
                    GUILayout.Label( name, K10GuiStyles.bigBoldCenterStyle );
                EditorGUILayout.EndVertical();
                GUIBooleanLight.Layout( valid );
            EditorGUILayout.EndHorizontal();
            SeparationLine.Horizontal();
        }
	}
}