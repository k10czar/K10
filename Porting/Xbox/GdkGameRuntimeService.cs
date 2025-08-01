#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using Unity.XGamingRuntime;
using UnityEngine;
using System;
using System.Collections;
using K10.DebugSystem;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using WebSocketSharp;

public class GdkUserData
{
    public XUserHandle userHandle;
    public XUserLocalId localId;
    public ulong userXUID;
    public string userGamertag;
    public XblContextHandle contextHandle;
    public XStoreContext storeContext;
    public Privileges Privileges = new();
}

#region Privileges
public class Privileges
{
    public bool AlreadyRead = false;
    public readonly Privilege Multiplayer = new();
    public readonly Privilege Crossplay = new();
    public readonly Privilege UserGeneratedContent = new();
    public readonly Privilege Communications = new();

    public class Privilege
    {
        public XUserPrivilege xUserPrivilege;
        public bool readed;
        public int hr;
        public XUserPrivilegeDenyReason denyReason;
        public bool isEnabled;
    }

    public void ReadUserPrivileges( XUserHandle userHandle )
    {
        ReadPrivilege( userHandle, XUserPrivilege.CrossPlay, Crossplay );
        ReadPrivilege( userHandle, XUserPrivilege.Multiplayer, Multiplayer );
        ReadPrivilege( userHandle, XUserPrivilege.UserGeneratedContent, UserGeneratedContent );

        ReadPrivilege( userHandle, XUserPrivilege.Communications, Communications );

        AlreadyRead = true;
    }

    int ReadPrivilege( XUserHandle userHandle, XUserPrivilege privilegeType, Privilege privilege )
    {
        var hr = SDK.XUserCheckPrivilege(userHandle, XUserPrivilegeOptions.None, privilegeType, out privilege.isEnabled, out privilege.denyReason);
        
        var failed = HR.FAILED( hr );
        privilege.readed = !failed;
        privilege.hr = hr;
        privilege.xUserPrivilege = privilegeType;

        if( failed )
        {
            Debug.LogError($"Failed to check Privilege {privilegeType} reason {privilege.denyReason}. HR {hr} - {HR.NameOf(hr)}");
            return hr;
        }
            
        Debug.Log($"Check Privilege <color=magenta>{privilegeType}</color>:<color=cyan>{privilege.isEnabled}</color>. HR {hr} - {HR.NameOf(hr)}");
        return hr;
    }
}
#endregion

public interface IGdkRuntimeService : IService, IGdkRuntimeData
{
    IBoolStateObserver IsInitialized { get; }
    GdkUserData UserData { get; }
}

public interface IGdkRuntimeData
{
    string Sandbox { get; }
    string Scid { get; }
    string SaveScid { get; }
    string TitleId { get; }
    uint TitleIdNumeric { get; }
}

public class GdkLogCategory : IK10LogCategory
{
    public string Name => "GDK";
    public Color Color => Colors.LawnGreen;
}

public class GdkGameRuntimeService : IGdkRuntimeService, ILoggable<GdkLogCategory>
{
    // Config
    public string Sandbox { get; private set; } = "XDKS.1";
    // Documented as: "Specifies the SCID to be used for Save Game Storage."
    public string Scid { get; private set; } = "00000000-0000-0000-0000-0000FFFFFFFF";
    public string SaveScid { get; private set; } = "00000000-0000-0000-0000-0000FFFFFFFF";

    // Documented as: "...a default value of 'FFFFFFFF' for this element. This allows for early iteration of your
    //   title without having to immediately acquire the Id from Partner Center. It is strongly recommended to change
    //   this Id as soon as you get your title building to avoid failures when attempting to do API calls."
    public string TitleId { get; private set; } = "FFFFFFFF";
    public uint TitleIdNumeric { get; private set; } = 0;

    // Initialization
    public static GdkGameRuntimeService Instance => ServiceLocator.Get<GdkGameRuntimeService>();
    private BoolState _isInitialized = new BoolState( false );
    public IBoolStateObserver IsInitialized => _isInitialized;
    
    private BoolState _isLogged = new BoolState( false );
    public IBoolStateObserver IsLogged => _isLogged;
    
    private BoolState _isReady = new BoolState( false );
    public IBoolStateObserver IsReady => _isReady;

