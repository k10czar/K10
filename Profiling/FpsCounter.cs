using System.Collections.Generic;
using UnityEngine;

public struct FpsCounter
{
    const int MAX_COUNT = 1200;
    const float MIN_SAMPLE_TIME = .1f;
    const string SUFFIX = " FPS";
    static string[] _fpsCounterStringsCache = new string[ MAX_COUNT ];

    [Range(MIN_SAMPLE_TIME,2),SerializeField] float _sampleSecond;
    List<double> _frameTimes;
    int _currentFps;
    int _lastFrameCount;

    public int CurrentFps => _currentFps;
    public string CurrentFpsText => RequestString( _currentFps );

    public FpsCounter( float sampleSecond = .5f )
    {
        _sampleSecond = sampleSecond;
        _frameTimes = new();
        _lastFrameCount = -1;
        _currentFps = 0;
    }

    public string RequestString( int value )
    {
        var maxFpsLabel = _fpsCounterStringsCache.Length - 1;
        var clamped = Mathf.Clamp( value, 0, maxFpsLabel );

        if( _fpsCounterStringsCache[clamped] == null )
        {
            var str = value.ToString() + SUFFIX;
            if( clamped == 0 ) _fpsCounterStringsCache[ clamped ] = "<" + str;
            else if( clamped == maxFpsLabel ) _fpsCounterStringsCache[ clamped ] = ">" + str;
            else _fpsCounterStringsCache[ clamped ] = str;
        }

        return _fpsCounterStringsCache[ clamped ];
    }

    public bool Update()
    {
        var currFrameCount = Time.frameCount;
        if( _lastFrameCount == currFrameCount ) return false;
        _lastFrameCount = currFrameCount;

        if( _sampleSecond < MIN_SAMPLE_TIME ) _sampleSecond = MIN_SAMPLE_TIME;

        var sampleTime = Time.unscaledTimeAsDouble - _sampleSecond;
        if( _frameTimes == null ) _frameTimes = new();
        while( _frameTimes.Count > 1 && _frameTimes[0] < sampleTime ) _frameTimes.RemoveAt( 0 );

        var currTime = Time.unscaledTimeAsDouble;

        var sample = 1.0;
        if( _frameTimes.Count > 0 ) sample = currTime - _frameTimes[0];

        var fps = (int)( System.Math.Round( _frameTimes.Count / sample ) + .01 );
        var changed = fps != _currentFps;
        _currentFps = fps;

        _frameTimes.Add( Time.unscaledTimeAsDouble );

        return changed;
    }
}
