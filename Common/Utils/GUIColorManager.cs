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

    public static void New(object color)
    {
        throw new NotImplementedException();
    }

    public static void Revert( int count = 1 )
	{
		if( count <= 0 ) return;
		var len = _colors.Count;
		
		if( _colors.Count > 0 )
		{
			count = Mathf.Max( count, len );
			var candidate = len - count;
			var candidateColor = _colors[candidate];
			GUI.color = candidateColor;
			GUI.contentColor = candidateColor;
			_colors.RemoveRange( candidate, count );
		}
		else
		{
			GUI.color = Color.white;
			GUI.contentColor = Color.white;
			GUI.backgroundColor = Color.white;
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
		
		if( _colors.Count > 0 )
		{
			count = Mathf.Max( count, len );
			var candidate = len - count;
			Gizmos.color = _colors[candidate];
			_colors.RemoveRange( candidate, count );
		}
		else
		{
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
		
		if( _colors.Count > 0 )
		{
			count = Mathf.Max( count, len );
			var candidate = len - count;
			GUI.backgroundColor = _colors[candidate];
			_colors.RemoveRange( candidate, count );
		}
		else
		{
			GUI.backgroundColor = Color.white;
		}
	}
}