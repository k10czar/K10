#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using Unity.XGamingRuntime;
using UnityEngine;
using System;
using System.Collections;
using K10.DebugSystem;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
    // TODO-Porting: Set values, just in case
    // Config
    public string Sandbox { get; private set; } = "XDKS.1";
    // Documented as: "Specifies the SCID to be used for Save Game Storage."
    public string Scid { get; private set; } = "00000000-0000-0000-0000-0000FFFFFFFF";

    // Documented as: "...a default value of 'FFFFFFFF' for this element. This allows for early iteration of your
    //   title without having to immediately acquire the Id from Partner Center. It is strongly recommended to change
    //   this Id as soon as you get your title building to avoid failures when attempting to do API calls."
    public string TitleId { get; private set; } = "FFFFFFFF";
    public uint TitleIdNumeric { get; private set; } = 0;

    // Initialization
    public GdkGameRuntimeService Instance => ServiceLocator.Get<GdkGameRuntimeService>();
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
    private bool _hasCreatedDispatchTask;
    
    private XGameSaveFilesFileAdapter _gdkFileAdapter;
    private GdkUserData _userData;
    public GdkUserData UserData => _userData;

    // Social
    private const string INVITE_ACTION_KEY = "://";
    private const string SENDER_KEY = "sender=";
    private const string INVITED_USER_KEY = "invitedUser=";
    private const string CONNECTION_STRING_KEY = "connectionString=";
    private const string JOINER_KEY = "joinerXuid=";
    private const string JOINEE_KEY = "joineeXuid=";
    private const string ACCEPT_INVITE_KEY = "inviteAccept";
    private const string JOIN_ACTIVITY_KEY = "activityJoin";
    private XGameInviteRegistrationToken _inviteRegistrationToken;
    private const XblSocialManagerExtraDetailLevel SOCIAL_EXTRA_DETAIL_LEVEL = XblSocialManagerExtraDetailLevel.TitleHistoryLevel;
    private XblSocialManagerUserGroupHandle _friendsFilter = null;
    private List<XblSocialManagerUser> _friendsList = new();
    public ReadOnlyCollection<XblSocialManagerUser> FriendsList => _friendsList.AsReadOnly();
    private EventSlot<ReadOnlyCollection<XblSocialManagerUser>> _onFriendsListUpdated = new();
    public IEventRegister<ReadOnlyCollection<XblSocialManagerUser>> OnFriendsListUpdated => _onFriendsListUpdated;
    public bool ShouldUpdateFriendsList; // TODO-Porting: Do as lock/unlock in case many want to listen to this


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
 
#region Initialization
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

        _gdkFileAdapter = new();
        FileAdapter.SetImplementation(_gdkFileAdapter); 
        _gdkFileAdapter.IsInitilized.Synchronize( _isReady );

        AddDefaultUser();

        SDK.XGameInviteRegisterForEvent(InviteEventHandler, out _inviteRegistrationToken);
#if UNITY_GAMECORE
        SDK.XUserRegisterForDeviceAssociationChanged(
            new XTaskQueueHandle(System.IntPtr.Zero),
            IntPtr.Zero,
            HandleDeviceAssociationChange,
            out _deviceAssociationChangedRegistrationToken
        );	
