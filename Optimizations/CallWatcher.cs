// #define DEBUG_NOTIFY
using UnityEngine;

public class CallWatcher
{
    double lastCall;
#if DEBUG_NOTIFY
    int calls;
    int ignoredCalls;
    string debugName;
#endif

    public CallWatcher(string debugName) : this()
    {
#if DEBUG_NOTIFY
        this.debugName = debugName;
#endif
    }

    public CallWatcher()
    {
#if DEBUG_NOTIFY
        calls = 0;
#endif
        lastCall = -1;
    }

    public bool CallIsBlocked( float minTime, GameObject context = null )
    {
        return !CanCall( minTime, context );
    }

    public bool CanCall( float minTime, GameObject context = null )
    {
        var timeRef = Time.timeSinceLevelLoadAsDouble;
        var firstCall = lastCall < 0 || timeRef < lastCall;

#if DEBUG_NOTIFY
        var contextStr = context != null ? context.name : "Unknown";
#endif

        if( firstCall )
        {
#if DEBUG_NOTIFY
            Debug.Log($"<color=#FFD700>[{debugName}]</color> first call <color=#7FFF7F>{timeRef:F3}s</color> — <color=#FF9944>{calls} calls</color> on <color=#7FC8FF>{contextStr}</color>");
            calls++;
#endif
            lastCall = timeRef;
            return true;
        }
        
        var deltaTime = timeRef - lastCall;
        lastCall = timeRef;
        var canCall = deltaTime >= minTime;

#if DEBUG_NOTIFY
        if (canCall)
            Debug.Log($"<color=#FFD700>[{debugName}]</color> +<color=#7FFF7F>{deltaTime:F3}s</color> — <color=#FF9944>{calls}/{ignoredCalls+calls} calls</color> on <color=#7FC8FF>{contextStr}</color>");
        else
            Debug.Log($"<color=#FFD700>[{debugName}]</color> <color=#FF9944>Skipped</color> <color=#7FFF7F>{deltaTime:F3}s</color> — <color=#FF9944>{ignoredCalls}/{ignoredCalls+calls} ignored calls</color> on <color=#7FC8FF>{contextStr}</color>");
#endif

        if( canCall )
        {
#if DEBUG_NOTIFY
            calls++;
#endif
            lastCall = timeRef;
        }
#if DEBUG_NOTIFY
        else
        {
            ignoredCalls++;
        }
#endif
        
        return canCall;
    }
}