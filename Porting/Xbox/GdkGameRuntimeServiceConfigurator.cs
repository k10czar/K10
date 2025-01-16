#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using UnityEngine;

public class GdkGameRuntimeServiceConfigurator : IService, IStartable, ILogglable<GdkLogCategory>
{
    [SerializeField,InlineProperties] GdkPlatformSettingsData _gdkSettings;
    GdkGameRuntimeService gdkRuntimeManager;

    public IGdkRuntimeService MainService => gdkRuntimeManager;

    public void Start()
    {
        if( _gdkSettings == null )
        {
            this.LogError( "Cannot start <color=LawnGreen>GDKGameRuntime</color> because <color=LawnGreen>gdkSettings</color> is NULL" );
            return;
        }
        if( ServiceLocator.Contains<IGdkRuntimeService>() ) {
            this.LogVerbose( $"Already has a IGdkRuntimeService {ServiceLocator.Get<IGdkRuntimeService>()}" );
            return;
        }
        this.Log( "Starting <color=LawnGreen>GdkGameRuntimeService</color>..." );
        gdkRuntimeManager = new GdkGameRuntimeService( _gdkSettings.TitleId, _gdkSettings.Scid, _gdkSettings.Sandbox );
        ServiceLocator.Register( gdkRuntimeManager );
    }
}
#endif