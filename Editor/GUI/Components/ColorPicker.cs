using UnityEngine;
using UnityEditor;

namespace K10.EditorGUIExtention 
{
	public static class ColorPicker
	{
		public static void Layout( SerializedProperty prop ) { prop.colorValue = Layout( prop.colorValue ); }

		public static Color Layout( Color color )
		{
			EditorGUILayout.BeginHorizontal();
			
			if( IconButton.Layout( "colorInvertRotate", '<', "Rotate hue -15°" ) ) color = color.ToHSV().Rotate( -15 ).RGB;
			if( IconButton.Layout( "complementaryColor", 'C', "Change to complementary color" ) ) color = color.ToHSV().Invert().RGB;
			if( IconButton.Layout( "triad", '3', "Cycle to triad" ) ) color = color.ToHSV().Triad().RGB;
			
			color = EditorGUILayout.ColorField( color );
			
			if( IconButton.Layout( "colorRotate", '>', "Rotate hue 15°" ) ) color = color.ToHSV().Rotate( 15 ).RGB;
			EditorGUILayout.EndHorizontal();
			return color;
		}		
	}
}