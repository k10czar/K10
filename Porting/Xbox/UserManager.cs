using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;
using UnityEngine;

internal class UserManager
{
    public enum UserOpResult
    {
        Success,
        NoDefaultUser,
        ResolveUserIssueRequired,
        UnclearedVetoes,

        UnknownError
    }

    private enum State
    {
        Initializing,
        GetContext,
        WaitForAddingUser,
        GetBasicInfo,
        InitializeNetwork,
        GrabAchievements,
        UserDisplayImage,
        ReturnMuteList,
        ReturnAvoidList,
        UserPermissionsCheck,
        WaitForNextTask,
        Error,
        Idle,
        End
    }

    public struct UserData
    {
        public XUserHandle userHandle;
        public XUserLocalId m_localId;
        public ulong userXUID;
        public string userGamertag;
        public bool userIsGuest;
        public XblPermissionCheckResult canPlayMultiplayer;
        public ulong[] avoidList;
        public ulong[] muteList;
        public byte[] imageBuffer;
        public XblContextHandle m_context;
    }

    public delegate void AddUserCompletedDelegate(UserOpResult result);
    public List<UserData> UserDataList = new List<UserData>();
    public event EventHandler<XUserChangeEvent> UsersChanged;

    private State m_State = State.Idle;
    private UserData m_CurrentUserData;
    private AddUserCompletedDelegate m_CurrentCompletionDelegate;
    private XUserChangeRegistrationToken m_CallbackRegistrationToken;

    // Constructor
    public UserManager()
    {
        // Register for the user change event with the GDK
        SDK.XUserRegisterForChangeEvent(UserChangeEventCallback, out m_CallbackRegistrationToken);
    }

    ~UserManager()
    {
        if (UserDataList != null)
        {
            foreach (UserData user in UserDataList)
            {
                if (user.m_context != null)
                {
                    SDK.XBL.XblContextCloseHandle(user.m_context);
                }
                if (user.userHandle != null)
                {
                    SDK.XUserCloseHandle(user.userHandle);
                }
            }
            UserDataList.Clear();
        }

        SDK.XUserUnregisterForChangeEvent(m_CallbackRegistrationToken);
    }

    public void Update()
    {
        switch (m_State)
        {
            case State.GetContext:
                GetUserContext();
                break;

            case State.GetBasicInfo:
                GetBasicInfo();
                break;

            case State.UserDisplayImage:
                m_State = State.WaitForNextTask;
                GetUserImage();
                break;

            case State.ReturnMuteList:
                m_State = State.WaitForNextTask;
                GetMuteList();
                break;

            case State.ReturnAvoidList:
                m_State = State.WaitForNextTask;
                GetAvoidList();
                break;

            case State.UserPermissionsCheck:
                m_State = State.WaitForNextTask;
                GetUserMultiplayerPermissions();
                break;

            case State.Error:
                m_CurrentCompletionDelegate(UserOpResult.UnknownError);
                m_CurrentCompletionDelegate = null;
                m_State = State.Idle;
                break;

            case State.End:
                UserDataList.Add(m_CurrentUserData);
                m_CurrentCompletionDelegate(UserOpResult.Success);
                m_CurrentCompletionDelegate = null;
                m_State = State.Idle;
                break;

            default:
                break;
        }
    }

    public bool AddUser( AddUserCompletedDelegate completionDelegate )
    {
        // We attempt to add the first user as the default one, the others need to be explicitely selected
        if (UserDataList.Count == 0) return AddDefaultUserSilently( completionDelegate );
        else return AddUserWithUI( completionDelegate );
    }

