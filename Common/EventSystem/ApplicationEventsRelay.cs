// #define LOG_EVENTS
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ApplicationEventsRelay : MonoBehaviour
{
    private static Thread _mainThread;
    EventSlot _onQuit = new();
    BoolState _isFocused = new();
    BoolState _isPaused = new();
    BoolState _isSuspended = new();

    public static bool HasInstance => Singleton<ApplicationEventsRelay>.IsValid;

    public static IEventRegister OnQuit => Eternal<ApplicationEventsRelay>.Instance._onQuit;
    public static IBoolStateObserver IsFocused  => Eternal<ApplicationEventsRelay>.Instance._isFocused;
    public static IBoolStateObserver IsPaused  => Eternal<ApplicationEventsRelay>.Instance._isPaused;
    public static IBoolStateObserver IsSuspended  => Eternal<ApplicationEventsRelay>.Instance._isSuspended;

    private void Awake()
    {
        if (_mainThread == null)
            _mainThread = Thread.CurrentThread;

#if LOG_EVENTS
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.Awake()" );
#endif
        _isFocused = new BoolState(Application.isFocused);
        _isPaused = new BoolState(!Application.isPlaying);

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
        if (!HasInstance)
            Eternal<ApplicationEventsRelay>.Request();

        return _mainThread != null && _mainThread.Equals(Thread.CurrentThread);
    }

    private void OnApplicationQuit()
    {
        _isSuspended?.SetTrue();
#if LOG_EVENTS
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnApplicationQuit</color>()" );
#endif
        _onQuit?.Trigger();
    }

    private void OnApplicationFocus(bool focus)
    {
#if LOG_EVENTS
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnApplicationFocus</color>( {focus} )" );
#endif
        _isFocused?.Setter(focus);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
#if LOG_EVENTS
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnApplicationPause</color>( {pauseStatus} )" );
#endif
        _isPaused?.Setter(pauseStatus);
    }

#if UNITY_GAMECORE || UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
    private void OnDestroy()
    {
#if LOG_EVENTS
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnDestroy</color>()" );
#endif
        _onQuit?.Trigger();
    }
#if UNITY_EDITOR
    private void OnExitPlayMode(PlayModeStateChange change)
    {
        _isSuspended?.SetTrue();
        _onQuit?.Trigger();
    }
#endif
#endif
}
