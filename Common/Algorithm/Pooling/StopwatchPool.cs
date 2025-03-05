
using System.Diagnostics;
using K10;

public static class StopwatchPool
{
	public static Stopwatch RequestStarted()
	{
		ObjectPool.Request<Stopwatch>( out var stopwatch );
		stopwatch.Restart();
		return stopwatch;
	}

	public static double ReturnAndGetElapsedMs( Stopwatch stopwatch)
	{
		stopwatch.Stop();
		ObjectPool.Return( stopwatch );
		return stopwatch.Elapsed.TotalMilliseconds;
	}

	public static double ReturnToPoolAndElapsedMs( this Stopwatch stopwatch)
	{
		stopwatch.Stop();
		ObjectPool.Return( stopwatch );
		return stopwatch.Elapsed.TotalMilliseconds;
	}
}