    //Adding User Silently
    public bool AddDefaultUserSilently(AddUserCompletedDelegate completionDelegate)
    {
        if (m_State != State.Idle)
        {
            // busy adding a user already
            return false;
        }

        m_State = State.WaitForAddingUser;
        m_CurrentUserData = new UserData();
        m_CurrentCompletionDelegate = completionDelegate;
        SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserSilently, (Int32 hresult, XUserHandle userHandle) =>
        {
            if (hresult == 0 && userHandle != null)
            {
                Debug.Log("AddUser complete " + hresult + " user handle " + userHandle.GetHashCode());

                // Call XUserGetId here to ensure all vetos (privacy consent, gametag banned, etc) have passed
                UserOpResult result = GetUserId(userHandle);
                if (result == UserOpResult.ResolveUserIssueRequired)
                {
                    ResolveSigninIssueWithUI(userHandle, m_CurrentCompletionDelegate);
                }
                else if (result != UserOpResult.Success)
                {
                    m_CurrentCompletionDelegate(result);
                }
                else
                    m_State = State.GetContext;
            }
            else if (hresult == HR.E_GAMEUSER_NO_DEFAULT_USER)
            {
                m_State = State.Idle;
                m_CurrentCompletionDelegate(UserOpResult.NoDefaultUser);
            }
            else
            {
                m_State = State.Idle;
                m_CurrentCompletionDelegate(UserOpResult.UnknownError);
            }
        });

