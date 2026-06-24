// #define DEBUG_NOTIFY_CALLS
// #define DEBUG_NOTIFY_CALLS_GLOBAL
using UnityEngine;

#if UNITY_EDITOR && DEBUG_NOTIFY_CALLS_GLOBAL
using UnityEditor;

[InitializeOnLoad]
public static class CallWatcherEditorFlusher
{
    static CallWatcherEditorFlusher()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
            Debug.Log($"<color=#FFD700>[CallWatcher]</color> Global: <color=#FF9944>{CallWatcher.GlobalState()}</color>");
    }
}
#endif

public class CallWatcher
{
    double lastCall;
#if DEBUG_NOTIFY_CALLS
    int calls;
    int ignoredCalls;
    string debugName;
#endif

#if DEBUG_NOTIFY_CALLS_GLOBAL
    static int globalCalls;
    static int globalignoredCalls;
#endif

    public CallWatcher(string debugName) : this()
    {
#if DEBUG_NOTIFY_CALLS
        this.debugName = debugName;
#endif
    }

    public CallWatcher()
    {
#if DEBUG_NOTIFY_CALLS
        calls = 0;
#endif
        lastCall = -1;
    }

    public bool CallIsBlocked( float minTime, GameObject context = null )
    {
        return !CanCall( minTime, context );
    }

    public static string GlobalState()
    {   
#if DEBUG_NOTIFY_CALLS_GLOBAL
        var globalTotal = globalCalls + globalignoredCalls;
        var callsPctg = globalCalls*100/globalTotal;
        var ignoredCallsPctg = globalCalls*100/globalTotal;
        return $"{globalCalls}({callsPctg}%) + *ignored{globalignoredCalls}({ignoredCallsPctg}%)* / {globalTotal}";
#else
        return "CallWatcher Cannot count global calls without DEBUG_NOTIFY_CALLS_GLOBAL directive";
#endif
    }

    public string LocalState()
    {   
#if DEBUG_NOTIFY_CALLS
        var total = calls + ignoredCalls;
        var callsPctg = calls*100/total;
        var ignoredCallsPctg = calls*100/total;
        return $"{calls}({callsPctg}%) + *ignored{ignoredCalls}({ignoredCallsPctg}%)* / {total}";
#else
        return "CallWatcher Cannot count calls without DEBUG_NOTIFY_CALLS directive";
#endif
    }

    public bool CanCall( float minTime, GameObject context = null )
    {
        var timeRef = Time.timeSinceLevelLoadAsDouble;
        var firstCall = lastCall < 0 || timeRef < lastCall;

#if DEBUG_NOTIFY_CALLS
        var contextStr = context != null ? context.name : "Unknown";
#endif

        if( firstCall )
        {
#if DEBUG_NOTIFY_CALLS
            Debug.Log($"<color=#FFD700>[{debugName}]</color> first call <color=#7FFF7F>{timeRef:F3}s</color> — <color=#FF9944>{LocalState()}</color> on <color=#7FC8FF>{contextStr}</color>");
            calls++;
#endif
#if DEBUG_NOTIFY_CALLS_GLOBAL
            globalCalls++;
#endif
            lastCall = timeRef;
            return true;
        }
        
        var deltaTime = timeRef - lastCall;
        lastCall = timeRef;
        var canCall = deltaTime >= minTime;

        if( canCall )
        {
#if DEBUG_NOTIFY_CALLS
            calls++;
            Debug.Log($"<color=#FFD700>[{debugName}]</color> +<color=#7FFF7F>{deltaTime:F3}s</color> — <color=#FF9944>{LocalState()}</color> on <color=#7FC8FF>{contextStr}</color>");
#endif
#if DEBUG_NOTIFY_CALLS_GLOBAL
            globalCalls++;
#endif
            lastCall = timeRef;
        }
        else
        {
#if DEBUG_NOTIFY_CALLS_GLOBAL
            globalignoredCalls++;
#endif
#if DEBUG_NOTIFY_CALLS
            ignoredCalls++;
            Debug.Log($"<color=#FFD700>[{debugName}]</color> <color=#FF9944>Skipped</color> <color=#7FFF7F>{deltaTime:F3}s</color> — <color=#FF9944>{LocalState()}</color> on <color=#7FC8FF>{contextStr}</color>");
#endif
        }
        
        return canCall;
    }
}