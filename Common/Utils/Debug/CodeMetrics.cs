#define LOG_ALL_METRICS
#define LOG_REPORT_ON_QUIT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using K10;
using UnityEngine;

public static class CodeMetrics
{
    private static bool _inited;
#if LOG_ALL_METRICS || LOG_REPORT_ON_QUIT
	static Dictionary<string,Stopwatch> _currentRunningMetrics = new();
#endif
#if LOG_REPORT_ON_QUIT
	static Dictionary<string,double> logReport = new();
	static Dictionary<string,int> callsReport = new();
	static double totalTime;
	static int tracks;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
	{
		_inited = false;
#if LOG_REPORT_ON_QUIT
		if( ApplicationEventsRelay.HasInstance ) ApplicationEventsRelay.OnQuit.Unregister( LogAndClear );
		logReport.Clear();
		callsReport.Clear();
		totalTime = 0;
		tracks = 0;
#endif
#if LOG_ALL_METRICS || LOG_REPORT_ON_QUIT
		_currentRunningMetrics.Clear();
#endif
	}

	public static void Log()
	{
#if LOG_REPORT_ON_QUIT
		var sb = StringBuilderPool.RequestEmpty();
		sb.AppendLine( $"<color=#0080FF>CodeMetrics</color> tracked <color=#FF69B4>{tracks}</color> calls that took <color=#DAA520>{ValueToString(totalTime)}ms</color>" );
		foreach( var log in logReport.OrderBy( log => -log.Value ) )
		{
			var code = log.Key;
			var ms = log.Value;
			sb.Append( $"<color=#FF69B4>{code}</color> took <color=#DAA520>{ValueToString(ms)}ms</color>" );
			callsReport.TryGetValue( code,out var calls );
			sb.AppendLine( calls > 1 ? $" in {calls} call(s) averaging:<color=#DAA520>{ValueToString(ms/calls)}ms</color>" : "" );
		}
		UnityEngine.Debug.Log( sb.ReturnToPoolAndCast() );
#endif
	}

    static string ValueToString(double ms) => ms > 10 ? $"{ms:N0}" : ( ms > 1 ? $"{ms:N1}" : ( ms > .1 ? $"{ms:N2}" : ( ms > .01 ? $"{ms:N3}" : $"{ms:N6}" ) ) );

    static void LogAndClear()
	{
		Log();
		Clear();
	}

    private static void Clear()
    {
#if LOG_REPORT_ON_QUIT
		logReport.Clear();
		totalTime = 0;
		tracks = 0;
#endif
    }

    public static void Start( string code )
	{
		TryInit();
#if LOG_ALL_METRICS || LOG_REPORT_ON_QUIT
		_currentRunningMetrics[code] = StopwatchPool.RequestStarted();
#endif
	}

    private static void TryInit()
    {
		if( _inited ) return;
		if( !Application.isPlaying ) return;
		_inited = true;
#if LOG_REPORT_ON_QUIT
		ApplicationEventsRelay.OnQuit.Register( LogAndClear );
		UnityEngine.Debug.Log( "Registered <color=#0080FF>CodeMetrics</color> Log when <color=#FF69B4>ApplcationQuit</color>" );
#endif
    }


    public static void Finish( string code, string newName = null )
	{
#if LOG_ALL_METRICS || LOG_REPORT_ON_QUIT
		if( !_currentRunningMetrics.TryGetValue( code, out var sw ) ) return;
		var ms = sw.ReturnToPoolAndElapsedMs();
		_currentRunningMetrics.Remove( code );
		var codeToUse = string.IsNullOrEmpty(newName) ? code : newName;
#endif
#if LOG_ALL_METRICS 
		var logMessage = $"<color=#0080FF>CodeMetrics</color>: <color=#FF69B4>{codeToUse}</color> took <color=#DAA520>{ValueToString(ms)}ms</color>";
		UnityEngine.Debug.Log( logMessage );
#endif
#if LOG_REPORT_ON_QUIT
		tracks++;
		totalTime += ms;
		logReport.TryGetValue( codeToUse, out var codeTimeAcc );
		logReport[codeToUse] = codeTimeAcc + ms;
		callsReport.TryGetValue( codeToUse, out var calls );
		callsReport[codeToUse] = calls + 1;
#endif
	}
}
