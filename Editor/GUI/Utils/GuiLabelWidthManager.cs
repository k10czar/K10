using UnityEditor;
using System.Collections.Generic;


namespace K10
{
	namespace EditorGUIExtention
	{
		public class GuiLabelWidthManager
		{
			static List<float> _widths = new List<float>();

			public static void New( float width )
			{
				_widths.Add( EditorGUIUtility.labelWidth );
				EditorGUIUtility.labelWidth = width;
			}

			public static void Revert( int reversions = 1 )
			{
				while( _widths.Count > 0 && reversions > 0 )
				{
					EditorGUIUtility.labelWidth = _widths[_widths.Count - 1];
					_widths.RemoveAt( _widths.Count - 1 );
					reversions--;
				}
			}
		}

		public class IdentLevelManager
		{
			static List<int> _levels = new List<int>();

			public static void New( int level )
			{
				_levels.Add( EditorGUI.indentLevel );
				EditorGUI.indentLevel = level;
			}

			public static void Revert()
			{
				if( _levels.Count > 0 )
				{
					EditorGUI.indentLevel = _levels[_levels.Count - 1];
					_levels.RemoveAt( _levels.Count - 1 );
				}
			}
		}
	}
}