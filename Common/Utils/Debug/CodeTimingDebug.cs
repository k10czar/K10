using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

public static class CodeTimingDebug
{
	class FunctionTime
	{
		public double AccumulatedTimings { get; private set; }
		public int Calls { get; private set;}

		public void AddTime(double time)
		{
			AccumulatedTimings += time;
			Calls++;
		}

		public void CombineWithFunctionTime(FunctionTime functionTime)
		{
			AccumulatedTimings += functionTime.AccumulatedTimings;
			Calls += functionTime.Calls;
		}
	}
	class FrameData
	{
		public int FrameNumber { get; }
		public float StartTime { get; }
		public float UnscaledDeltaTime { get; }
		public Dictionary<string, FunctionTime> FunctionTimes;

		public FrameData(int frameNumber, float startTime, float unscaledDeltaTime)
		{
			this.FrameNumber = frameNumber;
			this.StartTime = startTime;
			this.UnscaledDeltaTime = unscaledDeltaTime;
			FunctionTimes = new Dictionary<string, FunctionTime>();
		}

		public void AddTimingData(string tag, double time)
		{
			if (!FunctionTimes.ContainsKey(tag)) 
				FunctionTimes.Add(tag, new FunctionTime());

			FunctionTimes[tag].AddTime(time);
		}
    }

	const float AVERAGE_SAMPLE_TIME = 3f;
	const float HIGH_FRAME_PERCENTAGE_WARNING = 3f;
	const float SPIKE_MULTIPLIER_WARNING = 3f;
	const int CLEAR_FRAMES_BATCH_SIZE = 300;
	static int firstSampleFrame = 0;

	static bool enabled = false;
	static bool deep = false;

    public static void ToogleDeep() => deep = !deep;

    public static void Enable() 
	{ 
		#if UNITY_EDITOR || CHEATS_ENABLED
		enabled = true; 
		#else
		enabled = false;
		#endif
	}
	public static void Disable() 
	{ 
		if( !enabled ) return;
		enabled = false;
		Clear();
		_logStopwatch.Stop();
	}

	private static readonly Dictionary<string, Stopwatch> _watches = new Dictionary<string, Stopwatch>();
	private static readonly List<FrameData> _framesData = new List<FrameData>();



	private static Stopwatch _logStopwatch = new Stopwatch();
	
	public static void LogDeepStart( string tag )
	{
		if( !enabled ) return;
		if( !deep ) return;
		LogStart( tag );
	}

	public static void LogStart( string tag )
	{
		if( !enabled ) return;
		LogEnd( tag );

		var sw = new Stopwatch();
		_watches[tag] = sw;
		sw.Start();
	}
	
	public static void LogDeepEnd( string tag )
	{
		if( !enabled ) return;
		if( !deep ) return;
		LogEnd( tag );
	}

	public static double LogEnd( string tag )
	{
		if( !enabled ) return 0;

		if( !_watches.TryGetValue( tag, out var osw ) ) return 0;
		if( osw.IsRunning ) osw.Stop();
		_watches.Remove( tag );
		

		int currentFrame = Time.frameCount;
		if (_framesData.Count == 0 || _framesData[_framesData.Count - 1].FrameNumber != currentFrame)
		{

			var frame = new FrameData(currentFrame, Time.time, Time.unscaledDeltaTime);
			_framesData.Add(frame);
		}

		var frameData = _framesData[_framesData.Count - 1];

		var elapsed = osw.Elapsed.TotalMilliseconds;
		frameData.AddTimingData(tag, elapsed);

		return elapsed;
	}

	public static void ClearUnusedData()
	{
		_watches.Clear();
		if (_framesData.Count == 0)
			return;

		if (firstSampleFrame < CLEAR_FRAMES_BATCH_SIZE)
			return;

		_framesData.RemoveRange(0, firstSampleFrame);
		firstSampleFrame = 0;
	}

	public static void Clear()
	{
		_watches.Clear();
		_framesData.Clear();
	}

	private static readonly StringBuilder SB = new StringBuilder();
	private static System.Comparison<KeyValuePair<string, FunctionTime>> DESCENDING_COMPARISON = ( KeyValuePair<string, FunctionTime> a, KeyValuePair<string, FunctionTime> b ) => b.Value.AccumulatedTimings.CompareTo( a.Value.AccumulatedTimings );

