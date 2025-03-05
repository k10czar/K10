using System;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
	public static T GetClamped<T>(this IList<T> list, int index) => list[Mathf.Min(index, list.Count - 1)];

	public static T GetClampedOrDefault<T>(this IList<T> list, int index)
	{
		if (list.Count == 0) return default;
		return list[Mathf.Min(index, list.Count - 1)];
	}

	public static T GetLooping<T>(this IList<T> list, int index) => list[index % list.Count];
	public static T GetLooping<T,U>(this IList<T> list, U id) where U : Enum
		=> list[(int)(object) id % list.Count];

	public static void Swap<T>(this IList<T> list, int index1, int index2)
    {
		var temp = list[index1];
		list[index1] = list[index2];
		list[index2] = temp;
	}
}