    IBoolStateObserver _isReadyToUse = null;
    public IBoolStateObserver IsFullyInitialized 
    { 
        get
        {
            if( _isReadyToUse == null ) _isReadyToUse = new BoolStateOperations.And( _isInitialized, _isLogged, _isReady );
            return _isReadyToUse;
        } 
    }
    public IEventRegister OnStartedCleanUp => _onStartedCleanUp;
    private EventSlot _onStartedCleanUp = new();

    private bool _hasCreatedDispatchTask;
    
    private XGameSaveFilesFileAdapter _gdkFileAdapter;
    private GdkUserData _userData;
    public GdkUserData UserData => _userData;

    // Invites
    private XGameInviteRegistrationToken _inviteRegistrationToken;
    private EventSlot<string> _onReceivedInvite = new();
    public IEventRegister<string> OnReceivedInvite => _onReceivedInvite;
    private Coroutine _earlyInviteHandlingCoroutine;

    // DLC
    private HashSet<string> _dlcsWithValidLicense = new();


    // Controller related
#if UNITY_GAMECORE
    private XUserDeviceAssociationChangedRegistrationToken _deviceAssociationChangedRegistrationToken;
    private GXDKAppLocalDeviceId _activeControllerDeviceId;
    public GXDKAppLocalDeviceId ActiveControllerDeviceId => _activeControllerDeviceId;
    private EventSlot<XUserDeviceAssociationChange> _onDeviceAssiociationChanged = new();
    public IEventRegister<XUserDeviceAssociationChange> OnDeviceAssiociationChanged => _onDeviceAssiociationChanged;
    private EventSlot<GXDKAppLocalDeviceId> _onUpdatedActiveController = new();
    public IEventRegister<GXDKAppLocalDeviceId> OnUpdatedActiveController => _onUpdatedActiveController;
    private bool _showingPairControllerUI;
#endif

    public void ShowFriendData(ulong friendID)
    {
        SDK.XGameUiShowPlayerProfileCardAsync(_userData.userHandle, friendID, (result) => Debug.Log($"{result}"));
    }
 
#region Initialization
    public GdkGameRuntimeService( string scid, string saveScid = "", string titleId = "", string sandbox = "" )
    {
        if (saveScid.IsNullOrEmpty())
            saveScid = scid;

        TitleIdNumeric = uint.Parse(titleId, System.Globalization.NumberStyles.HexNumber);
		Debug.Log( $"<color=Crimson>GdkGameRuntimeService</color>( {titleId}({TitleIdNumeric}), {scid} (sav: {saveScid}), {sandbox} )" );
        if( !string.IsNullOrEmpty(sandbox) ) Sandbox = sandbox;
        TitleId = titleId;
        Scid = scid;
        SaveScid = saveScid;
        this.Log($"<color=LawnGreen>GDK Xbox Live</color> API SCID: {Scid}");
        this.Log($"<color=LawnGreen>GDK Xbox Live</color> Save SCID: {SaveScid}");
        this.Log($"<color=LawnGreen>GDK</color> TitleId: {TitleId}");
        this.Log($"<color=LawnGreen>GDK</color> Sandbox: {Sandbox}");
        InitializeRuntime();

        _gdkFileAdapter = new();
        FileAdapter.SetImplementation(_gdkFileAdapter); 
        _gdkFileAdapter.IsInitilized.Synchronize( _isReady );

        AddDefaultUser();

        SDK.XGameInviteRegisterForEvent(HandleReceivedInvite, out _inviteRegistrationToken);
#if UNITY_GAMECORE
        SDK.XUserRegisterForDeviceAssociationChanged(
            new XTaskQueueHandle(System.IntPtr.Zero),
            IntPtr.Zero,
            HandleDeviceAssociationChange,
            out _deviceAssociationChangedRegistrationToken
        );	
#endif

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += EDITOR_CleanUp;
#endif
    }
   
