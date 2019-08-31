using System.Collections.Generic;
using UnityEngine;

public static class ListExtentions
{
	public static T GetClamped<T>( this IList<T> list, int id ) => list[Mathf.Min( id, list.Count - 1 )];
}