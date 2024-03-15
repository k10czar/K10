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
	const int CLEAR_FRAMES_BATCH_SIZE = 300;
	static int firstSampleFrame = 0;

	static bool enabled = false;

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

	public static void LogStart( string tag )
	{
		if( !enabled ) return;
		LogEnd( tag );

		var sw = new Stopwatch();
		_watches[tag] = sw;
		sw.Start();
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
			if (!lastFrameFunctionTimes.ContainsKey(kvp.Key))
				continue;
			
			var functionTime = lastFrameFunctionTimes[kvp.Key];
				
			SB.Append( "[" );
			var tag = kvp.Key;
			SB.Append( functionTime.Calls );
			SB.Append( "]" );
			SB.Append( tag );
			SB.Append( ":" );

			var ms = functionTime.AccumulatedTimings;
			SB.Append( ms.ToString( msFormat ) );
			SB.Append( "ms" );

			if( hasLastLogStopwatch )
			{
				SB.Append( " " );
				SB.Append( ( ms * 100 / totalMs ).ToString( percentageFormat ) );
				SB.Append( "%" );
			}

			SB.Append( "\tAvg: " );
			SB.Append( "[" );
			SB.Append( Mathf.Round((float)kvp.Value.Calls / nFramesInSample) );
			SB.Append( "]" );
			SB.Append( " " );

			ms = kvp.Value.AccumulatedTimings / nFramesInSample ;
			SB.Append( ms.ToString( msFormat ) );
			SB.Append( "ms" );

			ms = kvp.Value.AccumulatedTimings;
			SB.Append( " " );
			SB.Append( ( ms * 100 / totalSampleMs ).ToString( percentageFormat ) );
			SB.Append( "%" );

			SB.Append( "\n" );
		}
		var log = SB.ToString();
		SB.Clear();

		_logStopwatch.Reset();
		_logStopwatch.Start();

		return log;
	}
}
