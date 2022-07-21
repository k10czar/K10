using UnityEngine;
using System.Collections.Generic;
using System;

public class ActionListReferenceHolder : MonoBehaviour
{
	[SerializeField] List<string> _debug = new List<string>();
	[SerializeField] List<Action> _objects = new List<Action>();

	public void AddRef( Action obj )
	{
		_objects.Add( obj );
		_debug.Add( obj.ToStringOrNull() );
	}

	void Update()
	{
		for( int i = _debug.Count; i < _objects.Count; i++ ) _debug.Add( string.Empty );
		for( int i = _debug.Count - 1; i >= _objects.Count; i++ ) _debug.RemoveAt( i );
		for( int i = 0; i < _objects.Count; i++ )
		{
			_debug[i] = _objects[i].ToStringOrNull();
		}
	}
}
