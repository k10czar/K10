using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using static Colors.Console;

public class TimeLogging<T>
{
	Log _currentLog;
	Log _defaultLog = new Log( Time.unscaledTime, "Default" );
	List<Log> _loadingLogs = new List<Log>();
	Stopwatch _logMs = new Stopwatch();
	double _totalLog = 0;

    protected EventSlot<string> _onLogEnd = new EventSlot<string>();
    protected EventSlot<string> _onLogSectionEnd = new EventSlot<string>();

#if UNITY_EDITOR
    static bool _colored = true;
#else
    static bool _colored = false;
#endif

	public void StartLog( string name )
	{
		_logMs.Reset();
		if( _currentLog != null && _onLogEnd.HasListeners ) _onLogEnd.Trigger( $"{"StartLog beyond old".Colorfy(Negation)} {_currentLog}" );
		_currentLog = new Log( Time.unscaledTime, name );
		_logMs.Stop();
	}

	public void EndLog()
	{
        if( _currentLog == null ) return;

		_logMs.Start();
		_currentLog.End();
		_loadingLogs.Add( _currentLog );

		if( _onLogEnd.HasListeners ) 
		{
			_onLogEnd.Trigger( GetEndLog() );
			_onLogEnd.Trigger( GetExportLog() );
		}

		_currentLog = null;

		_logMs.Stop();
		UnityEngine.Debug.Log( $"{"Log".Colorfy(Info)} end with {$"{_logMs.ElapsedMilliseconds}ms".Colorfy(Numbers)} of {"overhead".Colorfy(Negation)}" );
		_logMs.Reset();
	}

    private string GetEndLog() => $"{$"Loading log [{_loadingLogs.Count}] => {_currentLog.Name}:".Colorfy(Info)}\n{_currentLog.LogWithPercentage()}";
    private string GetExportLog() => $"{"Loading log data export".Colorfy(Info)}:\n{_currentLog.GetExportedData(-_currentLog.StartMs)}";

	public void CancelLog()
	{
        if( _currentLog == null ) return;

		_logMs.Start();
		
		_currentLog.End();
        if( _onLogEnd.HasListeners ) _onLogEnd.Trigger( $"{"Canceled log:".Colorfy(Negation)} {_currentLog.ToString()}" );
		_currentLog = null;

		_logMs.Stop();
		UnityEngine.Debug.Log( $"Log end with {$"{_logMs.ElapsedMilliseconds}ms".Colorfy(Numbers)} of {"overhead".Colorfy(Negation)}" );
		_logMs.Reset();
	}

	public void StartSection( T cat, object key = null )
	{
		_logMs.Start();
		var log = ( _currentLog ?? _defaultLog );
		log.StartSection( cat, key );
		_logMs.Stop();
	}

	public void EndSection( T cat, object key = null )
	{
		_logMs.Start();
		var log = ( _currentLog ?? _defaultLog );
		log.EndSection( cat, key );
		if( _onLogSectionEnd.HasListeners ) _onLogSectionEnd.Trigger( $"{"End loading log section:".Colorfy(Info)} {cat.ToStringOrNullColored(Keyword)}{( ( key != null ) ? $" {key.ToStringOrNullColored(EventName)}" : "" )}\n{log.GetData( cat )}" );
		_logMs.Stop();
	}

	public class Log
	{
		private string _name;
		private LogEntry _log = new LogEntry();
		private Dictionary<T, LogCategoryEntry> _logData = new Dictionary<T, LogCategoryEntry>();

		public string Name => _name;
		public float StartMs => _log.StartMs;

		public Log( float startTime, string name )
		{
			_log.Start( startTime );
			_name = name;
		}

		public void End()
		{
			_log.End();
			foreach( var data in _logData ) data.Value.EndSection( null );
		}

		public LogCategoryEntry GetData( T cat )
		{
			if( !_logData.TryGetValue( cat, out var data ) )
			{
				data = new LogCategoryEntry();
				_logData[cat] = data;
			}
			return data;
		}

		public void StartSection( T cat, object key )
		{
			GetData( cat ).StartSection( key, _log.CurrentTime );
		}

		public void EndSection( T cat, object key )
		{
			var data = GetData( cat );
			data.EndSection( key );
		}

		public string GetExportedData( float startOffset = 0 )
		{
			return $"{string.Join(",\n", _logData.ToList().ConvertAll((kvp) => $"[{$"\'{kvp.Key.ToStringOrNull()}\'".Colorfy(Info)}, {(kvp.Value.GenericLog.StartMs + startOffset).ToStringColored( Negation )}, {(kvp.Value.GenericLog.EndMs + startOffset).ToStringColored( Negation )}]{(kvp.Value.HasEntries()?($",\n{kvp.Value.GetExportedData(startOffset)}"):"")}"))}";
		}

		public override string ToString()
		{
			return $"Log:{_log}\n\n{string.Join( "\n\n", _logData.ToList().ConvertAll( ( kvp ) => $"{kvp.Key.ToStringOrNullColored(Names)}: {kvp.Value}" ) )}";
		}

		public string LogWithPercentage()
		{
			var totalMs = _log.DurationMs;
			return $"Log:{_log}\n\n{string.Join("\n\n", _logData.ToList().ConvertAll((kvp) => $"{kvp.Key.ToStringOrNullColored(Names)}({$"{(kvp.Value.DurationMs/totalMs):P1}".Colorfy( Keyword )}): {kvp.Value.LogWithPercentage(totalMs)}"))}";
		}

