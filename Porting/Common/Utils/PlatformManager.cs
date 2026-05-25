using UnityEngine;

// #if UNITY_STANDALONE && !MICROSOFT_GDK_SUPPORT
// using Steamworks;
// #elif UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
// using Unity.XGamingRuntime;
// #endif

namespace K10.Platforms
{    
    public static class PlatformConstants
    {
        public const int PLATFORMS_COUNT = 9;
    }

    [System.Flags]
    public enum EPlatform
    {
        Steam       = 1 << 0,
        Switch      = 1 << 1,
        PS4         = 1 << 2,
        PS5         = 1 << 3,
        XboxOne     = 1 << 4,
        XboxSeries  = 1 << 5,
        XboxPC      = 1 << 6,
        Android     = 1 << 7,
        IOS         = 1 << 8,
        COUNT
    }

    [System.Flags]
    public enum EPlatformFamily
    {
        PC          = 1 << 0,
        Switch      = 1 << 1,
        PlayStation = 1 << 2,
        Xbox        = 1 << 3,
        Mobile      = 1 << 4,
        COUNT
    }

    [System.Flags]
    public enum EPlatformType
    {
        PC          = 1 << 0,
        Console     = 1 << 1,
        Mobile      = 1 << 2,
        COUNT
    }

    public static class PlatformManager
    {
#if UNITY_EDITOR
        public static EPlatform DebugPlatform = EPlatform.COUNT;
        public static EPlatformType DebugPlatformType = EPlatformType.COUNT;
        public static EPlatformFamily DebugPlatformFamily = EPlatformFamily.COUNT;
#endif

        public static bool IsOnMobile => GetPlatformType() == EPlatformType.Mobile;
        public static bool IsOnConsole => GetPlatformType() == EPlatformType.Console;
        public static bool IsOnPC => GetPlatformType() == EPlatformType.PC;
        public static bool IsOnXboxEnvironment => GetPlatformFamily() == EPlatformFamily.Xbox;
        public static bool IsOnPlayStation => GetPlatformFamily() == EPlatformFamily.PlayStation;

        public static EPlatform GetPlatform()
        {
#if UNITY_EDITOR
            if (DebugPlatform != EPlatform.COUNT)
                return DebugPlatform;
#endif

#if UNITY_SWITCH
            return EPlatform.Switch;
#elif UNITY_PS4
            return EPlatform.PS4;
#elif UNITY_PS5
            return EPlatform.PS5;
#elif UNITY_GAMECORE_XBOXONE
            return EPlatform.XboxOne;
#elif UNITY_GAMECORE_XBOXSERIES
            return EPlatform.XboxSeries;
#elif UNITY_ANDROID
            return EPlatform.Android;
#elif UNITY_IOS 
            return EPlatform.IOS;
#elif MICROSOFT_GDK_SUPPORT
            return EPlatform.XboxPC;
#else
            return EPlatform.Steam;
#endif
        }

        public static EPlatform GetPlatformFamilyMask()
        {
#if UNITY_SWITCH
            return SwitchPlatformFilter;
#elif UNITY_PS4 || UNITY_PS5
            return PlaystationPlatformFilter;
#elif UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
            return XboxPlatformFilter;
#elif UNITY_ANDROID || UNITY_IOS
            return MobilePlatformFilter;
#else
            return SteamPlatformFilter;
#endif
        }

        public static EPlatform XboxPlatformFilter => EPlatform.XboxPC | EPlatform.XboxOne | EPlatform.XboxSeries;
        public static EPlatform SteamPlatformFilter => EPlatform.Steam;
        public static EPlatform PlaystationPlatformFilter => EPlatform.PS4 | EPlatform.PS5;
        public static EPlatform SwitchPlatformFilter => EPlatform.Switch;
        public static EPlatform MobilePlatformFilter => EPlatform.Android | EPlatform.IOS;
        
        public static EPlatformFamily GetPlatformFamily()
        {
#if UNITY_EDITOR
            if (DebugPlatformFamily != EPlatformFamily.COUNT)
                return DebugPlatformFamily;
#endif

#if UNITY_SWITCH
            return EPlatformFamily.Switch;
#elif UNITY_PS4 || UNITY_PS5
            return EPlatformFamily.PlayStation;
#elif UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
            return EPlatformFamily.Xbox;
#elif UNITY_ANDROID || UNITY_IOS
            return EPlatformFamily.Mobile;
#else
            return EPlatformFamily.PC;
#endif
        }

        public static EPlatformType GetPlatformType()
        {
#if UNITY_EDITOR
            if (DebugPlatformType != EPlatformType.COUNT)
                return DebugPlatformType;
#endif

        
#if UNITY_PS5 || UNITY_PS4 || UNITY_XBOXONE || UNITY_GAMECORE || UNITY_SWITCH
            return EPlatformType.Console;
#elif UNITY_ANDROID || UNITY_IOS
            return EPlatformType.Mobile;
#else
            return EPlatformType.PC;
#endif
        }

        // TODO: Reenable this when Steam Deck check is accessible on K10 library
//         public static bool IsOnSteamDeck()
//         {
// #if UNITY_STANDALONE && !MICROSOFT_GDK_SUPPORT
//             if (!SteamManager.Initialized)
//             {
//                 Debug.LogError("<color=cyan>Steam Manager not Initalized, can't tell if running on Steam Deck or not.</color>");
//                 return false;
//             }

//             return SteamUtils.IsSteamRunningOnSteamDeck();
// #else
//             return false;
// #endif
//         }
        