    private bool InitializeRuntime()
    {
        if (HR.FAILED(InitializeGamingRuntime()) || !InitializeXboxLive(Scid))
        {
            _isInitialized.SetFalse();
            return false;
        }

        // Not necessary but handy to know when debugging
        int hResult = SDK.XGameGetXboxTitleId(out uint titleIdNumeric);
        if (HR.FAILED(hResult))
            this.Log($"FAILED: Could not get TitleID! hResult: 0x{hResult:x} ({HR.NameOf(hResult)})");

        if (!TitleId.IsNullOrEmpty() && !titleIdNumeric.ToString("X").ToLower().Equals(TitleId.ToLower()))
            this.LogVerbose($"WARNING! Expected Title Id: {TitleId} got: {titleIdNumeric:X}");

        TitleId = titleIdNumeric.ToString("X");
        TitleIdNumeric = titleIdNumeric;

        hResult = SDK.XSystemGetXboxLiveSandboxId(out var sandboxId);
        if (HR.FAILED(hResult))
            this.Log($"FAILED: Could not get SandboxID! HResult: 0x{hResult:x} ({HR.NameOf(hResult)})");

        if (!Sandbox.IsNullOrEmpty() && !sandboxId.Equals(Sandbox))
            this.LogVerbose($"WARNING! Expected sandbox Id: {Sandbox} got: {sandboxId}");

        Sandbox = sandboxId;

        this.Log($"<color=LawnGreen>GDK</color> Initialized, titleId: {TitleId}, sandboxId: {Sandbox}");

        ExternalCoroutine.StartCoroutine(UpdateCoroutine());

        // Done!
        _isInitialized.SetTrue();
        return true;
    }

    private int InitializeGamingRuntime()
    {
        // We do not want stack traces for all log statements. (Exceptions logged
        // with Debug.LogException will still have stack traces though.):
        //Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        this.Log("Initializing XGame Runtime Library.");

        if (IsInitialized.Value)
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
        if (_hasCreatedDispatchTask)
            return;

        int hResult = SDK.CreateDefaultTaskQueue();
        if (HR.FAILED(hResult))
        {
            this.Log($"FAILED: XTaskQueueCreate, HResult: 0x{hResult:X}");
            return;
        }

        _hasCreatedDispatchTask = true;
    }
#endregion
    
#region Update
    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            Update();
            yield return null;
        }
    }

    private void Update()
    {
        DispatchGDKEvents();
    }

    private void DispatchGDKEvents()
    {
        if (_hasCreatedDispatchTask)
            SDK.XTaskQueueDispatch(0);
    }
    
#endregion

#region User
    public void AddDefaultUser()
    {
        _userData = new GdkUserData();
        SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserSilently, (Int32 hresult, XUserHandle userHandle) =>
        {
            if (HR.FAILED(hresult) || userHandle == null)
            {
                Debug.LogError($"Couldnt add default user {hresult}");
                return;
            }

            _gdkFileAdapter.Initialize(userHandle, SaveScid);
            InitializeUser(userHandle);
            _isLogged.SetTrue();
        });
    }

    private void InitializeUser(XUserHandle userHandle)
    {
        Debug.Log("Initializing user");
        _userData.userHandle = userHandle;

        FetchUserData();
    }

    private void FetchUserData()
    {
        GetUserId();
        GetUserLocalId();
        GetUserContext();
        GetGamertag();
        GetStoreContext();

        _userData.Privileges.ReadUserPrivileges( _userData.userHandle );
        // GetUserPrivileges(); 
    }

    private int GetUserId()
    {
        int hr = SDK.XUserGetId(UserData.userHandle, out _userData.userXUID);
        if (HR.FAILED(hr))
            Debug.LogError($"Couldn't Get User ID {hr}");

        return hr;
    }

    private int GetUserContext()
    {
        int hr = SDK.XBL.XblContextCreateHandle(_userData.userHandle, out _userData.contextHandle);
        if (HR.FAILED(hr) || _userData.contextHandle == null)
            Debug.LogError("Error creating context handle");

        return hr;
    }

    private int GetGamertag()
    {
        int hr = SDK.XUserGetGamertag(_userData.userHandle, XUserGamertagComponent.Classic, out _userData.userGamertag);
        if (HR.FAILED(hr))
            Debug.LogError($"Failed to get Gamertag. HR {hr} - {HR.NameOf(hr)}");

        return hr;
    }

    private int GetUserLocalId()
    {
        int hr = SDK.XUserGetLocalId(_userData.userHandle, out _userData.localId);
        if (HR.FAILED(hr))
            Debug.LogError($"Failed to get LocaId. HR {hr} - {HR.NameOf(hr)}");

        return hr;
    }

    private int GetStoreContext()
    {
        int hr = SDK.XStoreCreateContext(UserData.userHandle, out _userData.storeContext);
        if (HR.FAILED(hr) || _userData.storeContext == null)
            Debug.LogError($"CreateStoreContext failed - 0x{hr:X8}.");

        return hr;
    }