	public static string GetLog( string msFormat = "F3", string percentageFormat = "F1" )
	{
		if (_framesData.Count == 0) 
			return string.Empty;

		float totalSampleTime = 0;
		Dictionary<string, FunctionTime> functionTimesInSample = new Dictionary<string, FunctionTime>();
		for (int i = _framesData.Count - 1; i >= 0 && i >= firstSampleFrame; i--)
		{
			var frameInfo = _framesData[i];
			var sampleMinTime = Time.time - AVERAGE_SAMPLE_TIME;

			if (frameInfo.StartTime < sampleMinTime)
			{
				firstSampleFrame = i + 1;
				break;
			}

			foreach (var functionTimeInFrame in frameInfo.FunctionTimes)
			{
				if (!functionTimesInSample.ContainsKey(functionTimeInFrame.Key))
					functionTimesInSample.Add(functionTimeInFrame.Key, new FunctionTime());

				functionTimesInSample[functionTimeInFrame.Key].CombineWithFunctionTime(functionTimeInFrame.Value);
			}

			totalSampleTime += frameInfo.UnscaledDeltaTime;
		}
		var totalSampleMs = totalSampleTime * 1000f;

		var timings = functionTimesInSample.ToList();
		timings.Sort( DESCENDING_COMPARISON );

		var hasLastLogStopwatch = _logStopwatch.IsRunning;
		var totalMs = _logStopwatch.Elapsed.TotalMilliseconds;
		SB.Clear();
		
		if( hasLastLogStopwatch )
		{
			_logStopwatch.Stop();
			SB.Append( "Total time: " );
			SB.Append( totalMs.ToString( msFormat ) );
			SB.Append( "ms\n" );
		}

		var lastFrameFunctionTimes = _framesData[_framesData.Count - 1].FunctionTimes;

		int nFramesInSample = _framesData.Count - firstSampleFrame;
		foreach( var kvp in timings )
		{
			var tag = kvp.Key;
			double ms = 0;
			var averageMs = kvp.Value.AccumulatedTimings / nFramesInSample ;

			if (lastFrameFunctionTimes.ContainsKey(tag))
			{
				var functionTime = lastFrameFunctionTimes[tag];

				SB.Append( "Frame: [" );
				SB.Append( functionTime.Calls );
				SB.Append( "] " );

				ms = functionTime.AccumulatedTimings;
				var msString = ms.ToString( msFormat );
				if (ms > averageMs * SPIKE_MULTIPLIER_WARNING)
					msString = msString.Colorfy(Colors.OrangeRed);

				SB.Append( msString );
				SB.Append( "ms" );

				if( hasLastLogStopwatch )
				{
					SB.Append( " " );
					var percentage = ( ms * 100 / totalMs );
					var percentageString = percentage.ToString( percentageFormat );
					if (percentage > HIGH_FRAME_PERCENTAGE_WARNING)
						percentageString = percentageString.Colorfy(Colors.Crimson);

					SB.Append( " " );
					SB.Append( percentageString );
					SB.Append( "% " );
				}
			}
			else
			{
				// SB.Append("Frame: [0] 0.000ms 0.0%");
				SB.Append("Frame: ------------------------");
			}

			SB.Append( "\t" );
				
			SB.Append( "Avg: [");
			SB.Append( Mathf.Round((float)kvp.Value.Calls / nFramesInSample) );
			SB.Append( "] " );
			
			SB.Append( "Avg: [");
			SB.Append( Mathf.Round((float)kvp.Value.Calls / nFramesInSample) );
			SB.Append( "] " );

			SB.Append( averageMs.ToString( msFormat ) );
			SB.Append( "ms " );

			var avg = averageMs;

			averageMs = kvp.Value.AccumulatedTimings;
			SB.Append( ( averageMs * 100 / totalSampleMs ).ToString( percentageFormat ) );
			SB.Append( "% " );
			
			if( avg > MathAdapter.EP2 ) 
			{
				var pFps = Mathf.RoundToInt( (float)( 1000f / avg ) );
				if( pFps < 100000 ) SB.Append( "  " );
				if( pFps < 10000 ) SB.Append( "  " );
				if( pFps < 1000 ) SB.Append( "  " );
				if( pFps < 100 ) SB.Append( "  " );
				SB.Append( (1000f / avg).ToString( "N0" ) );
			}
			else SB.Append( "      ∞" );
			SB.Append( "pfps " );

			SB.Append( "\t" );
			SB.Append( tag );

			SB.Append( "\n" );
		}

		var log = SB.ToString();
		SB.Clear();

		_logStopwatch.Reset();
		_logStopwatch.Start();

		return log;
	}
}
