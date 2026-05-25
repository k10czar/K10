

public static class TimeLimitExtensions
{
    public static double ElapsedSeconds( this ITimeLimitObserver tl ) => tl.ElapsedMiliseconds / 1000;
}

public interface ITimeLimit : ITimeLimitObserver, ITimeLimitInteractor
{
}

public interface ITimeLimitObserver
{
    bool IsExausted { get; }
    double ElapsedMiliseconds { get; }
}

public interface ITimeLimitInteractor
{
    void Begin();
    void Reset();
    void End();
    bool CheckExaustion();
}

public class TimeLimit : ITimeLimit
{
    float _milisecondsLimit = 100; 
    System.Diagnostics.Stopwatch _calcTime;
    bool _exausted = false;
    bool _running = false;
    double _elapsedMiliseconds = 0;
    public double ElapsedMiliseconds => _elapsedMiliseconds;

    bool _isExausted = false;
    public bool IsExausted => _isExausted;

    public TimeLimit( float milisecondsLimit = 100 )
    {
        _milisecondsLimit = milisecondsLimit;
    }

    public void Begin()
    {
        if( _running ) return;
        _running = true;
        _calcTime = StopwatchPool.RequestStarted();
    }

    public void Reset()
    {
        if( _running )
        {
            End();
        }
        _running = true;
        _calcTime = StopwatchPool.RequestStarted();
    }

    public void End()
    {
        if( !_running ) return;
        _running = false;
        _elapsedMiliseconds = _calcTime.ReturnToPoolAndGetElapsedMs();
        _calcTime = null;
    }

    public bool CheckExaustion()
    {
        if( _exausted ) return true;
        if( !_running ) return true;
        if( _calcTime.Elapsed.Milliseconds > _milisecondsLimit )
        {
            _exausted = true;
            return true;
        }
        return false;
    }

    public override string ToString() => $"{(_isExausted?"Exausted":"Completed")} in {_elapsedMiliseconds/1000}s";
}