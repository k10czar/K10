using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public static class CodeTimingDebug
{
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

	private static readonly Dictionary<string,Stopwatch> _watches = new Dictionary<string, Stopwatch>();
	private static readonly Dictionary<string,double> _accTimmings = new Dictionary<string,double>();
	private static readonly Dictionary<string,int> _calls = new Dictionary<string,int>();



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
		

		var elapsed = osw.Elapsed.TotalMilliseconds;
		var ms = elapsed;
		if( _accTimmings.TryGetValue( tag, out var acc ) ) ms += acc;
		_accTimmings[tag] = ms;

		var calls = 1;
		if( _calls.TryGetValue( tag, out var oldCalls ) ) calls += oldCalls;
		_calls[tag] = calls;

		return elapsed;
	}

	public static void Clear()
	{
		_watches.Clear();
		_accTimmings.Clear();
		_calls.Clear();
	}

	private static readonly StringBuilder SB = new StringBuilder();
	private static System.Comparison<KeyValuePair<string,double>> DESCENDING_COMPARISON = ( KeyValuePair<string, double> a, KeyValuePair<string, double> b ) => b.Value.CompareTo( a.Value );
	// private static System.Comparison<KeyValuePair<string,double>> ASCENDING_COMPARISON = ( KeyValuePair<string, double> a, KeyValuePair<string, double> b ) => b.Value.CompareTo( a.Value );
	public static string GetLog( string msFormat = "F3", string percentageFormat = "F1" )
	{
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

		var timings = _accTimmings.ToList();
		timings.Sort( DESCENDING_COMPARISON );
		foreach( var kvp in timings )
		{
			SB.Append( "[" );
			var tag = kvp.Key;
			if( _calls.TryGetValue( tag, out var calls ) ) SB.Append( calls );
			SB.Append( "]" );
			SB.Append( tag );
			SB.Append( ":" );

			var ms = kvp.Value;

			SB.Append( ms.ToString( msFormat ) );
			SB.Append( "ms" );

			if( hasLastLogStopwatch )
			{
				SB.Append( " " );
				SB.Append( ( ms * 100 / totalMs ).ToString( percentageFormat ) );
				SB.Append( "%" );
			}

			SB.Append( "\n" );
		}
		var log = SB.ToString();
		SB.Clear();

		_logStopwatch.Reset();
		_logStopwatch.Start();

		return log;
	}
}
