using UnityEngine;

public class ApplicationEventsRelay : MonoBehaviour
{
    EventSlot _onQuit = new();
    BoolState _isFocused = new();
    BoolState _isPaused = new();

    public static bool HasInstance => Singleton<ApplicationEventsRelay>.IsValid;

    public static IEventRegister OnQuit => Eternal<ApplicationEventsRelay>.Instance._onQuit;
    public static IBoolStateObserver IsFocused  => Eternal<ApplicationEventsRelay>.Instance._isFocused;
    public static IBoolStateObserver IsPaused  => Eternal<ApplicationEventsRelay>.Instance._isPaused;

    private void Awake()
    {
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.Awake()" );
        _isFocused = new BoolState(Application.isFocused);
        _isPaused = new BoolState(!Application.isPlaying);

#if MICROSOFT_GDK && UNITY_GAMECORE
        UnityEngine.GameCore.GameCorePLM.OnApplicationSuspendingEvent += OnApplicationSuspendingEvent;
        UnityEngine.GameCore.GameCorePLM.OnSuspendingEvent += () => Debug.Log( $"<color=magenta>GameCorePLM</color>.<color=cyan>OnSuspendingEvent</color>()" );
        UnityEngine.GameCore.GameCorePLM.OnResumingEvent += OnResumingEvent;
        UnityEngine.GameCore.GameCorePLM.OnResourceAvailabilityChangedEvent += OnResourceAvailabilityChangedEvent;

        void OnApplicationSuspendingEvent( bool midLoad )
        {
            Debug.Log( $"<color=magenta>GameCorePLM</color>.<color=cyan>OnApplicationSuspendingEvent</color>( {midLoad} )" );
            _isPaused?.Setter(true);
            _onQuit?.Trigger();
        }

        void OnResumingEvent( double secondsSuspended )
        {
            Debug.Log( $"<color=magenta>GameCorePLM</color>.<color=cyan>OnResumingEvent</color>( {secondsSuspended} )" );
            _isPaused?.Setter( false );
        }

        void OnResourceAvailabilityChangedEvent( bool amConstrained )
        {
            Debug.Log( $"<color=magenta>GameCorePLM</color>.<color=cyan>OnResourceAvailabilityChangedEvent</color>( {(amConstrained?"amConstrained":"?NotConstrained?")} )" );
        }
#endif
    }

    private void OnApplicationQuit()
    {
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnApplicationQuit</color>()" );
        _onQuit?.Trigger();
    }

    private void OnApplicationFocus(bool focus)
    {
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnApplicationFocus</color>( {focus} )" );
        _isFocused?.Setter(focus);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnApplicationPause</color>( {pauseStatus} )" );
        _isPaused?.Setter(pauseStatus);
    }

#if UNITY_GAMECORE || UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
    private void OnDestroy()
    {
        Debug.Log( $"<color=magenta>ApplicationEventsRelay</color>.<color=cyan>OnDestroy</color>()" );
        _onQuit?.Trigger();
    }
#endif
}