		public class LogCategoryEntry
		{
			private LogEntry _genericLog = new LogEntry();
			private Dictionary<object, LogEntry> _data = new Dictionary<object, LogEntry>();

			public LogEntry GenericLog => _genericLog;

			public float DurationMs => _genericLog.DurationMs;

			public void StartSection( object key, float startTime )
			{
				if( !_genericLog.IsRunning ) _genericLog.Start( startTime );
				if( key != null )
				{
					if( !_data.TryGetValue( key, out var log ) )
					{
						log = new LogEntry();
						_data[key] = log;
					}
					log.Start( startTime );
				}
			}

			public void EndSection( object key )
			{
				if( key == null )
				{
					_genericLog.End();
					foreach( var data in _data ) data.Value.End();
				}
				else
				{
					if( _data.TryGetValue( key, out var log ) ) log.End();
				}
			}

			public bool HasEntries() => _data.Count > 0;

			public string GetExportedData( float startOffset = 0 )
			{
				return $"{string.Join(",\n", _data.ToList().ConvertAll((kvp) => kvp.Value.GetExportedData( kvp.Key.ToStringOrNull(), startOffset ) ) )}";
			}

			public override string ToString()
			{
				return $"{_genericLog}{((_data.Count > 0) ? "\n" : "")}{string.Join(",\n", _data.ToList().ConvertAll((kvp) => $"\t-{kvp.Key.ToStringOrNull().Colorfy(Info)}: {kvp.Value}"))}";
			}

			public string LogWithPercentage( float totalMs )
			{
				return $"{_genericLog}{((_data.Count > 0) ? "\n" : "")}{string.Join(",\n", _data.ToList().ConvertAll((kvp) => $"\t-{kvp.Key.ToStringOrNull().Colorfy(Info)}({$"{(kvp.Value.DurationMs/totalMs):P1}".Colorfy(Keyword)}): {kvp.Value}"))}";
			}
		}

		public class LogEntry
		{
			float _startTime;
			float _duration = 0;
			private Stopwatch _watch = null;

			public bool IsRunning => ( _watch != null );
			public float CurrentTime => _startTime + ((_watch?.ElapsedMilliseconds ?? _duration) / 1000);

			public float StartMs => _startTime * 1000;
			public float EndMs => StartMs + _duration;

			public float DurationMs => _watch?.ElapsedMilliseconds ?? _duration;

			public void Start() 
			{
				Start( UnityEngine.Time.unscaledTime );
			}

			public void Start( float startTime )
			{
				_startTime = startTime;
				_watch = new Stopwatch();
				_watch.Start();
			}

			public void End()
			{
				if( _watch == null ) return;
				_watch.Stop();
				_duration = _watch.ElapsedMilliseconds;
				_watch = null;
			}

			public string GetExportedData( string name, float startOffset = 0 )
			{
				return $"[\'{name.ToStringOrNullColored( Info )}\', {(StartMs + startOffset).ToStringColored( Negation )}ms, {(EndMs + startOffset).ToStringColored( Negation )}ms]";
			}

			public static string EndColorTag() => ( _colored ? "</color>" : "" );
			public static string StartColorTag( string color ) => ( _colored ? $"<color=#{color}>" : "" );
			public static string StartColorTag( Color color ) => StartColorTag( color.ToHexRGB() );

			public string LogWithPercentage( float totalMs )
			{
				var mins = Mathf.FloorToInt(_startTime / 60);
				var secs = Mathf.FloorToInt(_startTime % 60);
				var ms = Mathf.FloorToInt((_startTime * 1000) % 1000);

				var endTime = _startTime + (_duration / 1000);
				var toMins = Mathf.FloorToInt(endTime / 60);
				var toSecs = Mathf.FloorToInt(endTime % 60);
				var toMs = Mathf.FloorToInt((endTime * 1000) % 1000);

				return $"{GetLogPrefix(_watch != null)}{_duration}ms{EndColorTag()} ({$"{(DurationMs / totalMs):P1}".ToStringColored( Keyword )}) @ ( {mins:N0}:{secs:D2}.{ms:D4} -> {toMins:N0}:{toSecs:D2}.{toMs:D4} [{_startTime} -> {endTime}] )";
			}

            private static string GetLogPrefix( bool running )
            {
                if( _colored )
                {
                    if( running ) return $"{StartColorTag( Negation )}®{EndColorTag()}{StartColorTag( Names )}";
                    else return StartColorTag( Numbers );
                }

                if( running ) return "®";
                return "";
            }

			public override string ToString()
			{
				var mins = Mathf.FloorToInt(_startTime / 60);
				var secs = Mathf.FloorToInt(_startTime % 60);
				var ms = Mathf.FloorToInt((_startTime * 1000) % 1000);

				var endTime = _startTime + (_duration / 1000);
				var toMins = Mathf.FloorToInt(endTime / 60);
				var toSecs = Mathf.FloorToInt(endTime % 60);
				var toMs = Mathf.FloorToInt((endTime * 1000) % 1000);

				return $"{GetLogPrefix(_watch != null)}{_duration}ms{EndColorTag()}@( {mins:N0}:{secs:D2}.{ms:D4} -> {toMins:N0}:{toSecs:D2}.{toMs:D4} [{_startTime} -> {endTime}] )";
			}
		}
	}
}