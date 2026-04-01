#if UNITY_GAMECORE
using UnityEngine;
using K10.DebugSystem;

public class GdkGameRuntimeServiceConfigurator : MonoBehaviour, IService, IStartable, ILoggable<GdkLogCategory>
{
    [SerializeField, InlineProperties] private GdkPlatformSettingsData _gdkSettings;
    private GdkGameRuntimeService gdkRuntimeManager;

    public IGdkRuntimeService MainService => gdkRuntimeManager;

    private void Awake()
    {
        Debug.Log("[GDK] Configurator Awake");
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        Debug.Log("[GDK] Configurator Start");

        if (_gdkSettings == null)
        {
            Debug.LogError("[GDK] gdkSettings is NULL (missing SO reference or serialization mismatch)");
            return;
        }

        if (ServiceLocator.Contains<GdkGameRuntimeService>())
        {
            Debug.Log("[GDK] Service already registered, skipping");
            return;
        }

        Debug.Log("[GDK] Creating GdkGameRuntimeService...");
        gdkRuntimeManager = new GdkGameRuntimeService(
            _gdkSettings.Scid,
            _gdkSettings.SaveScid,
            _gdkSettings.TitleId,
            _gdkSettings.Sandbox
        );

        ServiceLocator.Register(gdkRuntimeManager);

        // Optional: if your ServiceLocator supports interface registration, add this too
        // ServiceLocator.Register<IGdkRuntimeService>(gdkRuntimeManager);

        Debug.Log("[GDK] Service registered OK");
    }

    private void OnDestroy()
    {
        Debug.Log("[GDK] Configurator OnDestroy");

        if (gdkRuntimeManager != null)
            gdkRuntimeManager.Dispose();
    }
}
#endif