#endregion

#region DLC
    public void FetchDLCLicense(string storeId)
    {
        if (storeId.IsNullOrEmpty())
            return;

        SDK.XStoreAcquireLicenseForDurablesAsync(UserData.storeContext, storeId,
            (Int32 hresult, XStoreLicense license) =>
            {
                if (HR.FAILED(hresult) || license == null)
                {
                    if (HR.FAILED(hresult))
                        Debug.LogError($"Failed to get DLC license {storeId}: HR = {hresult}");

                    return;
                }

                if (SDK.XStoreIsLicenseValid(license))
                    _dlcsWithValidLicense.Add(storeId);
            }
        );
    }

    //! Only checks DLC which licenses have already been checked (used to avoid async handling)
    public bool HasDLC(string storeId)
    {
        return _dlcsWithValidLicense.Contains(storeId);
    }
#endregion

#region Invites
    private void HandleReceivedInvite(IntPtr context, string inviteUri)
    {
        if (_earlyInviteHandlingCoroutine != null)
        {
            Debug.LogError($"Ignored invite because is still handling an early invite");
            return;
        }

        if (!IsFullyInitialized.Value)
        {
            _earlyInviteHandlingCoroutine = ExternalCoroutine.StartCoroutine(HandleEarlyInvite(inviteUri));
            return;
        }

        TriggerReceivedInviteEvent(inviteUri);
    }

    private IEnumerator HandleEarlyInvite(string inviteUri)
    {
        Debug.Log($"Invite received, but waiting for login to handle it");
        yield return new WaitUntil(() => IsFullyInitialized.Value);
        yield return new WaitForEndOfFrame();

        TriggerReceivedInviteEvent(inviteUri);
        _earlyInviteHandlingCoroutine = null;
    }

    private void TriggerReceivedInviteEvent(string inviteUri)
    {
        Debug.Log($"Triggering invite event: {inviteUri}");
        _onReceivedInvite.Trigger(inviteUri);
    }
#endregion

#region Controller
#if UNITY_GAMECORE
    private void HandleDeviceAssociationChange(IntPtr context, ref XUserDeviceAssociationChange change)
    {
        _onDeviceAssiociationChanged.Trigger(change);
    }
    
    public void PairControllerToUser()
    {
        if (_showingPairControllerUI)
            return;

        _showingPairControllerUI = true;

        SDK.XUserFindControllerForUserWithUiAsync(UserData.userHandle, (Int32 Hresult, APP_LOCAL_DEVICE_ID deviceId) =>
        {
            _showingPairControllerUI = false;

            if (HR.FAILED(Hresult))
            {
                PairControllerToUser();
                return;
            }

            SetActiveController(new GXDKAppLocalDeviceId(deviceId.Value));
        });
    }

    public void SetActiveController(GXDKAppLocalDeviceId deviceId)
    {
        _activeControllerDeviceId = deviceId;
        _onUpdatedActiveController.Trigger(ActiveControllerDeviceId);
    }
#endif
#endregion

#region CleanUp
    ~GdkGameRuntimeService()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        Debug.Log($"GDK CleanUp");
        _onStartedCleanUp.Trigger();

        ExternalCoroutine.StopCoroutine(_earlyInviteHandlingCoroutine);
#if UNITY_GAMECORE
        SDK.XUserUnregisterForDeviceAssociationChanged(_deviceAssociationChangedRegistrationToken, true);
#endif
        SDK.XGameInviteUnregisterForEvent(_inviteRegistrationToken);
        SDK.XStoreCloseContextHandle(UserData.storeContext);
        SDK.XUserCloseHandle(UserData.userHandle);
        SDK.XBL.XblContextCloseHandle(UserData.contextHandle);

        Debug.Log($"GDK CleanUp finished");
    }

#if UNITY_EDITOR
    private void EDITOR_CleanUp(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.ExitingPlayMode)
            return;

        CleanUp();
        EditorApplication.playModeStateChanged -= EDITOR_CleanUp;
    }
#endif
#endregion
}
#endif