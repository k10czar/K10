#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using Unity.XGamingRuntime;
using UnityEngine;
using System;
using System.Collections;
using K10.DebugSystem;

public class GdkUserData
{
    public XUserHandle userHandle;
    public XUserLocalId localId;
    public ulong userXUID;
    public string userGamertag;
    public XblPermissionCheckResult canPlayMultiplayer;
    public XblContextHandle contextHandle;
}

public interface IGdkRuntimeService : IService, IGdkRuntimeData
{
    IBoolStateObserver IsInitialized { get; }
    GdkUserData UserData { get; }
}

public interface IGdkRuntimeData
{
    string Sandbox { get; }
    string Scid { get; }
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
    public enum UserOpResult
    {
        Success,
        NoDefaultUser,
        ResolveUserIssueRequired,
        UnclearedVetoes,
        UnknownError
    }

    public string Sandbox { get; private set; } = "XDKS.1";
    // Documented as: "Specifies the SCID to be used for Save Game Storage."
    public string Scid { get; private set; } = "00000000-0000-0000-0000-0000FFFFFFFF";

    // Documented as: "...a default value of 'FFFFFFFF' for this element. This allows for early iteration of your
    //   title without having to immediately acquire the Id from Partner Center. It is strongly recommended to change
    //   this Id as soon as you get your title building to avoid failures when attempting to do API calls."
    public string TitleId { get; private set; } = "FFFFFFFF";
    public uint TitleIdNumeric { get; private set; } = 0;

    private BoolState _isInitialized = new BoolState( false );
    public IBoolStateObserver IsInitialized => _isInitialized;
    
    private BoolState _isLoggedRaw = new BoolState( false );
    public IBoolStateObserver IsLogged => _isLoggedRaw;

    IBoolStateObserver _isInitializedAndLogged = null;
    public IBoolStateObserver IsInitializedAndLogged 
    { 
        get
        {
            if( _isInitializedAndLogged == null ) new BoolStateOperations.And( _isInitialized, _isLoggedRaw );
            return _isInitializedAndLogged;
        } 
    }


    public GdkGameRuntimeService Instance => ServiceLocator.Get<GdkGameRuntimeService>();
    private XGameSaveFilesFileAdapter _gdkFileAdapter;
    private Coroutine _dispatchCoroutine;

    public delegate void AddUserCompletedDelegate(UserOpResult result);
    public event EventHandler<XUserChangeEvent> UsersChanged;

    private GdkUserData _userData;
    public GdkUserData UserData => _userData;

    private AddUserCompletedDelegate _currentCompletionDelegate;
    private XUserChangeRegistrationToken _callbackRegistrationToken;
 
    public GdkGameRuntimeService( string titleId = "62ab3c24", string scid = "00000000-0000-0000-0000-000062ab3c24", string sandbox = "" )
    {
        TitleIdNumeric = uint.Parse(titleId, System.Globalization.NumberStyles.HexNumber);
		Debug.Log( $"<color=Crimson>GdkGameRuntimeService</color>( {titleId}({TitleIdNumeric}), {scid}, {sandbox} )" );
        if( !string.IsNullOrEmpty(sandbox) ) Sandbox = sandbox;
        TitleId = titleId;
        Scid = scid;
        this.Log($"<color=LawnGreen>GDK Xbox Live</color> API SCID: {Scid}");
        this.Log($"<color=LawnGreen>GDK</color> TitleId: {TitleId}");
        this.Log($"<color=LawnGreen>GDK</color> Sandbox: {Sandbox}");
        InitializeRuntime();

        _gdkFileAdapter = new XGameSaveFilesFileAdapter();
        FileAdapter.SetImplementation(_gdkFileAdapter); 

        AddUser(AddUserCompleted, true);

        // Register for the user change event with the GDK
        SDK.XUserRegisterForChangeEvent(UserChangeEventCallback, out _callbackRegistrationToken);
    }

    ~GdkGameRuntimeService()
    {
        SDK.XUserUnregisterForChangeEvent(_callbackRegistrationToken);
    }
    
    private bool InitializeRuntime(bool forceInitialization = false)
    {
        if (HR.FAILED(InitializeGamingRuntime(forceInitialization)) ||
            !InitializeXboxLive(Scid))
        {
            _isInitialized.SetFalse();
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
        _isInitialized.SetTrue();
        return true;
    }

    private int InitializeGamingRuntime(bool forceInitialization = false)
    {
        // We do not want stack traces for all log statements. (Exceptions logged
        // with Debug.LogException will still have stack traces though.):
        //Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        this.Log("Initializing XGame Runtime Library.");

        if( IsInitialized.Value && !forceInitialization )
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
            // We need to execute SDK.XTaskQueueDispatch(0) to pump all GDK events.
            SDK.XTaskQueueDispatch(0);
            yield return null;
        }
    }

    public bool AddUser(AddUserCompletedDelegate completionDelegate, bool silently)
    {
        if (silently)
            return AddDefaultUserSilently( completionDelegate );
        return AddUserWithUI( completionDelegate );
    }

    // TODO: Maybe merge with function below
    //Adding User Silently
    public bool AddDefaultUserSilently(AddUserCompletedDelegate completionDelegate)
    {
        _userData = new GdkUserData();
        _currentCompletionDelegate = completionDelegate;
        SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, (Int32 hresult, XUserHandle userHandle) =>
        {
            if (HR.SUCCEEDED(hresult) && userHandle != null)
            {
                Debug.Log("AddUser complete " + hresult + " user handle " + userHandle.GetHashCode());

                // Call XUserGetId here to ensure all vetos (privacy consent, gametag banned, etc) have passed
                UserOpResult result = GetAllUserInfo(userHandle);
                if (result == UserOpResult.ResolveUserIssueRequired)
                {
                    ResolveSigninIssueWithUI(userHandle, _currentCompletionDelegate);
                }
                else
                {
                    _currentCompletionDelegate(result);
                }
            }
            else if (hresult == HR.E_GAMEUSER_NO_DEFAULT_USER)
                _currentCompletionDelegate(UserOpResult.NoDefaultUser);
            else
                _currentCompletionDelegate(UserOpResult.UnknownError);
        });

