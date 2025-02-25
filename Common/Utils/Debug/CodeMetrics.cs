#define CODE_METRICS_ENABLED
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
	static Dictionary<string,Stopwatch> _currentRunningMetrics = new();
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
		_currentRunningMetrics.Clear();
	}

	static void Log()
	{
#if LOG_REPORT_ON_QUIT
		var sb = StringBuilderPool.RequestEmpty();
		foreach( var log in logReport.OrderBy( log => -log.Value ) )
		{
			var code = log.Key;
			sb.Append( $"<color=#0080FF>CodeMetrics</color>: <color=#FF69B4>{code}</color> took <color=#DAA520>{log.Value:N0}ms</color>" );
			callsReport.TryGetValue( code,out var calls );
			sb.AppendLine( calls > 1 ? $" in {calls} call(s)" : "" );
		}
		UnityEngine.Debug.Log( $"<color=#0080FF>CodeMetrics</color> tracked <color=#FF69B4>{tracks}</color> calls that took <color=#DAA520>{totalTime:N0}ms</color>\n{sb}" );
		sb.ReturnToPool();
#endif
	}

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
#if CODE_METRICS_ENABLED
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
		UnityEngine.Debug.Log( "Registered <color=#0080FF>CodeMetrics</color> Log when ApplcationQuit" );
#endif
    }


    public static void Finish( string code )
	{
#if CODE_METRICS_ENABLED
		if( !_currentRunningMetrics.TryGetValue( code, out var sw ) ) return;
		var ms = sw.ReturnToPoolAndElapsedMs();
		var logMessage = $"<color=#0080FF>CodeMetrics</color>: <color=#FF69B4>{code}</color> took <color=#DAA520>{ms:N0}ms</color>";
		UnityEngine.Debug.Log( logMessage );
		_currentRunningMetrics.Remove( code );
#if LOG_REPORT_ON_QUIT
		tracks++;
		totalTime += ms;
		logReport.TryGetValue( code, out var codeTimeAcc );
		logReport[code] = codeTimeAcc + ms;
		callsReport.TryGetValue( code, out var calls );
		callsReport[code] = calls + 1;
#endif
#endif
	}
}
