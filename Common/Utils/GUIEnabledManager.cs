using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIEnabledManager
{
	static List<bool> _enables = new List<bool>();

	public static void New( bool enabled )
	{
		_enables.Add( GUI.enabled );
		GUI.enabled = enabled;
	}

	public static void Revert()
	{
		if( _enables.Count > 0 )
		{
			GUI.enabled = _enables[_enables.Count - 1];
			_enables.RemoveAt( _enables.Count - 1 );
		}
	}
}