#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using System.Collections.Generic;
using Unity.XGamingRuntime;
using UnityEngine;
using XBL = Unity.XGamingRuntime.SDK.XBL;

public static class GdkAchievementsAdapter
{
    private const int Progress100 = 100;

    public static readonly Dictionary<string, string> Map = new()
    {
        { "ACH_OUR_LEGACY", "15" },
        { "ACH_OLD_COMPANIONS", "2" },
        { "ACH_KILLED_MERCHANT", "1" },
        { "ACH_KILLED_MATRIARCH", "4" },
        { "ACH_KILLED_HEADCUTTER", "5" },
        { "ACH_KILLED_TAMARINDO", "6" },
        { "ACH_BACK_HOME", "7" },
        { "ACH_KILLED_VULTURE", "8" },
        { "ACH_DIVINE_HELP", "9" },
        { "ACH_KILLED_MINISTER", "10" },
        { "ACH_FINISH_SUBWAY_SURVIVAL", "11" },
        { "ACH_ENTER_THE_TRAIN", "12" },
        { "ACH_KILLED_MACHINIST", "13" },
        { "ACH_KILLED_DRNINA", "14" },
        { "ACH_FINAL_COUNSELOR", "15" },
        { "ACH_RED_PORTAL", "17" },
        { "ACH_TALK_TO_GHOSTS", "18" },
        { "ACH_FAITH_CONSTELLATION", "19" },
        { "ACH_DISCIPLINE_CONSTELLATION", "20" },
        { "ACH_FURY_CONSTELLATION", "21" },
        { "ACH_KILLED_LOOTGOBLIN", "22" },
        { "ACH_KILLED_MERCHANT_FIRST_RUN", "23" },
        { "ACH_KILLED_HEADCUTTER_LESS_THAN_SEVEN_RUNS", "27" },
        { "ACH_KILLED_MINISTER_LESS_THAN_FOURTEEN_RUNS", "28" },
        { "ACH_KILLED_NINA_LESS_THAN_TWENTYONE_RUNS", "29" },
        { "ACH_ALL_BLESSINGS", "24" },
        { "ACH_BLESSING_FOUNDATION", "25" },
        { "ACH_BLESSING_OF_THE_FUTURE", "26" },
        { "ACH_DEAL_30000_DAMAGE_SINGLEHIT", "30" },
        { "ACH_DEAL_100000_DAMAGE_SINGLEHIT", "31" },
        { "ACH_DEAL_7777777_DAMAGE_SINGLEHIT", "32" },
        { "ACH_DEAL_77777777_DAMAGE_SINGLEHIT", "33" },
        { "ACH_DEAL_777777777_DAMAGE_SINGLEHIT", "34" },
    };

    private static bool TryGetGdk(out GdkGameRuntimeService gdk)
    {
        gdk = ServiceLocator.Get<GdkGameRuntimeService>();
        return gdk != null;
    }

    private static bool IsReady(GdkGameRuntimeService gdk)
    {
        if (gdk == null) return false;
        if (!gdk.IsInitialized.Value) return false;

        // user/context must exist for achievements
        var u = gdk.UserData;
        if (u == null) return false;
        if (u.userHandle == null) return false;
        if (u.contextHandle == null) return false;
        if (u.userXUID == 0) return false;

        return true;
    }

    private static string ResolveAchievementId(string gameCodeOrXboxId)
        => Map.TryGetValue(gameCodeOrXboxId, out var xboxId) ? xboxId : gameCodeOrXboxId;

    // "Unlock" by setting 100%
    public static void Unlock(string gameCodeOrXboxId)
    {
        if (!TryGetGdk(out var gdk) || !IsReady(gdk))
        {
            Debug.LogWarning("[GDK][ACH] Not ready (no user/context yet).");
            return;
        }

        var achievementId = ResolveAchievementId(gameCodeOrXboxId);

        Debug.Log($"[GDK][ACH] Unlock '{gameCodeOrXboxId}' -> id '{achievementId}'");

        XBL.XblAchievementsUpdateAchievementAsync(
            gdk.UserData.contextHandle,
            gdk.UserData.userXUID,
            achievementId,
            Progress100,
            hr =>
            {
                if (HR.FAILED(hr))
                {
                    // -2145844944 is the "not modified" you had before
                    Debug.LogWarning($"[GDK][ACH] Update failed HR=0x{hr:X8} ({HR.NameOf(hr)}) id='{achievementId}'");
                    return;
                }

                Debug.Log($"[GDK][ACH] Unlock success id='{achievementId}'");
            }
        );
    }

    public static void ShowUI()
    {
        if (!TryGetGdk(out var gdk) || gdk.UserData?.userHandle == null)
        {
            Debug.LogWarning("[GDK][ACH] Not ready to show achievements UI (no user handle).");
            return;
        }

        var titleId = gdk.TitleIdNumeric;

        Debug.Log($"[GDK][ACH] Show UI titleId=0x{titleId:X8}");

        SDK.XGameUiShowAchievementsAsync(gdk.UserData.userHandle, titleId, hr =>
        {
            if (HR.FAILED(hr))
                Debug.LogWarning($"[GDK][ACH] Show UI failed HR=0x{hr:X8} ({HR.NameOf(hr)})");
            else
                Debug.Log("[GDK][ACH] Show UI success");
        });
    }

    public static void GetAchievement(string gameCodeOrXboxId)
    {
        if (!TryGetGdk(out var gdk) || !IsReady(gdk))
        {
            Debug.LogWarning("[GDK][ACH] Not ready (no user/context yet).");
            return;
        }

        var achievementId = ResolveAchievementId(gameCodeOrXboxId);

        XBL.XblAchievementsGetAchievementAsync(
            gdk.UserData.contextHandle,
            gdk.UserData.userXUID,
            gdk.Scid,                
            achievementId,
            (hr, result) =>
            {
                if (HR.FAILED(hr))
                {
                    Debug.LogError($"[GDK][ACH] GetAchievement failed HR=0x{hr:X8} ({HR.NameOf(hr)})");
                    return;
                }

                hr = XBL.XblAchievementsResultGetAchievements(result, out var achievements);
                if (HR.FAILED(hr))
                {
                    Debug.LogError($"[GDK][ACH] ResultGetAchievements failed HR=0x{hr:X8} ({HR.NameOf(hr)})");
                    return;
                }

                foreach (var a in achievements)
                    Debug.Log($"[GDK][ACH] {a.Id} '{a.Name}' state={a.ProgressState}");
            }
        );
    }
}
#endif