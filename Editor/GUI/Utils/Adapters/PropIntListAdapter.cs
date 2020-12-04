using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;

public class PropIntListAdapter : PropReadOnlyIntListAdapter, IList<int>
{
	public bool IsReadOnly => false;

	int IList<int>.this[int index]
	{
		get => _property.GetArrayElementAtIndex( index ).intValue;
		set => _property.GetArrayElementAtIndex( index ).intValue = value;
	}

	public PropIntListAdapter( SerializedProperty prop ) : base( prop ) { }

	public void Add( int item )
	{
		var id = Count;
		_property.InsertArrayElementAtIndex( id );
		_property.GetArrayElementAtIndex( id ).intValue = item;
	}

	public void Clear() { _property.ClearArray(); }

	public bool Contains( int item )
	{
		for( int i = Count - 1; i >= 0; i-- ) if( this[i] == item ) return true;
		return false;
	}

	public void CopyTo( int[] array, int arrayIndex )
	{
		for( int i = 0; i < Count; i++ ) array[arrayIndex++] = this[i];
	}

	public int IndexOf( int item )
	{
		for( int i = Count - 1; i >= 0; i-- ) if( this[i] == item ) return i;
		return -1;
	}

	public void Insert( int index, int item )
	{
		_property.InsertArrayElementAtIndex( index );
		_property.GetArrayElementAtIndex( index ).intValue = item;
	}

	public bool Remove( int item )
	{
		bool removed = false;
		for( int i = Count - 1; i >= 0; i-- )
		{
			if( this[i] != item ) continue;
			_property.DeleteArrayElementAtIndex( i );
			removed = true;
		}
		return removed;
	}

	public void RemoveAt( int index ) { _property.DeleteArrayElementAtIndex( index ); }
}