        return true;
    }

    public bool AddUserWithUI(AddUserCompletedDelegate completionDelegate)
    {
        if (m_State != State.Idle)
        {
            // busy adding a user already
            return false;
        }

        m_State = State.WaitForAddingUser;
        m_CurrentUserData = new UserData();
        m_CurrentCompletionDelegate = completionDelegate;

        SDK.XUserAddAsync(XUserAddOptions.None, (Int32 hresult, XUserHandle userHandle) =>
        {
            if (hresult == 0 && userHandle != null)
            {
                Debug.Log("AddUserWithUI complete " + hresult + " user handle " + userHandle.GetHashCode());

                // Call XUserGetId here to ensure all vetos (privacy consent, gametag banned, etc) have passed
                UserOpResult result = GetUserId(userHandle);
                if (result == UserOpResult.ResolveUserIssueRequired)
                {
                    ResolveSigninIssueWithUI(userHandle, m_CurrentCompletionDelegate);
                }
                else if (result != UserOpResult.Success)
                {
                    m_CurrentCompletionDelegate(result);
                }
                else
                    m_State = State.GetContext;
            }
            else if (userHandle != null)
            {
                // Failed to log in, try to resolve issue
                ResolveSigninIssueWithUI(userHandle, m_CurrentCompletionDelegate);
            }
            else
            {
                Debug.Log("Got empty user handle back from AddUserWithUI.");
                m_State = State.Idle;
                m_CurrentCompletionDelegate(UserOpResult.UnknownError);
            }
        });

        return true;
    }

    //Resolve sign in issue - lauches  UI to sign in users
    private void ResolveSigninIssueWithUI(XUserHandle userHandle, AddUserCompletedDelegate completionDelegate)
    {
        SDK.XUserResolveIssueWithUiUtf16Async(userHandle, null, (Int32 resolveHResult) =>
        {
            if (resolveHResult == 0)
            {
                GetUserId(userHandle);
                m_State = State.GetContext;
            }
            else
            {
                // User has uncleared vetoes.  The game should decide how to handle this,
                // either by gracefully continuing or dropping user back to title screen to
                // with "Press 'A' or 'Enter' to continue" to select a new user.
                completionDelegate(UserOpResult.UnclearedVetoes);
                m_State = State.Idle;
            }
        });
    }

    // Get User ID
    private UserOpResult GetUserId(XUserHandle userHandle)
    {
        ulong xuid;
        int hr = SDK.XUserGetId(userHandle, out xuid);
        if (hr == 0)
        {
            m_CurrentUserData.userHandle = userHandle;
            m_CurrentUserData.userXUID = xuid;
            return UserOpResult.Success;
        }
        else if (hr == HR.E_GAMEUSER_RESOLVE_USER_ISSUE_REQUIRED)
        {
            return UserOpResult.ResolveUserIssueRequired;
        }

        return UserOpResult.UnknownError;
    }

    // Retrive gamer tag for user and guest status via User Handle
    private void GetBasicInfo()
    {
        Debug.Log("Grabbing Gamertag and checking if user is a guest");
        int hr = SDK.XUserGetGamertag(m_CurrentUserData.userHandle, XUserGamertagComponent.Classic,
            out m_CurrentUserData.userGamertag);
        if (hr == 0)
        {
            hr = SDK.XUserGetIsGuest(m_CurrentUserData.userHandle, out m_CurrentUserData.userIsGuest);
        }

        if (hr == 0)
        {
            hr = SDK.XUserGetLocalId(m_CurrentUserData.userHandle, out m_CurrentUserData.m_localId);

            // check this isn't a user we already have
            foreach (UserData userData in UserDataList)
                if (userData.m_localId.Value == m_CurrentUserData.m_localId.Value)
                {
                    // we already have this user
                    m_State = State.Error;
                    return;
                }
        }
        else
        {
            Debug.Log("Failed to grab gamertag, hresult: " + hr);
            m_State = State.Error;
            return;
        }

        if (hr == 0)
        {
            m_State = State.UserDisplayImage;
        }
        else
        {
            Debug.Log("Failed to grab gamertag and guest status, hresult: " + hr);
            m_State = State.Error;
        }
    }

    // User Image
    private void GetUserImage()
    {
        SDK.XUserGetGamerPictureAsync(m_CurrentUserData.userHandle, XUserGamerPictureSize.Large,
            (Int32 hresult, Byte[] buffer) =>
            {
                if (hresult == 0)
                {
                    m_CurrentUserData.imageBuffer = buffer;
                    m_State = State.ReturnMuteList;
                }
                else
                {
                    Debug.Log("Failed to grab image, hresult: " + hresult);
                    m_State = State.Error;
                }
            });
    }

    // User Mute List
    private void GetMuteList()
    {
        SDK.XBL.XblPrivacyGetMuteListAsync(m_CurrentUserData.m_context, (Int32 hresult, UInt64[] xuids) =>
        {
            if (hresult == 0)
            {
                m_CurrentUserData.muteList = xuids;
                Debug.Log("Retrieved " + xuids.Length + " muted users");
                m_State = State.ReturnAvoidList;
            }
            else
            {
                Debug.Log("Failed to retrive users mute list.");
                m_State = State.Error;
            }
        });
    }

    // User Avoid List
    private void GetAvoidList()
    {
        SDK.XBL.XblPrivacyGetAvoidListAsync(m_CurrentUserData.m_context, (Int32 hresult, UInt64[] xuids) =>
        {
            if (hresult == 0)
            {
                m_CurrentUserData.avoidList = xuids;
                Debug.Log("Retrieved " + xuids.Length + "avoided users");
                m_State = State.UserPermissionsCheck;
            }
            else
            {
                Debug.Log("Failed to retrive users avoid list");
                m_State = State.Error;
            }
        });
    }

    //Example of User PlayerMultiplayer Permssions retrieval
    private void GetUserMultiplayerPermissions()
    {
        SDK.XBL.XblPrivacyCheckPermissionAsync(m_CurrentUserData.m_context, XblPermission.PlayMultiplayer,
            m_CurrentUserData.userXUID, (Int32 hresult, XblPermissionCheckResult result) =>
            {
                if (hresult == 0)
                {
                    m_CurrentUserData.canPlayMultiplayer = result;
                    m_State = State.End;
                }
                else
                {
                    Debug.Log("Failed to get user multiplayer permission");
                    m_State = State.Error;
                }
            });
    }

    // Get the user's live context
    private void GetUserContext()
    {
        int hr = SDK.XBL.XblContextCreateHandle(m_CurrentUserData.userHandle, out m_CurrentUserData.m_context);
        if (hr == 0 && m_CurrentUserData.m_context != null)
        {
            Debug.Log("Success XBL and Context");
            m_State = State.GetBasicInfo;
        }
        else
        {
            Debug.Log("Error creating context");
            m_State = State.Error;
        }
    }

    private void UserChangeEventCallback(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
    {
        if (eventType == XUserChangeEvent.SignedOut)
        {
            Debug.LogWarning("User logging out");

            foreach (UserData userData in UserDataList)
                if (userData.m_localId.Value == userLocalId.Value)
                {
                    // we already have this user
                    UserDataList.Remove(userData);
                    break;
                }
        }

        if (eventType != XUserChangeEvent.SignedInAgain)
        {
            EventHandler<XUserChangeEvent> handler = UsersChanged;
            handler?.Invoke(this, eventType);
        }
    }
}
