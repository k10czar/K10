#define LOG_ALL_METRICS
#define LOG_REPORT_ON_SUSPEND
#define LOG_REPORT_PARTIAL
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public static class CodeMetrics
{
    private static bool _inited;
#if LOG_ALL_METRICS || LOG_REPORT_ON_SUSPEND
	static Dictionary<string,Stopwatch> _currentRunningMetrics = new();
#endif
	static CodeMetricsReport _fullReport = new();
	static CodeMetricsReport _partialReport = new();

	public static ICodeMetricsReport FullReport => _fullReport;
	public static ICodeMetricsPartialReport PartialReport => _partialReport;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
	{
		_inited = false;
#if LOG_REPORT_ON_SUSPEND
		ApplicationEventsRelay.IsSuspended.RegisterOnTrue( _fullReport.DebugLog );
#endif
		_fullReport.Clear();
		_partialReport.Clear();
#if LOG_ALL_METRICS || LOG_REPORT_ON_SUSPEND
		_currentRunningMetrics.Clear();
#endif
	}

    static string ValueToString(double ms) => ms > 10 ? $"{ms:N0}" : ( ms > 1 ? $"{ms:N1}" : ( ms > .1 ? $"{ms:N2}" : ( ms > .01 ? $"{ms:N3}" : $"{ms:N6}" ) ) );

    public static void Start( string code )
	{
		TryInit();
#if LOG_ALL_METRICS || LOG_REPORT_ON_SUSPEND
		_currentRunningMetrics[code] = StopwatchPool.RequestStarted();
#endif
	}

    private static void TryInit()
    {
		if( _inited ) return;
		if( !Application.isPlaying ) return;
		_inited = true;
#if LOG_REPORT_ON_SUSPEND
		ApplicationEventsRelay.IsSuspended.RegisterOnTrue( _fullReport.DebugLog );
		UnityEngine.Debug.Log( "Registered <color=#0080FF>CodeMetrics</color> Log when <color=#FF69B4>ApplcationQuit</color>" );
#endif
    }

    public static void Finish( string code, string newNameToUse = null )
	{
#if LOG_ALL_METRICS || LOG_REPORT_ON_SUSPEND
		if( !_currentRunningMetrics.TryGetValue( code, out var sw ) ) return;
		var ms = sw.ReturnToPoolAndElapsedMs();
		_currentRunningMetrics.Remove( code );
		var codeToUse = string.IsNullOrEmpty(newNameToUse) ? code : newNameToUse;
#endif
#if LOG_ALL_METRICS 
		var logMessage = $"<color=#0080FF>CodeMetrics</color>: <color=#FF69B4>{codeToUse}</color> took <color=#DAA520>{ValueToString(ms)}ms</color>";
		UnityEngine.Debug.Log( logMessage );
#endif
#if LOG_REPORT_ON_SUSPEND
		_fullReport.Add( codeToUse, ms );
#endif
#if LOG_REPORT_PARTIAL
		_partialReport.Add( codeToUse, ms );
#endif
	}

	public interface ICodeMetricsReport
	{
		string GetLogString();
		void DebugLog();
	}

	public interface ICodeMetricsPartialReport
	{
		string GetLogStringAndClear();
		void DebugLogAndClear();
	}

	private class CodeMetricsReport : ICodeMetricsReport, ICodeMetricsPartialReport
	{
		Dictionary<string,double> intervals = new();
		Dictionary<string,int> calls = new();
		double totalTime;
		int totalCalls;

		public string GetLogString()
		{
			var sb = StringBuilderPool.RequestEmpty();
			sb.AppendLine( $"<color=#0080FF>CodeMetrics</color> tracked <color=#FF69B4>{totalCalls}</color> calls that took <color=#DAA520>{ValueToString(totalTime)}ms</color>" );
			foreach( var log in intervals.OrderBy( log => -log.Value ) )
			{
				var code = log.Key;
				var ms = log.Value;
				sb.Append( $"<color=#FF69B4>{code}</color> took <color=#DAA520>{ValueToString(ms)}ms</color>" );
				calls.TryGetValue( code,out var callsCount );
				sb.AppendLine( callsCount > 1 ? $" in {callsCount} call(s) averaging:<color=#DAA520>{ValueToString(ms/callsCount)}ms</color>" : "" );
			}
			return sb.ReturnToPoolAndCast();
		}

		public void Clear()
		{
			intervals.Clear();
			calls.Clear();
			totalTime = 0;
			totalCalls = 0;
		}

		public void DebugLogAndClear()
		{
			UnityEngine.Debug.Log( GetLogStringAndClear() );
		}

        public void Add( string codeToUse, double ms )
        {
			totalCalls++;
			totalTime += ms;
			intervals.TryGetValue( codeToUse, out var codeTimeAcc );
			intervals[codeToUse] = codeTimeAcc + ms;
			calls.TryGetValue( codeToUse, out var callsCount );
			calls[codeToUse] = callsCount + 1;
        }

        public void DebugLog()
        {
			UnityEngine.Debug.Log( GetLogString() );
        }

        public string GetLogStringAndClear()
        {
			var log = GetLogString();
			Clear();
			return log;
        }
    }
}
