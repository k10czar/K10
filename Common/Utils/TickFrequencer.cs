using System.Runtime.CompilerServices;

public struct TickFrequencer
{
    float _time;
    float _interval;

    public TickFrequencer(int frequency)
    {
        _time = 0;
        _interval = 1f / frequency;
    }

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void SetFrequency(float frequency)
    {
        _interval = 1f / MathAdapter.max( 0.000001f, frequency );
    }

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public bool Tick(float deltaTime)
    {
        _time += deltaTime;
        if (_time < _interval) return false;
        NotifyTick();
        return true;
    }

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
    public void NotifyTick()
    {
        if (_time > _interval) _time -= _interval;
        if (_time > _interval) _time = _time % _interval;
    }
}
