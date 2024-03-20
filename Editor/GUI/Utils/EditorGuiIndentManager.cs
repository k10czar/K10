using System.Collections.Generic;
using UnityEditor;
namespace K10
{
	namespace EditorGUIExtention
	{
		public class EditorGuiIndentManager
		{
			[Blackboard] static List<int> _widths = new List<int>();

			public static void New( int indent )
			{
				_widths.Add( EditorGUI.indentLevel );
				EditorGUI.indentLevel = indent;
			}

			public static void Revert()
			{
				if( _widths.Count > 0 )
				{
					EditorGUI.indentLevel = _widths[_widths.Count - 1];
					_widths.RemoveAt( _widths.Count - 1 );
				}
			}
		}

		public class FakeIndentManager
		{
			[Blackboard] static List<int> _widths = new List<int>();

			public static UnityEngine.Rect New( UnityEngine.Rect area, float widthOffset = 0 )
			{
				var indentedArea = EditorGUI.IndentedRect( area );
				EditorGuiIndentManager.New( 0 );
				GuiLabelWidthManager.New( EditorGUIUtility.labelWidth + widthOffset - ( ( indentedArea.x - area.x ) ) );
				return indentedArea;
			}

			public static void Revert()
			{
				GuiLabelWidthManager.Revert();
				EditorGuiIndentManager.Revert();
			}
		}
	}
}
