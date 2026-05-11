#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.XGamingRuntime;
using UnityEngine;

public class GdkSocialService : IService
{
    const float FRIEND_UPDATES_CHECK_COOLDOWN = 10;
    const float BLOCKLIST_UPDATE_COOLDOWN = 30;

    private const XblSocialManagerExtraDetailLevel SOCIAL_EXTRA_DETAIL_LEVEL = XblSocialManagerExtraDetailLevel.TitleHistoryLevel;

    private GdkGameRuntimeService _gdkService;
    private GdkUserData UserData => _gdkService.UserData;

	private static readonly ConditionalEventsCollection _validator = new();
    private Coroutine _updateCoroutine;
    private XblSocialManagerUserGroupHandle _allFriendsFilter = null;

    private List<XblSocialManagerUser> _friendsList = new();
    public ReadOnlyCollection<XblSocialManagerUser> FriendsList => _friendsList.AsReadOnly();
    private EventSlot<ReadOnlyCollection<XblSocialManagerUser>> _onFriendsListUpdated = new();
    public IEventRegister<ReadOnlyCollection<XblSocialManagerUser>> OnFriendsListUpdated => _onFriendsListUpdated;
    private bool _hasFriendUpdatesPending;

    private double _lastFriendUpdateCheckTime = double.MinValue;

    public GdkSocialService(GdkGameRuntimeService gdkService)
    {
        _gdkService = gdkService;
        InitializeSocial();
        RegisterToActivityRelatedEvents();

        _updateCoroutine = ExternalCoroutine.StartCoroutine(UpdateCoroutine());
        _gdkService.OnStartedCleanUp.Register(CleanUp);
    }

    private void CleanUp()
    {
        Debug.Log($"GdkSocialService.CleanUp()");
        SDK.XBL.XblSocialManagerRemoveLocalUser(UserData.userHandle, SOCIAL_EXTRA_DETAIL_LEVEL);
        SDK.XBL.XblMultiplayerActivityDeleteActivityAsync(UserData.contextHandle, (Int32 hresult) => { });
        ExternalCoroutine.StopCoroutine(_updateCoroutine);
    }

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
        CheckForFriendUpdates();
    }

#region Friends
    private void InitializeSocial()
    {
        CreateSocialGraph();
        CreateFriendsFilter();
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
        int hResult;
        hResult = SDK.XBL.XblSocialManagerCreateSocialUserGroupFromFilters(UserData.userHandle, XblPresenceFilter.All, XblRelationshipFilter.Friends, out _allFriendsFilter);

        if (HR.FAILED(hResult))
            Debug.LogError($"Could create friends filter {hResult}");

        return hResult;
    }

    private void CheckForFriendUpdates()
    {
        SDK.XBL.XblSocialManagerDoWork(out XblSocialManagerEvent[]  socialEvents);
        _hasFriendUpdatesPending |= (socialEvents != null && socialEvents.Length > 0);

        if (!_hasFriendUpdatesPending)
            return;

        if (Time.unscaledTimeAsDouble > _lastFriendUpdateCheckTime + FRIEND_UPDATES_CHECK_COOLDOWN)
            UpdateFriendsList();
    }

    private void UpdateFriendsList()
    {   
        if (_allFriendsFilter == null)
            return;

        _hasFriendUpdatesPending = false;
        _lastFriendUpdateCheckTime = Time.unscaledTimeAsDouble;

        int hr = SDK.XBL.XblSocialManagerUserGroupGetUsers(_allFriendsFilter, out XblSocialManagerUser[] allFriends);
        if (HR.FAILED(hr))
            return;

        _friendsList = allFriends.ToList();
        _onFriendsListUpdated.Trigger(FriendsList);
    }
#endregion

#region Activity
    private void RegisterToActivityRelatedEvents()
    {
        Party.OnChangeMembers.RegisterValidated(_validator, UpdateActivity);
        PhotonDirector.connectionRoutineFinished.RegisterValidated(_validator, OnOnlineStateChanged);
    }
    
    private void OnOnlineStateChanged()
    {
        if (PhotonDirector.OfflineMode)
            SetActivityUnjoinable();
        else
            UpdateActivity();
    }

    public void SetActivityUnjoinable() => UpdateActivity(false);
    private void UpdateActivity() => UpdateActivity(true);
    private void UpdateActivity(bool joinable)
    {
        var info  = new XblMultiplayerActivityInfo();
        info.ConnectionString = Party.ID.CurrentReference;
        info.GroupId = Party.ID.CurrentReference;
        info.JoinRestriction = XblMultiplayerActivityJoinRestriction.InviteOnly;
        info.CurrentPlayers = (uint)Mathf.Max(Party.MembersCount, 1);
        info.MaxPlayers = joinable ? (uint)Constants.Networking.PARTY_MAX_MEMBERS_COUNT : info.CurrentPlayers;

        // Debug.Log($">>> Setting activity: {info.GroupId} ({info.CurrentPlayers}/{info.MaxPlayers})");

        SDK.XBL.XblMultiplayerActivitySetActivityAsync(UserData.contextHandle, info, true,
            (Int32 hresult) => 
            {
                if (HR.FAILED(hresult))
                    Debug.LogError($"Couldn't Set activity multiplayer activity. HR {hresult}");
            }
        );
    }

    public void AddRecentPlayer(ulong xuid)
    {
        XblMultiplayerActivityRecentPlayerUpdate update = new();
        update.Xuid = xuid;

        XblMultiplayerActivityRecentPlayerUpdate[] updates = { update };
        int hr = SDK.XBL.XblMultiplayerActivityUpdateRecentPlayers(UserData.contextHandle, updates);

        if (HR.FAILED(hr))
            Debug.LogError($"Failed to add recent player {update.Xuid} to recent players");
    }
#endregion
}
#endif