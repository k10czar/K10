using System.Collections;
using Unity.XGamingRuntime;
using UnityEngine;

public interface IGdkRuntimeService : IService, IGdkRuntimeData
{
    IBoolStateObserver Initialized { get; }
}

public interface IGdkRuntimeData
{
    string Sandbox { get; }
    string Scid { get; }
    string TitleId { get; }
}

public class GdkLogCategory : IK10LogCategory
{
    public string Name => "GDK";
    public Color Color => Colors.LawnGreen;
}

public class GdkGameRuntimeService : IGdkRuntimeService, ILogglable<GdkLogCategory>
{
    public string Sandbox { get; private set; } = "XDKS.1";
    // Documented as: "Specifies the SCID to be used for Save Game Storage."
    public string Scid { get; private set; } = "00000000-0000-0000-0000-0000FFFFFFFF";

    // Documented as: "...a default value of 'FFFFFFFF' for this element. This allows for early iteration of your
    //   title without having to immediately acquire the Id from Partner Center. It is strongly recommended to change
    //   this Id as soon as you get your title building to avoid failures when attempting to do API calls."
    public string TitleId { get; private set; } = "FFFFFFFF";

    public BoolState InitializedRaw { get; private set; } = new BoolState( false );
    public IBoolStateObserver Initialized => InitializedRaw;
    public GdkGameRuntimeService Instance => ServiceLocator.Get<GdkGameRuntimeService>();

    private Coroutine _dispatchCoroutine;
    
    private UserManager m_UserManager;
    
    public GdkGameRuntimeService( string titleId = "62ab3c24", string scid = "00000000-0000-0000-0000-000062ab3c24", string sandbox = "" )
    {
        if( !string.IsNullOrEmpty(sandbox) ) Sandbox = sandbox;
        TitleId = titleId;
        Scid = scid;
        this.Log($"<color=LawnGreen>GDK Xbox Live</color> API SCID: {Scid}");
        this.Log($"<color=LawnGreen>GDK</color> TitleId: {TitleId}");
        this.Log($"<color=LawnGreen>GDK</color> Sandbox: {Sandbox}");
        InitializeRuntime();

        if (m_UserManager == null)
        {
            m_UserManager = new UserManager();
        }

        m_UserManager.UsersChanged += UserManager_UsersChanged;
    }
    
    private bool m_UsersChanged;

    private void UserManager_UsersChanged(object sender, XUserChangeEvent e)
    {
        m_UsersChanged = true;
    }

    private bool InitializeRuntime(bool forceInitialization = false)
    {
        if (HR.FAILED(InitializeGamingRuntime(forceInitialization)) ||
            !InitializeXboxLive(Scid))
        {
            InitializedRaw.SetFalse();
            return false;
        }

        // Not necessary but handy to know when debugging
        int hResult = SDK.XGameGetXboxTitleId(out var titleId);
        if (HR.FAILED(hResult))
        {
            this.Log($"FAILED: Could not get TitleID! hResult: 0x{hResult:x} ({HR.NameOf(hResult)})");
        }

        if (titleId.ToString("X").ToLower().Equals(TitleId.ToLower()) == false)
        {
            this.LogVerbose($"WARNING! Expected Title Id: {TitleId} got: {titleId:X}");
        }

        TitleId = titleId.ToString("X");

        hResult = SDK.XSystemGetXboxLiveSandboxId(out var sandboxId);
        if (HR.FAILED(hResult))
        {
            this.Log($"FAILED: Could not get SandboxID! HResult: 0x{hResult:x} ({HR.NameOf(hResult)})");
        }

        if (sandboxId.Equals(Sandbox) == false)
        {
            this.LogVerbose($"WARNING! Expected sandbox Id: {Sandbox} got: {sandboxId}");
        }

        Sandbox = sandboxId;

        this.Log($"<color=LawnGreen>GDK</color> Initialized, titleId: {TitleId}, sandboxId: {Sandbox}");

        // Done!
        InitializedRaw.SetTrue();
        return true;
    }

    private int InitializeGamingRuntime(bool forceInitialization = false)
    {
        // We do not want stack traces for all log statements. (Exceptions logged
        // with Debug.LogException will still have stack traces though.):
        //Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        this.Log("Initializing XGame Runtime Library.");

        if( Initialized.Value && !forceInitialization )
        {
            this.Log("Gaming Runtime already initialized.");
            return 0;
        }

        int hResult = SDK.XGameRuntimeInitialize();
        if (HR.FAILED(hResult))
        {
            this.Log($"FAILED: Initialize XGameRuntime, HResult: 0x{hResult:X} ({HR.NameOf(hResult)})");
            return hResult;
        }

        StartAsyncDispatchCoroutine();

        return 0;
    }

    private bool InitializeXboxLive(string scid)
    {
        this.Log($"Initializing Xbox Live API (SCID: {scid}).");

        int hResult = SDK.XBL.XblInitialize(scid);
        if (HR.FAILED(hResult) && hResult != HR.E_XBL_ALREADY_INITIALIZED)
        {
            this.Log($"FAILED: Initialize Xbox Live, HResult: 0x{hResult:X}, {HR.NameOf(hResult)}");
            return false;
        }

        return true;
    }

    private void StartAsyncDispatchCoroutine()
    {
        if (_dispatchCoroutine == null)
        {
            int hResult = SDK.CreateDefaultTaskQueue();
            if (HR.FAILED(hResult))
            {
                this.Log($"FAILED: XTaskQueueCreate, HResult: 0x{hResult:X}");
                return;
            }

            _dispatchCoroutine = ExternalCoroutine.StartCoroutine(DispatchGDKTaskQueue());
        }
    }

    private IEnumerator DispatchGDKTaskQueue()
    {
        while (true)
        {
            if (m_UserManager != null)
                m_UserManager.Update();
            // We need to execute SDK.XTaskQueueDispatch(0) to pump all GDK events.
            SDK.XTaskQueueDispatch(0);
            yield return null;
        }
    }
}
