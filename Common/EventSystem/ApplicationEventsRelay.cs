// #define LOG_EVENTS
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ApplicationEventsRelay : MonoBehaviour
{
    private static ApplicationEventsRelay instance;
    private static Thread mainThread;

    public static readonly BoolState isQuitting = new();
    public static readonly BoolState isFocused = new();
    public static readonly BoolState isPaused = new();
    public static readonly BoolState isSuspended = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance != null) return;

        var obj = new GameObject("[ApplicationEventsRelay]");
        instance = obj.AddComponent<ApplicationEventsRelay>();
        DontDestroyOnLoad(obj);
    }

    private void Awake()
    {
        mainThread ??= Thread.CurrentThread;

#if LOG_EVENTS
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.Awake()" );
#endif
        isFocused.Setter(Application.isFocused);
        isPaused.Setter(!Application.isPlaying);

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnExitPlayMode;
#endif

#if MICROSOFT_GDK && UNITY_GAMECORE
        UnityEngine.GameCore.GameCorePLM.OnApplicationSuspendingEvent += OnApplicationSuspendingEvent;
#if LOG_EVENTS
        UnityEngine.GameCore.GameCorePLM.OnSuspendingEvent += () => Debug.Log( $"<color=magenta>GameCorePLM</color>.<color=cyan>OnSuspendingEvent</color>()" );
#endif //LOG_EVENTS
        UnityEngine.GameCore.GameCorePLM.OnResumingEvent += OnResumingEvent;
        UnityEngine.GameCore.GameCorePLM.OnResourceAvailabilityChangedEvent += OnResourceAvailabilityChangedEvent;

        void OnApplicationSuspendingEvent( bool midLoad )
        {
            _isSuspended.SetTrue();
#if LOG_EVENTS
            Debug.Log( $"<color=magenta>GameCorePLM</color>.<color=cyan>OnApplicationSuspendingEvent</color>( {midLoad} )" );
#endif //LOG_EVENTS
        }

        void OnResumingEvent( double secondsSuspended )
        {
            _isSuspended.SetFalse();
#if LOG_EVENTS
            Debug.Log( $"<color=magenta>GameCorePLM</color>.<color=cyan>OnResumingEvent</color>( {secondsSuspended} )" );
#endif //LOG_EVENTS
        }

        void OnResourceAvailabilityChangedEvent( bool amConstrained )
        {
#if LOG_EVENTS
            Debug.Log( $"<color=magenta>GameCorePLM</color>.<color=cyan>OnResourceAvailabilityChangedEvent</color>( {(amConstrained?"Constrained":"Free")} )" );
#endif //LOG_EVENTS
        }
#endif //MICROSOFT_GDK && UNITY_GAMECORE
    }

    public static bool IsMainThread()
    {
#if UNITY_EDITOR
        if( !Application.isPlaying ) return false;
#endif
        return mainThread != null && mainThread.Equals(Thread.CurrentThread);
    }

    private void OnApplicationQuit()
    {
        isSuspended.SetTrue();
#if LOG_EVENTS
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnApplicationQuit</color>()" );
#endif
        isQuitting.SetTrue();
    }

    private void OnApplicationFocus(bool focus)
    {
#if LOG_EVENTS
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnApplicationFocus</color>( {focus} )" );
#endif
        isFocused.Setter(focus);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
#if LOG_EVENTS
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnApplicationPause</color>( {pauseStatus} )" );
#endif
        isPaused.Setter(pauseStatus);
    }

#if UNITY_GAMECORE || UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
    private void OnDestroy()
    {
#if LOG_EVENTS
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnDestroy</color>()" );
#endif
        isQuitting.SetTrue();
    }
#endif

#if UNITY_EDITOR
    private void OnExitPlayMode(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.ExitingPlayMode) return;

        isSuspended.SetTrue();
        isQuitting.SetTrue();
    }
#endif
}