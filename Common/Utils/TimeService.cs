
using System;
using UnityEngine;

public static class TimeService
{
	private static double _timeSetted = 0;
	private static DateTime _dateTimeSetted = DateTime.UtcNow;
	private static DateTime _timeServerTimeOnAuth = DateTime.UtcNow;

    public static void Reset()
    {
	    _timeSetted = 0;
	    _dateTimeSetted = DateTime.UtcNow;
	    _timeServerTimeOnAuth = DateTime.UtcNow;
    }

	public static void SetUtcTimeReference( string str )
	{
		_timeServerTimeOnAuth = DateTime.Parse( str ).ToUniversalTime();
		_timeSetted = Time.unscaledTimeAsDouble;
		_dateTimeSetted = DateTime.UtcNow;
		// Debug.Log( $"TimeService.Sync( {_timeServerTimeOnAuth} )" );
	}

	public static DateTime GetUtcNow()
	{
		var secsDiff = Time.unscaledTimeAsDouble - _timeSetted;
		var timeSpan = DateTime.UtcNow.Subtract( _dateTimeSetted );
		var timeSpanSeconds = timeSpan.TotalSeconds;
        // Debug.Log( $"secsDiff:{secsDiff} timeSpanSeconds:{timeSpanSeconds} timeSpan:{timeSpan}" );
		if( secsDiff > timeSpanSeconds || secsDiff < 0 ) secsDiff = timeSpanSeconds;
		return _timeServerTimeOnAuth.AddSeconds( secsDiff );
	}
}