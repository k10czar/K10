using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuiColorManager
{
	static List<Color> _colors = new List<Color>();

	public static void New( Color color )
	{
		_colors.Add( GUI.color );
		GUI.color = color;
		GUI.contentColor = color;
	}

	public static void Revert()
	{
		if( _colors.Count > 0 )
		{
			GUI.color = _colors[_colors.Count - 1];
			GUI.contentColor = GUI.color;
			_colors.RemoveAt( _colors.Count - 1 );
		}
		else
		{
			GUI.color = Color.white;
			GUI.contentColor = Color.white;
		}
	}
}


public class GizmosColorManager
{
	static List<Color> _colors = new List<Color>();

	public static void New( Color color )
	{
		_colors.Add( GUI.color );
		Gizmos.color = color;
	}

	public static void Revert()
	{
		if( _colors.Count > 0 )
		{
			Gizmos.color = _colors[_colors.Count - 1];
			_colors.RemoveAt( _colors.Count - 1 );
		}
		else
		{
			Gizmos.color = Color.white;
		}
	}
}