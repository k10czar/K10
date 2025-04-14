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
    
    private BoolState _isLogged = new BoolState( false );
    public IBoolStateObserver IsLogged => _isLogged;
    
    private BoolState _isReady = new BoolState( false );
    public IBoolStateObserver IsReady => _isReady;

#if UNITY_GAMECORE
    private GXDKAppLocalDeviceId _activeControllerDeviceId;
    public GXDKAppLocalDeviceId ActiveControllerDeviceId => _activeControllerDeviceId;
#endif
    
    private XUserDeviceAssociationChangedRegistrationToken _deviceAssociationChangedRegistrationToken;
    private XGameInviteRegistrationToken _inviteRegistrationToken;

    private EventSlot<XUserDeviceAssociationChange> _onDeviceAssiociationChanged = new();
    public IEventRegister<XUserDeviceAssociationChange> OnDeviceAssiociationChanged => _onDeviceAssiociationChanged;

#if UNITY_GAMECORE
    private EventSlot<GXDKAppLocalDeviceId> _onUpdatedActiveController = new();
    public IEventRegister<GXDKAppLocalDeviceId> OnUpdatedActiveController => _onUpdatedActiveController;
#endif

    IBoolStateObserver _isReadyToUse = null;
    public IBoolStateObserver IsFullyInitialized 
    { 
        get
        {
            if( _isReadyToUse == null ) _isReadyToUse = new BoolStateOperations.And( _isInitialized, _isLogged, _isReady );
            return _isReadyToUse;
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
    private bool _showingPairControllerUI;

 
    // TODO-Porting: Cleanup
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
        _gdkFileAdapter.IsInitilized.Synchronize( _isReady );

        AddUser(AddUserCompleted, true);

        // Register for the user change event with the GDK
        SDK.XUserRegisterForChangeEvent(UserChangeEventCallback, out _callbackRegistrationToken);
        SDK.XUserRegisterForDeviceAssociationChanged(
            new XTaskQueueHandle(System.IntPtr.Zero),
            IntPtr.Zero,
            HandleDeviceAssociationChange,
            out _deviceAssociationChangedRegistrationToken
        );	
        SDK.XGameInviteRegisterForEvent(InviteEventHandler, out _inviteRegistrationToken);
    }


    ~GdkGameRuntimeService()
    {
        SDK.XUserUnregisterForChangeEvent(_callbackRegistrationToken);
        SDK.XUserUnregisterForDeviceAssociationChanged(_deviceAssociationChangedRegistrationToken, true);
        SDK.XGameInviteUnregisterForEvent(_inviteRegistrationToken);

        SDK.XUserCloseHandle(UserData.userHandle);
        SDK.XBL.XblContextCloseHandle(UserData.contextHandle);
        Debug.Log($"Running GDKGameRuntimeService destructor");
    }

    private void HandleDeviceAssociationChange(IntPtr context, ref XUserDeviceAssociationChange change)
    {
        _onDeviceAssiociationChanged.Trigger(change);
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
        SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserSilently, (Int32 hresult, XUserHandle userHandle) =>
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
        _isLogged.SetTrue();

        ExternalCoroutine.StartCoroutine(DoActivityStuff());
    }

    IEnumerator DoActivityStuff()
    {
        Debug.Log($"Starting stuff");
        yield return new WaitForSeconds(3f);
        Debug.Log($"Really Starting stuff");
        
        var info  = new XblMultiplayerActivityInfo();
        info.ConnectionString = "ConnectionString";
        info.JoinRestriction = XblMultiplayerActivityJoinRestriction.Public;
        info.MaxPlayers = 4;
        info.CurrentPlayers = 1;
        info.GroupId = "PartyID";
        info.Platform = XblMultiplayerActivityPlatform.All;

        SDK.XBL.XblMultiplayerActivitySetActivityAsync(UserData.contextHandle, info, true,
            (Int32 hresult) => 
            {
                if (HR.SUCCEEDED(hresult))
                    Debug.Log($"Successfully Set Activity");
                else
                    Debug.Log($"Couldn't Set activity. HR {hresult}");

                SDK.XGameUiShowMultiplayerActivityGameInviteAsync(UserData.userHandle, (hresult) => {
                    Debug.Log($"Showing UI");
                });

                // UInt64[] xboxUserIdList = new UInt64[4];
                // SDK.XBL.XblMultiplayerActivityGetActivityAsync(UserData.contextHandle, xboxUserIdList, 
                //     (Int32 hresult, XblMultiplayerActivityInfo[] results) =>
                //     {
                //         if (results != null)
                //         {
                //             foreach (var info in results)
                //                 Debug.Log("result ai " + info.ConnectionString);
                //         }

                //         foreach (var xuser in xboxUserIdList)
                //             Debug.Log($"Found user {xuser}");

                //         SDK.XGameUiShowMultiplayerActivityGameInviteAsync(UserData.userHandle, (hresult) => {
                //             Debug.Log($"Showing UI");
                //         });
                //     }
                // );
            }
        );

    }

#if UNITY_GAMECORE
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

    private UserOpResult GetAllUserInfo(XUserHandle userHandle)
    {
        UserOpResult userOpResult = GetUserId(userHandle);
        if (userOpResult != UserOpResult.Success)
            return userOpResult;

        GetUserContext();
        GetBasicInfo();
        GetUserPrivileges(); 

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
        Debug.Log($"Gamertag = {_userData.userGamertag}");

        hr = SDK.XUserGetLocalId(_userData.userHandle, out _userData.localId);
        if (HR.FAILED(hr))
            Debug.LogError($"Failed to get LocaId. HR {hr} - {HR.NameOf(hr)}");
    }

    private void GetUserPrivileges()
    {
        // TODO-Porting: Check which privileges are needed and actually store them in UserData

        XUserPrivilegeDenyReason denyReason;
        SDK.XUserCheckPrivilege(UserData.userHandle, XUserPrivilegeOptions.None, XUserPrivilege.Multiplayer, out bool hasMultiplayerPrivilege, out denyReason);
        SDK.XUserCheckPrivilege(UserData.userHandle, XUserPrivilegeOptions.None, XUserPrivilege.CrossPlay, out bool hasCrossplayPrivilege, out denyReason);
        SDK.XUserCheckPrivilege(UserData.userHandle, XUserPrivilegeOptions.None, XUserPrivilege.UserGeneratedContent, out bool hasUGCPrivilege, out denyReason);

        // TODO-Porting: Remove, should not be needed since we are removing the chat
        SDK.XUserCheckPrivilege(UserData.userHandle, XUserPrivilegeOptions.None, XUserPrivilege.Communications, out bool hasCommunicationsPrivilege, out denyReason);

        // TODO-Porting: Check what those privileges actually mean
        SDK.XUserCheckPrivilege(UserData.userHandle, XUserPrivilegeOptions.None, XUserPrivilege.MultiplayerParties, out bool hasMultiplayerPartiesPrivilege, out denyReason);
        SDK.XUserCheckPrivilege(UserData.userHandle, XUserPrivilegeOptions.None, XUserPrivilege.Sessions, out bool hasSessionsPrivilege, out denyReason);
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


    private const string INVITE_ACTION_KEY = "://";
    private const string SENDER_KEY = "sender=";
    private const string INVITED_USER_KEY = "invitedUser=";
    private const string CONNECTION_STRING_KEY = "connectionString=";
    private const string JOINER_KEY = "joinerXuid=";
    private const string JOINEE_KEY = "joineeXuid=";
    private const string ACCEPT_INVITE_KEY = "inviteAccept";
    private const string JOIN_ACTIVITY_KEY = "activityJoin";

    private void InviteEventHandler(IntPtr context, string inviteUri)
    {
        // example invite string URI:
        // Accept invite
        // ms-xbl-6cf217a8://inviteAccept/?invitedUser=2814623999559850&sender=2814652139982289&connectionString=ConnectionString
        // ms-xbl-<titleId>://inviteAccept?invitedUser=<xuid>&sender=<xuid>&connectionString=<connectionString>

        // Activity Join
        // ms-xbl-6cf217a8://activityJoin/?joinerXuid=2814623999559850&connectionString=ConnectionString&joineeXuid=2814652139982289


        Debug.Log($"Invite URI: {inviteUri}");

        int inviteActionStart = inviteUri.IndexOf(INVITE_ACTION_KEY) + INVITE_ACTION_KEY.Length;
        int inviteActionEnd = inviteUri.IndexOf("?", inviteActionStart);
        string inviteAction = inviteUri.Substring(inviteActionStart, inviteActionEnd - inviteActionStart);

        string userInPartyKey, invitedUserKey;
        switch (inviteAction)
        {
            case ACCEPT_INVITE_KEY:
            case ACCEPT_INVITE_KEY + "/":
                userInPartyKey = SENDER_KEY;
                invitedUserKey = INVITED_USER_KEY;
                break;
                
            case JOIN_ACTIVITY_KEY:
            case JOIN_ACTIVITY_KEY + "/":
                userInPartyKey = JOINEE_KEY;
                invitedUserKey = JOINER_KEY;
                break;
            
            default:
                Debug.LogError($"Found unhandled invite action: {inviteAction}");
                return;
        }


        int userInPartyStart = inviteUri.IndexOf(userInPartyKey) + userInPartyKey.Length;
        int userInPartyEnd = inviteUri.IndexOf("&", userInPartyStart);
        userInPartyEnd = (userInPartyEnd == -1) ? inviteUri.Length : userInPartyEnd;
        string userInParty = inviteUri.Substring(userInPartyStart, userInPartyEnd - userInPartyStart);

        int invitedUserStart = inviteUri.IndexOf(invitedUserKey) + invitedUserKey.Length;
        int invitedUserEnd = inviteUri.IndexOf("&", invitedUserStart);
        invitedUserEnd = (invitedUserEnd == -1) ? inviteUri.Length : invitedUserEnd;
        string invitedUser = inviteUri.Substring(invitedUserStart, invitedUserEnd - invitedUserStart);

        int connectionStringStart = inviteUri.IndexOf(CONNECTION_STRING_KEY) + CONNECTION_STRING_KEY.Length;
        int connectionStringEnd = inviteUri.IndexOf("&", connectionStringStart);
        connectionStringEnd = (connectionStringEnd == -1) ? inviteUri.Length : connectionStringEnd;
        string connectionString = inviteUri.Substring(connectionStringStart, connectionStringEnd - connectionStringStart);

        HandleInviteReceived(UInt64.Parse(userInParty), UInt64.Parse(invitedUser), connectionString);
    }

    private void HandleInviteReceived(ulong userInParty, ulong invitedUser, string connectionString)
    {
        Debug.Log($"Handling Invite from {userInParty} to {invitedUser} with connection string {connectionString}");
        Debug.Log($"Invite to {invitedUser} == {UserData.userXUID} ? {invitedUser == UserData.userXUID}");

        // TODO? we are responding to an invite so we do not need to listen anymore
        // TODO: Check Privileges
        // var hasMultiplayerPrivileges = XboxLive.HasMultiplayerPrivileges;
        // var hasMultiplayerInvite = XboxLive.HasMultiplayerInvite;

        // TODO: Check if is already logged in

        // JoinInviteButton.interactable = hasMultiplayerPrivileges && hasMultiplayerInvite;
    }

}
#endif