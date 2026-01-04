using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GuiColorManager
{
	static List<Color> _colors = new List<Color>();

	public static void New( Color color )
	{
		_colors.Add( GUI.color );
		GUI.color = color;
		GUI.contentColor = color;
	}

    public static void Revert( int count = 1 )
	{
		if( count <= 0 ) return;
		var len = _colors.Count;
		
		if( len >= count )
		{
			var firstToRemoveID = len - count;
			var candidateColor = _colors[firstToRemoveID];
			GUI.color = candidateColor;
			GUI.contentColor = candidateColor;
			_colors.RemoveRange( firstToRemoveID, count );
		}
		else
		{
			_colors.Clear();
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

	public static void Revert( int count = 1 )
	{
		if( count <= 0 ) return;
		var len = _colors.Count;
		
		if( len >= count )
		{
			var firstToRemoveID = len - count;
			Gizmos.color = _colors[firstToRemoveID];
			_colors.RemoveRange( firstToRemoveID, count );
		}
		else
		{
			_colors.Clear();
			Gizmos.color = Color.white;
		}
	}
}

public class GuiBackgroundColorManager
{
	static List<Color> _colors = new List<Color>();

	public static void New( Color color )
	{
		_colors.Add( GUI.color );
		GUI.backgroundColor = color;
	}

	public static void Revert( int count = 1 )
	{
		if( count <= 0 ) return;
		var len = _colors.Count;
		
		if( len >= count )
		{
			var firstToRemoveID = len - count;
			GUI.backgroundColor = _colors[firstToRemoveID];
			_colors.RemoveRange( firstToRemoveID, count );
		}
		else
		{
			_colors.Clear();
			GUI.backgroundColor = Color.white;
		}
	}
}