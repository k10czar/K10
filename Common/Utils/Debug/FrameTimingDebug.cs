using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class FrameTimingDebug
{
	class FunctionTime
	{
		public double AccumulatedTimings { get; private set; }
		public int Calls { get; private set;}

		public void Accumulate(double time, int calls)
		{
			AccumulatedTimings += time;
			Calls += calls;
		}
	}

	class FrameData
	{
		public string Tag { get; }
		public int FrameNumber { get; }
		public float StartTime { get; }
		public float UnscaledDeltaTime { get; }
		public double AccumulatedTimings { get; private set; }
		public int Calls { get; private set; }
		public List<FrameData> Childs { get; } = new();

		// Root frame node
		public FrameData(int frameNumber, float startTime, float unscaledDeltaTime)
		{
			FrameNumber = frameNumber;
			StartTime = startTime;
			UnscaledDeltaTime = unscaledDeltaTime;
		}

		// Function entry node
		public FrameData(string tag) { Tag = tag; }

		public void AddTime(double time) { AccumulatedTimings += time; Calls++; }

		public FrameData GetOrAddChild(string tag)
		{
			foreach (var child in Childs)
				if (child.Tag == tag) return child;
			var newChild = new FrameData(tag);
			Childs.Add(newChild);
			return newChild;
		}

		public void AccumulateInto(Dictionary<string, FunctionTime> dict)
		{
			foreach (var child in Childs)
			{
				if (!dict.TryGetValue(child.Tag, out var ft))
				{
					ft = new FunctionTime();
					dict[child.Tag] = ft;
				}
				ft.Accumulate(child.AccumulatedTimings, child.Calls);
				child.AccumulateInto(dict);
			}
		}
	}

	const float AVERAGE_SAMPLE_TIME = 3f;
	const float HIGH_FRAME_PERCENTAGE_WARNING = 3f;
	const float SPIKE_MULTIPLIER_WARNING = 3f;
	const int CLEAR_FRAMES_BATCH_SIZE = 300;
	static int firstSampleFrame = 0;

	static bool enabled = false;
	static bool deep = false;

	[Conditional(ConstsK10.CODE_METRICS_CONDITIONAL)]
    public static void ToogleDeep() => deep = !deep;

	[Conditional(ConstsK10.CODE_METRICS_CONDITIONAL)]
    public static void Enable()
	{
		// #if UNITY_EDITOR || CHEATS_ENABLED
		enabled = true;
		// #else
		// enabled = false;
		// #endif
	}

	[Conditional(ConstsK10.CODE_METRICS_CONDITIONAL)]
	public static void Disable()
	{
		if( !enabled ) return;
		enabled = false;
		Clear();
		_logStopwatch.Stop();
	}

	private static readonly Dictionary<string, Stopwatch> _watches = new Dictionary<string, Stopwatch>();
	private static int _callStatckFrame = -1;
	private static readonly List<string> _callStack = new List<string>();
	private static readonly List<int> _stackSizeStack = new List<int>(); // parallel to _callStatck: stores _callStatck.Count at the time of each LogStart
	private static readonly List<FrameData> _framesData = new List<FrameData>();

	private static Stopwatch _logStopwatch = new Stopwatch();

	[Conditional(ConstsK10.CODE_METRICS_CONDITIONAL)]
	public static void LogDeepStart( string tag )
	{
		if( !enabled ) return;
		if( !deep ) return;
		LogStart( tag );
	}

	[Conditional(ConstsK10.CODE_METRICS_CONDITIONAL)]
	public static void LogStart( string tag )
	{
		if( !enabled ) return;
		LogEnd( tag );

		var sw = StopwatchPool.RequestStarted();
		_watches[tag] = sw;

		var frameId = Time.frameCount;
		if( _callStatckFrame != frameId )
		{
			_callStack.Clear();
			_stackSizeStack.Clear();
			_callStatckFrame = frameId;
		}

		_stackSizeStack.Add( _callStack.Count ); // record depth before pushing
		_callStack.Add( tag );
	}

	[Conditional(ConstsK10.CODE_METRICS_CONDITIONAL)]
	public static void LogDeepEnd( string tag )
	{
		if( !enabled ) return;
		if( !deep ) return;
		LogEnd( tag );
	}

	[Conditional(ConstsK10.CODE_METRICS_CONDITIONAL)]
	public static void LogEnd( string tag )
	{
		if( !enabled ) return;

		var stackSize = _callStack.Count;
		if( stackSize == 0 ) return;

		var stackIdx = stackSize - 1;
		var stackIsRight = stackSize > 0 && tag == _callStack[stackIdx];
		if( !stackIsRight ) stackIdx = _callStack.LastIndexOf( tag );

		if( stackIdx == -1 ) return;
		_callStack.RemoveAt( stackIdx );
		
		var startSize = _stackSizeStack[stackIdx];
		_stackSizeStack.RemoveAt( stackIdx );

		if( !_watches.TryGetValue( tag, out var osw ) ) return;
		if( osw.IsRunning ) osw.Stop();
		_watches.Remove( tag );

		int currentFrame = Time.frameCount;
		if( _framesData.Count == 0 || _framesData[^1].FrameNumber != currentFrame )
			_framesData.Add( new FrameData( currentFrame, Time.time, Time.unscaledDeltaTime ) );

		// Navigate to the correct parent depth using ancestor tags stored at LogStart time.
		// GetOrAddChild searches existing Childs first, so repeated calls accumulate on the same node.
		var node = _framesData[^1];
		for( int i = 0; i < startSize && i < _callStack.Count; i++ )
			node = node.GetOrAddChild( _callStack[i] );

		node.GetOrAddChild( tag ).AddTime( osw.ReturnToPoolAndGetElapsedMs() );
	}

	[Conditional(ConstsK10.CODE_METRICS_CONDITIONAL)]
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

	static void AppendEntry( FrameData node, Dictionary<string, FunctionTime> sampleTimes,
		int nFramesInSample, double totalMs, bool hasLastLogStopwatch, string msFormat, int depth )
	{
		var indent = depth > 1 ? new string( ' ', ( depth - 1 ) * 4 ) : string.Empty;
		if( depth > 0 ) indent += "└-- ";
		var ms = node.AccumulatedTimings;

		double averageMs = 0;
		if( sampleTimes.TryGetValue( node.Tag, out var sampleFt ) )
			averageMs = sampleFt.AccumulatedTimings / nFramesInSample;

		var msString = ms.ToString( msFormat );
		if( ms > averageMs * SPIKE_MULTIPLIER_WARNING )
			msString = msString.Colorfy( Colors.OrangeRed );

		SB.Append( indent );
		SB.Append( msString );
		SB.Append( "ms" );

		if( hasLastLogStopwatch && totalMs > 0 )
		{
			var percentage = ms * 100 / totalMs;
			string percentageString;
			if( percentage >= 10 ) percentageString = percentage.ToString( "F0" );
			else if( percentage >= 0.1 ) percentageString = percentage.ToString( "F1" );
			else percentageString = percentage.ToString( "F2" ).TrimStart( '0' );
			if( percentage > HIGH_FRAME_PERCENTAGE_WARNING )
				percentageString = percentageString.Colorfy( Colors.Crimson );

			SB.Append( " " );
			SB.Append( percentageString );
			SB.Append( "%" );
		}

		SB.Append( "\t[" );
		SB.Append( node.Calls );
		SB.Append( "]\t" );
		SB.Append( node.Tag );
		SB.Append( "\n" );

		node.Childs.Sort( ( a, b ) => b.AccumulatedTimings.CompareTo( a.AccumulatedTimings ) );
		foreach( var child in node.Childs )
			AppendEntry( child, sampleTimes, nFramesInSample, totalMs, hasLastLogStopwatch, msFormat, depth + 1 );
	}

	public static string GetLog( string msFormat = "F3", string percentageFormat = "F1" )
	{
		var hasLastLogStopwatch = _logStopwatch.IsRunning;
		var totalMs = _logStopwatch.Elapsed.TotalMilliseconds;
		SB.Clear();

		if( hasLastLogStopwatch )
		{
			_logStopwatch.Stop();
			SB.Append( totalMs.ToString( msFormat ) );
			SB.Append( "ms\n" );
		}

		if (_framesData.Count > 0)
		{
			SB.AppendLine( "" );
			float totalSampleTime = 0;
			var sampleTimes = new Dictionary<string, FunctionTime>();

			for (int i = _framesData.Count - 1; i >= 0 && i >= firstSampleFrame; i--)
			{
				var frameInfo = _framesData[i];
				var sampleMinTime = Time.time - AVERAGE_SAMPLE_TIME;

				if (frameInfo.StartTime < sampleMinTime)
				{
					firstSampleFrame = i + 1;
					break;
				}

				frameInfo.AccumulateInto( sampleTimes );
				totalSampleTime += frameInfo.UnscaledDeltaTime;
			}

			int nFramesInSample = _framesData.Count - firstSampleFrame;
			var lastFrame = _framesData[^1];

			lastFrame.Childs.Sort( ( a, b ) => b.AccumulatedTimings.CompareTo( a.AccumulatedTimings ) );
			foreach( var root in lastFrame.Childs )
				AppendEntry( root, sampleTimes, nFramesInSample, totalMs, hasLastLogStopwatch, msFormat, 0 );
		}

		var log = SB.ToString();
		SB.Clear();

		_logStopwatch.Reset();
		_logStopwatch.Start();

		return log;
	}
}
