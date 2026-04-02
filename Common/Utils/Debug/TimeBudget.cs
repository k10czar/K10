#define NOTIFY
#define COLORED
#define WITH_EMOTICONS
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TimeBudget : System.IDisposable
{
	private static readonly Queue<TimeBudget> _pool = new();
    private bool _returned = false;
	private long _timeBudget = 0;

	Stopwatch _stopwatch;

    public bool NeedSleep
    {
        get
        {
			var neddSleep = _stopwatch == null || _stopwatch.ElapsedMilliseconds > _timeBudget;
#if NOTIFY
			if( neddSleep ) 
			{
				var time = ( _stopwatch != null ) ? ( _stopwatch.ElapsedMilliseconds + "ms" ) :  "NULL";
#if COLORED
				time.Colorfy( Colors.Console.Numbers );
				var frame = Time.frameCount.ToStringColored( Colors.Console.Numbers );
#else
				var frame = Time.frameCount;
#endif
				
#if WITH_EMOTICONS && !UNITY_ANDROID && !UNITY_IOS
				NotificationConsole.Notify( $"😴 Sleep Requested after ⏰{time} at frame 🎞{frame}" );
#else
				NotificationConsole.Notify( $"Sleep Requested after {time} at frame {Time.frameCount}" );
#endif //WITH_EMOTICONS
			}
#endif //NOTIFY
            return neddSleep;
        }
    }

    public void Start()
	{
		if( _stopwatch == null ) _stopwatch = StopwatchPool.RequestStarted();
		else _stopwatch.Restart();
	}

	public static TimeBudget RentStarted( long miliseconds = 100 )
	{
		var instance = Rent();
		instance._timeBudget = miliseconds;
		instance.Start();
		return instance;
	}

    private static TimeBudget Rent()
	{
		var instance = _pool.Count > 0 ? _pool.Dequeue() : new TimeBudget();
		instance._returned = false;
		return instance;
	}

	public void Dispose()
	{
		if (_returned) return; // guard against double-return

		_returned = true;
		_stopwatch.ReturnToPool();
		_pool.Enqueue(this);
	}
}
