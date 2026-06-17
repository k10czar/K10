#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using Unity.XGamingRuntime;
using UnityEngine;
using XBL = Unity.XGamingRuntime.SDK.XBL;

public static class GdkAchievementsAdapter
{
    private const int Progress100 = 100;
    private const string DefaultMapResourceName = "GdkAchievementMap";

    private static GdkAchievementMapAsset _mapAsset;
    private static bool _missingMapWarningShown;

    public static void SetMap(GdkAchievementMapAsset map)
    {
        _mapAsset = map;
        _missingMapWarningShown = false;
    }

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

    private static GdkAchievementMapAsset GetMap()
    {
        if (_mapAsset != null)
            return _mapAsset;

        // Optional fallback: place the asset at Assets/Resources/GdkAchievementMap.asset
        _mapAsset = Resources.Load<GdkAchievementMapAsset>(DefaultMapResourceName);

        if (_mapAsset == null && !_missingMapWarningShown)
        {
            _missingMapWarningShown = true;
            Debug.LogWarning(
                "[GDK][ACH] GdkAchievementMapAsset is not assigned. " +
                "Call GdkAchievementsAdapter.SetMap(map) during platform initialization, " +
                "or place GdkAchievementMap.asset inside a Resources folder. " +
                "Raw ids will still work."
            );
        }

        return _mapAsset;
    }

    private static string ResolveAchievementId(string gameCodeOrXboxId)
    {
        var map = GetMap();

        if (map != null && map.TryResolve(gameCodeOrXboxId, out var xboxId))
            return xboxId;

        return gameCodeOrXboxId;
    }

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