        return true;
    }

    public bool AddUserWithUI(AddUserCompletedDelegate completionDelegate)
    {
        _userData = new GdkUserData();
        _currentCompletionDelegate = completionDelegate;

        SDK.XUserAddAsync(XUserAddOptions.None, (Int32 hresult, XUserHandle userHandle) =>
        {
            if (HR.SUCCEEDED(hresult) && userHandle != null)
            {
                Debug.Log("AddUserWithUI complete " + hresult + " user handle " + userHandle.GetHashCode());

                // Call XUserGetId here to ensure all vetos (privacy consent, gametag banned, etc) have passed
                UserOpResult result = GetAllUserInfo(userHandle);
                if (result == UserOpResult.ResolveUserIssueRequired)
                {
                    ResolveSigninIssueWithUI(userHandle, _currentCompletionDelegate);
                }
                else
                {
                    _currentCompletionDelegate(result);
                }
            }
            else if (userHandle != null)
            {
                // Failed to log in, try to resolve issue
                ResolveSigninIssueWithUI(userHandle, _currentCompletionDelegate);
            }
            else
            {
                // TODO: Maybe it is needed to show again? Idk
                Debug.Log("Got empty user handle back from AddUserWithUI."); 
                _currentCompletionDelegate(UserOpResult.UnknownError);
            }
        });

        return true;
    }

    //Resolve sign in issue - lauches  UI to sign in users
    private void ResolveSigninIssueWithUI(XUserHandle userHandle, AddUserCompletedDelegate completionDelegate)
    {
        SDK.XUserResolveIssueWithUiUtf16Async(userHandle, null, (Int32 resolveHResult) =>
        {
            if (HR.SUCCEEDED(resolveHResult))
                GetAllUserInfo(userHandle);
            else
            {
                // User has uncleared vetoes.  The game should decide how to handle this,
                // either by gracefully continuing or dropping user back to title screen to
                // with "Press 'A' or 'Enter' to continue" to select a new user.
                completionDelegate(UserOpResult.UnclearedVetoes);
            }
        });
    }

    private void AddUserCompleted(UserOpResult result)
    {
        Debug.Log($"Add user completed {result}");
        if (result != UserOpResult.Success)
            Debug.LogError($"Add User Complete failed. UserOpResult = {result}");

        _gdkFileAdapter.Initialize(_userData.userHandle, Scid);
        _isLoggedRaw.SetTrue();
    }

    private UserOpResult GetAllUserInfo(XUserHandle userHandle)
    {
        UserOpResult userOpResult = GetUserId(userHandle);
        if (userOpResult != UserOpResult.Success)
            return userOpResult;

        GetUserContext();
        GetBasicInfo();
        // TODO: This is async, if we are really going to use it, we should wait for its result and check
        GetUserMultiplayerPermissions(); 

        return userOpResult;
    }

    // Get User ID
    private UserOpResult GetUserId(XUserHandle userHandle)
    {
        ulong xuid;
        int hr = SDK.XUserGetId(userHandle, out xuid);
        if (HR.SUCCEEDED(hr))
        {
            _userData.userHandle = userHandle;
            _userData.userXUID = xuid;
            return UserOpResult.Success;
        }
        else if (hr == HR.E_GAMEUSER_RESOLVE_USER_ISSUE_REQUIRED)
        {
            return UserOpResult.ResolveUserIssueRequired;
        }

        return UserOpResult.UnknownError;
    }

    private void GetBasicInfo()
    {
        int hr = SDK.XUserGetGamertag(_userData.userHandle, XUserGamertagComponent.Classic, out _userData.userGamertag);
        if (HR.FAILED(hr))
            Debug.LogError($"Failed to get Gamertag. HR {hr} - {HR.NameOf(hr)}");

        hr = SDK.XUserGetLocalId(_userData.userHandle, out _userData.localId);
        if (HR.FAILED(hr))
            Debug.LogError($"Failed to get LocaId. HR {hr} - {HR.NameOf(hr)}");
    }

    private void GetUserMultiplayerPermissions()
    {
        SDK.XBL.XblPrivacyCheckPermissionAsync(_userData.contextHandle, XblPermission.PlayMultiplayer,
            _userData.userXUID, (Int32 hresult, XblPermissionCheckResult result) =>
            {
                if (HR.SUCCEEDED(hresult))
                    _userData.canPlayMultiplayer = result;
                else
                    Debug.Log("Failed to get user multiplayer permission");
            });
    }

    private void GetUserContext()
    {
        int hr = SDK.XBL.XblContextCreateHandle(_userData.userHandle, out _userData.contextHandle);
        if (HR.SUCCEEDED(hr) && _userData.contextHandle != null)
            Debug.Log("Success XBL and Context");
        else
            Debug.Log("Error creating context");
    }

    private void UserChangeEventCallback(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
    {
        if (eventType == XUserChangeEvent.SignedOut)
        {
            Debug.LogWarning("User logging out");
        }

        if (eventType != XUserChangeEvent.SignedInAgain)
        {
            EventHandler<XUserChangeEvent> handler = UsersChanged;
            handler?.Invoke(this, eventType);
        }
    }
}
#endif