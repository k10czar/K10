using UnityEngine;
using UnityEditor;
using System;

namespace K10.EditorGUIExtention
{
	public static class IconButton
	{
		const char DEFAULT_CHAR = ' ';
		const string DEFAULT_TOOLTIP = null;
		[ConstLike] static readonly Color DEFAULT_FAIL_COLOR = Color.white;

		public static bool Layout( string iconName, char failLetter = ' ', string tooltip = "" ) { return Layout( iconName, EditorGUIUtility.singleLineHeight, failLetter, tooltip, DEFAULT_FAIL_COLOR ); }
		public static bool Layout( float iconSize, string iconName ) { return Layout( iconName, iconSize, DEFAULT_CHAR, DEFAULT_TOOLTIP, DEFAULT_FAIL_COLOR ); }
		public static bool Layout( string iconName, char failLetter, Color failColor ) { return Layout( iconName, EditorGUIUtility.singleLineHeight, failLetter, DEFAULT_TOOLTIP, failColor ); }
		public static bool Layout( string iconName, char failLetter, string tooltip, Color failColor ) { return Layout( iconName, EditorGUIUtility.singleLineHeight, failLetter, tooltip, failColor ); }
		public static bool Layout( string iconName, float iconSize, char failLetter, string tooltip, Color failColor ) { return Layout( IconCache.Get( iconName ), iconSize, failColor, failLetter, tooltip ); }

		public static bool Layout( IIconCache icon, char failLetter = ' ', string tooltip = "" ) { return Layout( icon, EditorGUIUtility.singleLineHeight, DEFAULT_FAIL_COLOR, failLetter, tooltip ); }
		public static bool Layout( IIconCache icon, float iconSize, char failLetter = ' ', string tooltip = "" ) { return Layout( icon, iconSize, DEFAULT_FAIL_COLOR, failLetter, tooltip ); }
		public static bool Layout( IIconCache icon, float iconSize, Color failColor, char failLetter = ' ', string tooltip = "" ) { return Layout( icon.Texture, iconSize, failColor, failLetter, tooltip ); }

		public static bool Layout( Texture2D texture, char failLetter = ' ', string tooltip = "" ) { return Layout( texture, EditorGUIUtility.singleLineHeight, DEFAULT_FAIL_COLOR, failLetter, tooltip ); }
		public static bool Layout( Texture2D texture, float iconSize, char failLetter = ' ', string tooltip = "" ) { return Layout( texture, iconSize, DEFAULT_FAIL_COLOR, failLetter, tooltip ); }
		public static bool Layout( Texture2D texture, float iconSize, Color failColor, char failLetter = ' ', string tooltip = "" )
		{
			var ret = false;
			if( texture != null )
			{
				ret = GUILayout.Button( new GUIContent( texture, tooltip ), K10GuiStyles.basicStyle, GUILayout.MaxWidth( iconSize ), GUILayout.MaxHeight( iconSize ) );
			}
			else
			{
				GuiColorManager.New( failColor );
				ret = GUILayout.Button( new GUIContent( failLetter.ToString(), tooltip ), GUILayout.MaxWidth( 20 ) );
				GuiColorManager.Revert();
			}
			return ret;
		}

		public static bool Draw( Rect r, string iconName ) { return Draw( r, IconCache.Get( iconName ).Texture, DEFAULT_CHAR, DEFAULT_TOOLTIP, DEFAULT_FAIL_COLOR ); }
		public static bool Draw( Rect r, string iconName, char failLetter ) { return Draw( r, IconCache.Get( iconName ).Texture, failLetter, DEFAULT_TOOLTIP, DEFAULT_FAIL_COLOR ); }
		public static bool Draw( Rect r, string iconName, char failLetter, string tooltip, Color failColor ) { return Draw( r, IconCache.Get( iconName ).Texture, failLetter, tooltip, failColor ); }
		public static bool Draw( Rect r, Texture texture ) { return Draw( r, texture, DEFAULT_CHAR, DEFAULT_TOOLTIP, DEFAULT_FAIL_COLOR ); }
		public static bool Draw( Rect r, Texture texture, char failLetter, string tooltip, Color failColor )
		{
			var ret = false;
			if( texture != null ) ret = GUI.Button( r, new GUIContent( texture, tooltip ), K10GuiStyles.basicStyle );
			else
			{
				GuiColorManager.New( failColor );
				ret = GUI.Button( r, new GUIContent( failLetter.ToString(), tooltip ) );
				GuiColorManager.Revert();
			}
			return ret;
		}

		public static class Toggle
		{
			const string GREEN_LIGHT_ICON = UnityIcons.lightMeter_greenLight;
			const string RED_LIGHT_ICON = UnityIcons.lightMeter_redLight;
			const string TRAFFIC_LIGHT_OFF_ICON = UnityIcons.lightMeter_lightOff;
			const string LIGHT_ICON_ON = UnityIcons.LampIconColored;
			const string LIGHT_ICON_OFF = UnityIcons.LampIconGrey;

			static Color DEFAULT_ON_FAIL_COLOR = Color.LerpUnclamped( Color.white, Color.green, .25f );
			static Color DEFAULT_OFF_FAIL_COLOR = Color.LerpUnclamped( Color.white, Color.red, .25f );

			private static Color GetFailColor( bool active ) => active ? DEFAULT_ON_FAIL_COLOR : DEFAULT_OFF_FAIL_COLOR;
			private static char GetFailChar( bool active ) => active ? 'O' : '-';

			public static bool Layout( bool active, float size, string onIcon, string offIcon, string tooltip = "" )
			{
				if( IconButton.Layout( active ? onIcon : offIcon, size, GetFailChar( active ), "", GetFailColor( active ) ) ) active = !active;
				return active;
			}

			public static bool GreenLight( bool active, float size, string tooltip = "" ) { return Toggle.Layout( active, size, GREEN_LIGHT_ICON, TRAFFIC_LIGHT_OFF_ICON ); }
			public static bool TrafficLight( bool active, float size, string tooltip = "" ) { return Toggle.Layout( active, size, GREEN_LIGHT_ICON, RED_LIGHT_ICON ); }
			public static bool Lamp( bool active, float size = 32, string tooltip = "" ) { return Toggle.Layout( active, size, LIGHT_ICON_ON, LIGHT_ICON_OFF ); }

			public static bool Draw( Rect rect, bool active, string onIcon, string offIcon, string tooltip = "" )
			{
				if( IconButton.Draw( rect, active ? onIcon : offIcon, GetFailChar( active ), "", GetFailColor( active ) ) ) active = !active;
				return active;
			}

			public static bool GreenLight( Rect rect, bool active, float size, string tooltip = "" ) { return Toggle.Draw( rect, active, GREEN_LIGHT_ICON, TRAFFIC_LIGHT_OFF_ICON ); }
			public static bool TrafficLight( Rect rect, bool active, float size, string tooltip = "" ) { return Toggle.Draw( rect, active, GREEN_LIGHT_ICON, RED_LIGHT_ICON ); }
			public static bool Lamp( Rect rect, bool active, float size = 32, string tooltip = "" ) { return Toggle.Draw( rect, active, LIGHT_ICON_ON, LIGHT_ICON_OFF ); }
		}
	}
}
