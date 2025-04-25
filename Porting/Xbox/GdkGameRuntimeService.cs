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

public class GdkUserData
{
    public XUserHandle userHandle;
    public XUserLocalId localId;
    public ulong userXUID;
    public string userGamertag;
    public XblContextHandle contextHandle;
    public Privileges Privileges = new();
}

#region Privileges
public class Privileges
{
    public bool readed = false;
    public readonly Privilege hasMultiplayerPrivilege = new();
    public readonly Privilege hasCrossplayPrivilege = new();
    public readonly Privilege hasUGCPrivilege = new();
    public readonly Privilege hasCommunicationsPrivilege = new();
    public readonly Privilege hasMultiplayerPartiesPrivilege = new();
    public readonly Privilege hasSessionsPrivilege = new();

    public class Privilege
    {
        public bool readed;
        public int hr;
        public XUserPrivilegeDenyReason denyReason;
        public bool hasPrivilege;
    }

    public void ReadUserPrivileges( XUserHandle userHandle )
    {
        // TODO-Porting: Check which privileges are needed and actually store them in UserData
        ReadPrivilege( userHandle, XUserPrivilege.Multiplayer, hasMultiplayerPrivilege );
        ReadPrivilege( userHandle, XUserPrivilege.CrossPlay, hasCrossplayPrivilege );
        ReadPrivilege( userHandle, XUserPrivilege.UserGeneratedContent, hasUGCPrivilege );

        // TODO-Porting: Remove, should not be needed since we are removing the chat 
        ReadPrivilege( userHandle, XUserPrivilege.Communications, hasCommunicationsPrivilege );

        // TODO-Porting: Check what those privileges actually mean
        ReadPrivilege( userHandle, XUserPrivilege.MultiplayerParties, hasMultiplayerPartiesPrivilege );
        ReadPrivilege( userHandle, XUserPrivilege.Sessions, hasSessionsPrivilege );

        readed = true;
    }

    int ReadPrivilege( XUserHandle userHandle, XUserPrivilege privilegeType, Privilege privilege )
    {
        var hr = SDK.XUserCheckPrivilege(userHandle, XUserPrivilegeOptions.None, privilegeType, out privilege.hasPrivilege, out privilege.denyReason);
        
        var failed = HR.FAILED( hr );
        privilege.readed = !failed;
        privilege.hr = hr;

        if( failed )
        {
            Debug.LogError($"Failed to check Privilege {privilege} reason {privilege.denyReason}. HR {hr} - {HR.NameOf(hr)}");
            return hr;
        }
            
        Debug.Log($"Check Privilege <color=magenta>{privilege}</color>:<color=cyan>{privilege.hasPrivilege}</color>. HR {hr} - {HR.NameOf(hr)}");
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
    private Coroutine _updateCoroutine;
    
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

    private EventSlot<string> _onReceivedInvite = new();
    public IEventRegister<string> OnReceivedInvite => _onReceivedInvite;
    public bool ShouldUpdateFriendsList; // TODO-Porting: Do as lock/unlock in case many want to listen to this
    private Coroutine _earlyInviteHandlingCoroutine;

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

    ~GdkGameRuntimeService()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        Debug.Log($"GDK CleanUp");
#if UNITY_GAMECORE
        SDK.XUserUnregisterForDeviceAssociationChanged(_deviceAssociationChangedRegistrationToken, true);
#endif
        SDK.XGameInviteUnregisterForEvent(_inviteRegistrationToken);
        SDK.XBL.XblSocialManagerRemoveLocalUser(UserData.userHandle, SOCIAL_EXTRA_DETAIL_LEVEL);
        SDK.XUserCloseHandle(UserData.userHandle);
        SDK.XBL.XblContextCloseHandle(UserData.contextHandle);
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
        int hResult = SDK.XBL.XblSocialManagerCreateSocialUserGroupFromFilters(UserData.userHandle, XblPresenceFilter.AllOnline, XblRelationshipFilter.Unknown, out _friendsFilter);
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
}
#endif