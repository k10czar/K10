using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
	public static T GetClamped<T>( this IList<T> list, int id ) => list[Mathf.Min( id, list.Count - 1 )];
	public static T GetClampedOrDefault<T>( this IList<T> list, int id )
	{
		if( list.Count == 0 ) return default( T );
		return list[Mathf.Min( id, list.Count - 1 )];
	}

	public static void Swap<T>(this IList<T> list, int index1, int index2)
    {
		var temp = list[index1];
		list[index1] = list[index2];
		list[index2] = temp;
	}
}
