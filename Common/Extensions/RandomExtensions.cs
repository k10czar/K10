using UnityEngine;

public static class RandomExtensions
{
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
