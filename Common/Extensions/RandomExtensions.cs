using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class RandomExtensions
{
	[MethodImpl( Optimizations.INLINE_IF_CAN )] 
	public static List<T> Scrambled<T>( this IEnumerable<T> collection )
	{
		var olist = new List<T>( collection );
		var retList = new List<T>();

		while( olist.Count > 0 )
			// retList.Add( olist.RandomPop() );
		{
			var id = K10Random.Less( olist.Count );
			var element = olist[id];
			retList.Add( element );
			olist.RemoveAt( id );
		}

		return retList;
	}
	
	public static ulong NextULong(this System.Random random)
	{
		uint lower = (uint)Random.Range(uint.MinValue, uint.MaxValue);
		uint upper = (uint)Random.Range(uint.MinValue, uint.MaxValue);
		ulong randomULong = ((ulong)upper << 31) | lower;
		return randomULong;
	}

	public static long NextLong(this System.Random random)
	{
		uint lower = (uint)Random.Range(uint.MinValue, uint.MaxValue);
		uint upper = (uint)Random.Range(uint.MinValue, uint.MaxValue);
		long randomULong = ((long)upper << 32) | lower;
		return randomULong;
	}
}