#endif
    }

    ~GdkGameRuntimeService()
    {
#if UNITY_GAMECORE
        SDK.XUserUnregisterForDeviceAssociationChanged(_deviceAssociationChangedRegistrationToken, true);
#endif
        SDK.XGameInviteUnregisterForEvent(_inviteRegistrationToken);
        SDK.XBL.XblSocialManagerRemoveLocalUser(UserData.userHandle, SOCIAL_EXTRA_DETAIL_LEVEL);
        SDK.XUserCloseHandle(UserData.userHandle);
        SDK.XBL.XblContextCloseHandle(UserData.contextHandle);
    }
    
    private bool InitializeRuntime()
    {
        if (HR.FAILED(InitializeGamingRuntime()) || !InitializeXboxLive(Scid))
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
        if (ShouldUpdateFriendsList)
            UpdateFriendsList();
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

            _isLogged.SetTrue();
            _gdkFileAdapter.Initialize(_userData.userHandle, Scid);
            InitializeUser(userHandle);
        });
    }

    private void InitializeUser(XUserHandle userHandle)
    {
        Debug.Log("Initializing user");
        _userData.userHandle = userHandle;

        FetchUserData();
        InitializeSocial();
        
        // TODO-Porting: Remove this
        // ExternalCoroutine.StartCoroutine(DoActivityStuff());
    }

    private void FetchUserData()
    {
        GetUserId();
        GetUserLocalId();
        GetUserContext();
        GetGamertag();
        GetUserPrivileges(); 
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

#endregion

#region Social
    private void InitializeSocial()
    {
        CreateSocialGraph();
        CreateFriendsFilter();
        UpdateFriendsList();
    }

    private int CreateSocialGraph()
    {
        int hResult = SDK.XBL.XblSocialManagerAddLocalUser(UserData.userHandle, SOCIAL_EXTRA_DETAIL_LEVEL);
        if (HR.FAILED(hResult))
            Debug.LogError($"Could not initialize Social {hResult}");

        return hResult;
    }

    private int CreateFriendsFilter()
    {
        int hResult = SDK.XBL.XblSocialManagerCreateSocialUserGroupFromFilters(UserData.userHandle, XblPresenceFilter.All, XblRelationshipFilter.Unknown, out _friendsFilter);
        if (HR.FAILED(hResult))
            Debug.LogError($"Could create friends filter {hResult}");

        return hResult;
    }

    private void UpdateFriendsList()
    {
        SDK.XBL.XblSocialManagerDoWork(out XblSocialManagerEvent[]  socialEvents);
        if (socialEvents == null || socialEvents.Length == 0)
            return;

        if (_friendsFilter == null)
        {
            Debug.LogError($"Couldn't update friends list. Friend's filter still not ready");
            return;
        }

        // TODO-Porting: Use many filters to be able to order friends
        int hResult = SDK.XBL.XblSocialManagerUserGroupGetUsers(_friendsFilter, out XblSocialManagerUser[] friends);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"Couldn't get Xbox friends {hResult}");
            return;
        }
        
        _friendsList = friends.ToList();
        _onFriendsListUpdated.Trigger(FriendsList);
        Debug.Log($"Friend count {FriendsList.Count}");
    }

    // IEnumerator DoActivityStuff()
    // {
    //     Debug.Log($"Starting stuff");
    //     yield return new WaitForSeconds(3f);
    //     Debug.Log($"Really Starting stuff");
        
    //     var info  = new XblMultiplayerActivityInfo();
    //     info.ConnectionString = "ConnectionString";
    //     info.JoinRestriction = XblMultiplayerActivityJoinRestriction.Public;
    //     info.MaxPlayers = 4;
    //     info.CurrentPlayers = 1;
    //     info.GroupId = "PartyID";
    //     info.Platform = XblMultiplayerActivityPlatform.All;

    //     SDK.XBL.XblMultiplayerActivitySetActivityAsync(UserData.contextHandle, info, true,
    //         (Int32 hresult) => 
    //         {
    //             if (HR.SUCCEEDED(hresult))
    //                 Debug.Log($"Successfully Set Activity");
    //             else
    //                 Debug.Log($"Couldn't Set activity. HR {hresult}");
    //         }
    //     );

    // }
    

    private void InviteEventHandler(IntPtr context, string inviteUri)
    {
        Debug.Log($"Invite URI: {inviteUri}");

        int inviteActionStart = inviteUri.IndexOf(INVITE_ACTION_KEY) + INVITE_ACTION_KEY.Length;
        int inviteActionEnd = inviteUri.IndexOf("?", inviteActionStart);
        string inviteAction = inviteUri[inviteActionStart..inviteActionEnd];

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
        string userInParty = inviteUri[userInPartyStart..userInPartyEnd];

        int invitedUserStart = inviteUri.IndexOf(invitedUserKey) + invitedUserKey.Length;
        int invitedUserEnd = inviteUri.IndexOf("&", invitedUserStart);
        invitedUserEnd = (invitedUserEnd == -1) ? inviteUri.Length : invitedUserEnd;
        string invitedUser = inviteUri[invitedUserStart..invitedUserEnd];

        int connectionStringStart = inviteUri.IndexOf(CONNECTION_STRING_KEY) + CONNECTION_STRING_KEY.Length;
        int connectionStringEnd = inviteUri.IndexOf("&", connectionStringStart);
        connectionStringEnd = (connectionStringEnd == -1) ? inviteUri.Length : connectionStringEnd;
        string connectionString = inviteUri[connectionStringStart..connectionStringEnd];

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

    public void SendInvite(ulong userId)
    {
        ulong[] userIds = {userId};
        SendInvite(userIds);
    }

    public void SendInvite(ulong[] userIds)
    {
        // TODO-Porting: Get right configs
        SDK.XBL.XblMultiplayerActivitySendInvitesAsync(UserData.contextHandle, userIds, true, "PartyID",
            (Int32 hr) => {
                Debug.Log($"Send Invite {(HR.SUCCEEDED(hr) ? "SUCCEEDED" : "failed")}");
            }
        );
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
}
#endif