        // TODO: Reenable nickname prefixes
        // const string NicknamePrefix_Playstation = "psn_";
        // const string NicknamePrefix_Xbox = "xbo_";
        // const string NicknamePrefix_Steam = "ste_";
        // const string NicknamePrefix_Switch = "swi_";
        // TODO: Android and iOS prefixes

        // public static bool? EnforceDisplayNameStringValidation;

        // public static string GetPlatformPrefixFromNickname(string nickname)
        // {
        //     if (string.IsNullOrEmpty(nickname))
        //     {
        //         Debug.LogError($"Tried to get Platform Prefix of a null or empty string {nickname.ToStringOrNull()}. Returned empty.");
        //         return string.Empty;
        //     }
        //     var platformPrefix = nickname.Substring(0, UnityEngine.Mathf.Min(nickname.Length, 4));
        //     return platformPrefix;
        // }

        // public static EPlatform GetPlatformFromNickname(string nickname)
        // {
        //     if (string.IsNullOrEmpty(nickname)) return EPlatform.COUNT;
        //     var platformPrefix = nickname.Substring(0, UnityEngine.Mathf.Min(nickname.Length, 4));
        //     if (platformPrefix.Equals(NicknamePrefix_Xbox)) return EPlatform.XboxSeries;
        //     else if (platformPrefix.Equals(NicknamePrefix_Steam)) return EPlatform.Steam;
        //     else if (platformPrefix.Equals(NicknamePrefix_Switch)) return EPlatform.Switch;
        //     else if (platformPrefix.Equals(NicknamePrefix_Playstation)) return EPlatform.PS5;
        //     return EPlatform.COUNT;
        // }

        // public static string GetPlatformPrefixFromFamily(EPlatformFamily family)
        // {
        //     switch (family)
        //     {
        //         case EPlatformFamily.Xbox: return NicknamePrefix_Xbox;
        //         case EPlatformFamily.PlayStation: return NicknamePrefix_Playstation;
        //         case EPlatformFamily.Switch: return NicknamePrefix_Switch;
        //         case EPlatformFamily.PC: return NicknamePrefix_Steam;

        //         default:
        //             Debug.LogError($"Trying to get Platform Prefix from invalid family {family}");
        //             return string.Empty;
        //     }
        // }

        // public static EPlatformFamily GetPlatformFamilyFromNickname(string nickname)
        // {
        //     if (string.IsNullOrEmpty(nickname)) return EPlatformFamily.COUNT;
        //     var platformPrefix = nickname.Substring(0, UnityEngine.Mathf.Min(nickname.Length, 4));
        //     if (platformPrefix.Equals(NicknamePrefix_Xbox)) return EPlatformFamily.Xbox;
        //     else if (platformPrefix.Equals(NicknamePrefix_Steam)) return EPlatformFamily.PC;
        //     else if (platformPrefix.Equals(NicknamePrefix_Switch)) return EPlatformFamily.Switch;
        //     else if (platformPrefix.Equals(NicknamePrefix_Playstation)) return EPlatformFamily.PlayStation;
        //     return EPlatformFamily.COUNT;
        // }

        // public static string GetPlatformPrefix() => GetPlatformPrefix( GetPlatform() );

        // public static string GetPlatformPrefix( EPlatform platform )
        // {
        //     switch (platform)
        //     {
        //         case EPlatform.PS5:
        //         case EPlatform.PS4:
        //             return NicknamePrefix_Playstation;

        //         case EPlatform.XboxPC:
        //         case EPlatform.XboxOne:
        //         case EPlatform.XboxSeries:
        //             return NicknamePrefix_Xbox;

        //         case EPlatform.Switch:
        //             return NicknamePrefix_Switch;

        //         case EPlatform.Steam:
        //             return NicknamePrefix_Steam;

        //         default:
        //             Debug.LogError($"Trying to get Platform Prefix from invalid platform {platform}");
        //             return string.Empty;
        //     }
        // }

        // public static int CountFamiliesIn( EPlatform filter )
        // {
        //     var steam = ( filter & SteamPlatformFilter ) != 0;
        //     var xbox = ( filter &  XboxPlatformFilter ) != 0;
        //     var ps = ( filter &  PlaystationPlatformFilter ) != 0;
        //     var nintendo = ( filter &  SwitchPlatformFilter ) != 0;
            
        //     var families = 0;
        //     if( steam ) families++;
        //     if( xbox ) families++;
        //     if( ps ) families++;
        //     if( nintendo ) families++;

        //     return families;
        // }

//         public static string GetPlatformAccountName(string fallback, bool includePlatformPrefix = true)
//         {
//             string platformName = includePlatformPrefix ? GetPlatformPrefix() : string.Empty;
// #if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
//             var gdkService = ServiceLocator.Get<IGdkRuntimeService>() as GdkGameRuntimeService;
//             if (!gdkService.IsFullyInitialized.Value)
//             {
//                 Debug.LogError($"Trying to get xbox gamertag before logging in");
//                 platformName += "<GAMERTAG>";
//             }
//             else
//                 platformName += gdkService.UserData.userGamertag;

// #elif !MICROSOFT_GDK_SUPPORT
//             // if (!Steamworks.SteamManager.Initialized) platformName += fallback;
//             // else platformName += Steamworks.SteamFriends.GetPersonaName();
// #endif
//             return platformName;
//         }
    }
}
