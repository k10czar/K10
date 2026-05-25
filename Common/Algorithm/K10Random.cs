using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class K10Random
{
	public static bool Bool { get { return ( Value < .5f ); } }

	public static float Value
	{
		get
		{
			var val = Random.value;
			Random.InitState( Random.Range( int.MinValue, int.MaxValue ) );
			return val;
		}
	}

	public static int Hash
	{
		get
		{
			var val = Random.Range( 1, int.MaxValue );
			Random.InitState( Random.Range( int.MinValue, int.MaxValue ) );
			return val;
		}
	}

	public static bool Chance( float chance )
	{
		var v = Value;
		return v < chance;
	}

	public static int Less( int max )
	{
		var val = Random.Range( 0, max );
		Random.InitState( Random.Range( int.MinValue, int.MaxValue ) );
		return val;
	}

	public static int Interval( int min, int exclusiveMax ) { return K10Random.Less( exclusiveMax - min ) + min; }
	public static float FloatInterval( float min, float max ) { return Value * ( max - min ) + min; }

	public static int Exponential( int max, int power )
	{
		var val = Random.Range( 0, (int)Mathf.Pow( max, power ) );
		val = (int)Mathf.Pow( val, 1f / power );
		Random.InitState( Random.Range( int.MinValue, int.MaxValue ) );
		return val;
	}

	public static int Id( int count, LinkedList<int> sortedIgnore )
	{
		var rnd = Less( count - sortedIgnore.Count );

		var it = sortedIgnore.First;
		while( it != null && rnd >= it.Value )
		{
			rnd++;
			it = it.Next;
		}

		return rnd;
	}

	public static T RandomPop<T>(this IList<T> list)
	{
		var count = list.Count;
		if (count == 0)
			throw new System.IndexOutOfRangeException("Cannot take random element from an empty list");

		var id = Less(count);
		var element = list[id];
		list.RemoveAt(id);
		return element;
	}
}
