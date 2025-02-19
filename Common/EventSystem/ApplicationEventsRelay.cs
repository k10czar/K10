using UnityEngine;

public class ApplicationEventsRelay : MonoBehaviour
{
    EventSlot _onQuit = new();
    BoolState _isFocused = new();
    BoolState _isPaused = new();

    public static IEventRegister OnQuit => Eternal<ApplicationEventsRelay>.Instance._onQuit;
    public static IBoolStateObserver IsFocused  => Eternal<ApplicationEventsRelay>.Instance._isFocused;
    public static IBoolStateObserver IsPaused  => Eternal<ApplicationEventsRelay>.Instance._isPaused;

    private void Awake()
    {
        _isFocused = new BoolState(Application.isFocused);
        _isPaused = new BoolState(!Application.isPlaying);
    }

    private void OnApplicationQuit()
    {
        _onQuit?.Trigger();
    }

    private void OnApplicationFocus(bool focus)
    {
        _isFocused?.Setter(focus);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        _isPaused?.Setter(pauseStatus);
    }

#if UNITY_GAMECORE || UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
    private void OnDestroy()
    {
        _onQuit?.Trigger();
    }
#endif
}
