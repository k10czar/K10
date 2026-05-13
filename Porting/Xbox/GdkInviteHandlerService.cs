#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
using System;
using Unity.XGamingRuntime;
using UnityEngine;

public class GdkInviteHandlerService : IService
{
    private const string INVITE_ACTION_KEY = "://";
    private const string SENDER_KEY = "sender=";
    private const string INVITED_USER_KEY = "invitedUser=";
    private const string CONNECTION_STRING_KEY = "connectionString=";
    private const string JOINER_KEY = "joinerXuid=";
    private const string JOINEE_KEY = "joineeXuid=";
    private const string ACCEPT_INVITE_KEY = "inviteAccept";
    private const string JOIN_ACTIVITY_KEY = "activityJoin";

    private GdkGameRuntimeService _gdkService;
    private GdkUserData UserData => _gdkService.UserData;

	private static readonly ConditionalEventsCollection _validator = new();

    
    public GdkInviteHandlerService(GdkGameRuntimeService gdkService)
    {
        _gdkService = gdkService;
        _gdkService.OnReceivedInvite.RegisterValidated(_validator, HandleUriInvite_ZOMBIE);
    }

    public void SendInvite_ZOMBIE(ulong userId, string partyId)
    {
        // TODO: Circular dependency
        // CommunicationPermissionCache.CallIfPermitted(userId, () => ReallySendInvite(new ulong[] { userId }, partyId));
    }

    private void ReallySendInvite(ulong[] userIds, string partyId)
    {
        SDK.XBL.XblMultiplayerActivitySendInvitesAsync(UserData.contextHandle, userIds, true, partyId,
            (Int32 hr) => {
                Debug.Log($"Send Invite {(HR.SUCCEEDED(hr) ? "SUCCEEDED" : "failed")}");
            }
        );
    }

    private void HandleUriInvite_ZOMBIE(string inviteUri)
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

        HandleInvite_ZOMBIE(UInt64.Parse(userInParty), UInt64.Parse(invitedUser), connectionString);
    }

    private void HandleInvite_ZOMBIE(ulong userWhoInvited, ulong invitedUser, string partyId)
    {
        if (invitedUser != UserData.userXUID)
            return;

        // TODO: Move to a common invite handler to avoid circular dependency. Maybe just invoke an event and let the common invite handler hanlde it
        // BlockList.CallIfNotBlocked( userWhoInvited, () =>
        // {
        //     // CommunicationPermissionCache.CallIfPermitted( userWhoInvited,
	    //         // () => {
        //             _gdkService.AskForPrivilege(_gdkService.UserData.Privileges.Multiplayer, () => {
        //                 // TODO: Actually accept event
	    //                 // var inviteHandler = ServiceLocator.Get<InviteHandlerService>();
	    //                 // inviteHandler.AcceptInvite(partyId);
	    //             }, null);
	    //         // }
	    //     // );
        // } );
    }
}
#endif