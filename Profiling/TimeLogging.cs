using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

public class TimeLogging<T>
{
	Log _currentLog;
	Log _defaultLog = new Log( UnityEngine.Time.unscaledTime, "Default" );
	List<Log> _loadingLogs = new List<Log>();

    protected EventSlot<string> _onLogEnd = new EventSlot<string>();
    protected EventSlot<string> _onLogSectionEnd = new EventSlot<string>();

#if UNITY_EDITOR
    static bool _colored = true;
#else
    static bool _colored = false;
#endif

	public void StartLog( string name )
	{
		if( _currentLog != null && _onLogEnd.HasListeners ) _onLogEnd.Trigger( $"{StartColorTag("red")}StartLog beyond old{EndColorTag()} {_currentLog}" );
		_currentLog = new Log( UnityEngine.Time.unscaledTime, name );
	}

	public void EndLog()
	{
        if( _currentLog == null ) return;

		_currentLog.End();
		_loadingLogs.Add( _currentLog );

		if( _onLogEnd.HasListeners ) 
		{
			_onLogEnd.Trigger( GetEndLog() );
			_onLogEnd.Trigger( GetExportLog() );
		}

		_currentLog = null;
	}

    public static string EndColorTag() => ( _colored ? "</color>" : "" );
    public static string StartColorTag( string color ) => ( _colored ? $"<color={color}>" : "" );

    private string GetEndLog() => $"{StartColorTag("orange")}Loading log [{_loadingLogs.Count}] => {_currentLog.Name}:{EndColorTag()}\n{_currentLog.LogWithPercentage()}";
    private string GetExportLog() => $"{StartColorTag("orange")}Loading log data export{EndColorTag()}:\n{_currentLog.GetExportedData(-_currentLog.StartMs)}";

	public void CancelLog()
	{
        if( _currentLog == null ) return;

		_currentLog.End();
        if( _onLogEnd.HasListeners ) _onLogEnd.Trigger( $"{StartColorTag("red")}Canceled log:{EndColorTag()} {_currentLog.ToString()}" );
		_currentLog = null;
	}

	public void StartSection( T cat, object key = null )
	{
		var log = ( _currentLog ?? _defaultLog );
		log.StartSection( cat, key );
	}

	public void EndSection( T cat, object key = null )
	{
		var log = ( _currentLog ?? _defaultLog );
		log.EndSection( cat, key );
		if( _onLogSectionEnd.HasListeners ) _onLogSectionEnd.Trigger( $"{StartColorTag("orange")}End loading log section:{EndColorTag()} {cat}{( ( key != null ) ? $" {key}" : "" )}\n{log.GetData( cat )}" );
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
			return $"{string.Join(",\n", _logData.ToList().ConvertAll((kvp) => $"[{StartColorTag("orange")}\'{kvp.Key.ToStringOrNull()}\'{EndColorTag()}, {StartColorTag("red")}{kvp.Value.GenericLog.StartMs + startOffset}{EndColorTag()}, {StartColorTag("red")}{kvp.Value.GenericLog.EndMs + startOffset}{EndColorTag()}],{(kvp.Value.HasEntries()?($"\n{kvp.Value.GetExportedData(startOffset)}"):"")}"))}";
		}

		public override string ToString()
		{
			return $"Log:{_log}\n\n{string.Join( "\n\n", _logData.ToList().ConvertAll( ( kvp ) => $"{StartColorTag("yellow")}{kvp.Key}{EndColorTag()}: {kvp.Value}" ) )}";
		}

		public string LogWithPercentage()
		{
			var totalMs = _log.DurationMs;
			return $"Log:{_log}\n\n{string.Join("\n\n", _logData.ToList().ConvertAll((kvp) => $"{StartColorTag("yellow")}{kvp.Key}{EndColorTag()}({StartColorTag("lime")}{(kvp.Value.DurationMs/totalMs):P1}{EndColorTag()}): {kvp.Value.LogWithPercentage(totalMs)}"))}";
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
				return $"{_genericLog}{((_data.Count > 0) ? "\n" : "")}{string.Join(",\n", _data.ToList().ConvertAll((kvp) => $"\t-{StartColorTag("orange")}{kvp.Key.ToStringOrNull()}{EndColorTag()}: {kvp.Value}"))}";
			}

			public string LogWithPercentage( float totalMs )
			{
				return $"{_genericLog}{((_data.Count > 0) ? "\n" : "")}{string.Join(",\n", _data.ToList().ConvertAll((kvp) => $"\t-{StartColorTag("orange")}{kvp.Key.ToStringOrNull()}{EndColorTag()}({StartColorTag("green")}{(kvp.Value.DurationMs/totalMs):P1}{EndColorTag()}): {kvp.Value}"))}";
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
				return $"[\'{StartColorTag("orange")}{name.ToStringOrNull()}{EndColorTag()}\', {StartColorTag("red")}{StartMs + startOffset}{EndColorTag()}, {StartColorTag("red")}{EndMs + startOffset}{EndColorTag()}]";
			}

			public string LogWithPercentage( float totalMs )
			{
				var mins = UnityEngine.Mathf.FloorToInt(_startTime / 60);
				var secs = UnityEngine.Mathf.FloorToInt(_startTime % 60);
				var ms = UnityEngine.Mathf.FloorToInt((_startTime * 1000) % 1000);

				var endTime = _startTime + (_duration / 1000);
				var toMins = UnityEngine.Mathf.FloorToInt(endTime / 60);
				var toSecs = UnityEngine.Mathf.FloorToInt(endTime % 60);
				var toMs = UnityEngine.Mathf.FloorToInt((endTime * 1000) % 1000);

				return $"{GetLogPrefix(_watch != null)}{_duration}ms{EndColorTag()} ({StartColorTag("green")}{(DurationMs / totalMs):P1}{EndColorTag()}) @ ( {mins:N0}:{secs:D2}.{ms:D4} -> {toMins:N0}:{toSecs:D2}.{toMs:D4} [{_startTime} -> {endTime}] )";
			}

            private static string GetLogPrefix( bool running )
            {
                if( _colored )
                {
                    if( running ) return $"{StartColorTag("red")}®{EndColorTag()}{StartColorTag("yellow")}";
                    else return StartColorTag("cyan");
                }

                if( running ) return "®";
                return "";
            }

			public override string ToString()
			{
				var mins = UnityEngine.Mathf.FloorToInt(_startTime / 60);
				var secs = UnityEngine.Mathf.FloorToInt(_startTime % 60);
				var ms = UnityEngine.Mathf.FloorToInt((_startTime * 1000) % 1000);

				var endTime = _startTime + (_duration / 1000);
				var toMins = UnityEngine.Mathf.FloorToInt(endTime / 60);
				var toSecs = UnityEngine.Mathf.FloorToInt(endTime % 60);
				var toMs = UnityEngine.Mathf.FloorToInt((endTime * 1000) % 1000);

				return $"{GetLogPrefix(_watch != null)}{_duration}ms{EndColorTag()}@( {mins:N0}:{secs:D2}.{ms:D4} -> {toMins:N0}:{toSecs:D2}.{toMs:D4} [{_startTime} -> {endTime}] )";
			}
		}
	}
}