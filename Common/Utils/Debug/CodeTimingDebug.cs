using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public static class CodeTimingDebug
{
	static bool enabled = false;

	public static void Enable() { enabled = true; }
	public static void Disable() { enabled = false; }

	private static readonly Dictionary<string,Stopwatch> _watches = new Dictionary<string, Stopwatch>();
	private static readonly Dictionary<string,double> _accTimmings = new Dictionary<string,double>();
	private static readonly Dictionary<string,int> _calls = new Dictionary<string,int>();

	public static void LogStart( string tag )
	{
		if( !enabled ) return;
		LogEnd( tag );

		var sw = new Stopwatch();
		sw.Start();
		_watches[tag] = sw;
	}

	public static double LogEnd( string tag )
	{
		if( !_watches.TryGetValue( tag, out var osw ) ) return 0;
		_watches.Remove( tag );
		
		if( osw.IsRunning ) osw.Stop();

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
	public static string GetLog( string format = "F3" )
	{
		var timings = _accTimmings.ToList();
		timings.Sort( DESCENDING_COMPARISON );
		SB.Clear();
		foreach( var kvp in timings )
		{
			SB.Append( "[" );
			var tag = kvp.Key;
			if( _calls.TryGetValue( tag, out var calls ) ) SB.Append( calls );
			SB.Append( "]" );
			SB.Append( tag );
			SB.Append( ":" );
			SB.Append( kvp.Value.ToString( format ) );
			SB.Append( "ms" );
			SB.Append( "\n" );
		}
		var log = SB.ToString();
		SB.Clear();
		return log;
	}
}
