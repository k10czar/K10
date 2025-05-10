
using System.Diagnostics;
using System.Runtime.CompilerServices;
using K10;

public static class StopwatchPool
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Stopwatch RequestStarted()
	{
		ObjectPool.Request<Stopwatch>( out var stopwatch );
		stopwatch.Restart();
		return stopwatch;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double ReturnAndGetElapsedMs( Stopwatch stopwatch)
	{
		stopwatch.ReturnToPool();
		return stopwatch.Elapsed.TotalMilliseconds;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double ReturnToPoolAndGetElapsedMs( this Stopwatch stopwatch)
	{
		stopwatch.ReturnToPool();
		return stopwatch.Elapsed.TotalMilliseconds;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ReturnToPool( this Stopwatch stopwatch)
	{
		stopwatch.Stop();
		ObjectPool.Return( stopwatch );
	}
}
