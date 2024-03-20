using UnityEditor;
using System.Collections.Generic;


namespace K10
{
	namespace EditorGUIExtention
	{
		public class GuiLabelWidthManager
		{
			[Blackboard] static List<float> _widths = new List<float>();

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
	}
}