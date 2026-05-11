// #define LOG
using System.Collections;
using UnityEngine;
using K10.Promises;
using System.Collections.Generic;
using System.Linq;
using System;



#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
using Unity.XGamingRuntime;
#endif

public static class BlockList
{
    const float CALL_COOLDOWN = 3;

    static Dictionary<ulong, Future<bool>> _pendingValidations = new();
    static Future<List<ulong>> _listFuture = null;

    static Coroutine _coroutine;
    static List<ulong> _blockedUsers = null;

    public static void CallIfNotBlocked(ulong uid, Action actionToCall)
    {
        if (TryGetBlockedSync(uid, out var isBlocked))
        {
            if (!isBlocked) actionToCall();
        }
        else
        {
            GetBlockAsync(uid).Register((blockedAsyncResult) =>
            {
                if (!blockedAsyncResult) actionToCall();
            });
        }
    }

    public static void CallIfNotBlocked(string id, Action actionToCall)
    {
        if (!ulong.TryParse(id, out var uid)) actionToCall();
        else if (TryGetBlockedSync(uid, out var isBlocked))
        {
            if (!isBlocked) actionToCall();
        }
        else
        {
            GetBlockAsync(uid).Register((blockedAsyncResult) =>
            {
                if (!blockedAsyncResult) actionToCall();
            });
        }
    }

    public static void QueryBlockedList(Action<List<ulong>> actionToCall)
    {
#if !UNITY_GAMECORE && !MICROSOFT_GDK_SUPPORT
        actionToCall(_blockedUsers);
        return;
#else
        if (_blockedUsers != null) actionToCall( _blockedUsers );
        else GetBlockListAsync().Register(actionToCall);
#endif
    }

    public static void QueryBlocked(string id, Action<bool> actionToCall)
    {
        if (!ulong.TryParse(id, out var uid)) actionToCall( false );
        else QueryBlocked(uid, actionToCall );
    }

    public static void QueryBlocked(ulong uid, Action<bool> actionToCall)
    {
        if (TryGetBlockedSync(uid, out var isBlocked)) actionToCall(isBlocked);
        else GetBlockAsync(uid).Register(actionToCall);
    }

    public static bool TryGetBlockedSync(string id, out bool blocked)
    {
        blocked = false;
        //Not a valid xbox uid, so cannot be blocked? Assuming that since we can only query ulongs
        if (!ulong.TryParse(id, out var uid)) return true;
        return TryGetBlockedSync(uid, out blocked);
    }

    public static bool TryGetBlockedSync(ulong uid, out bool blocked)
    {
        blocked = false;
#if !UNITY_GAMECORE && !MICROSOFT_GDK_SUPPORT
        //Outside xbox ecosystem we do not check for blocked users, everyone is unblocked
        return true;
#endif

        //Cannot find the cache so could not query synchronously
        if (_blockedUsers == null) return false;

        //Found the cache so could query synchronously
        blocked = _blockedUsers.Contains(uid);
        return true;
    }

    public static Future<bool> GetBlockAsync(string id)
    {
        if (!ulong.TryParse(id, out var uid))
        {
            //TryGetBlocked does not let call GetBlockAsync with no ulong uid
            //But if fall here, we will call future resulting false after a frame
            var fakeFut = new Future<bool>();
            ExternalCoroutine.StartCoroutine(FakeCoroutine(fakeFut));
            return fakeFut;
        }
        
        return GetBlockAsync(uid);
    }

    public static Future<bool> GetBlockAsync(ulong uid)
    {
        if (_pendingValidations.TryGetValue(uid, out var result)) return result;
        var fut = new Future<bool>();
        _pendingValidations[uid] = fut;
        if (_coroutine == null) _coroutine = ExternalCoroutine.StartCoroutine(Coroutine());
        return fut;
    }

    public static Future<List<ulong>> GetBlockListAsync()
    {
        if (_listFuture != null) return _listFuture;
        _listFuture = new Future<List<ulong>>();
        if (_coroutine == null) _coroutine = ExternalCoroutine.StartCoroutine(Coroutine());
        return _listFuture;
    }

    private static IEnumerator FakeCoroutine(Future<bool> futureToCall, bool result = false)
    {
        yield return null; // wait at least one frame to complete future
        futureToCall.ForceComplete( result );
    }

    private static IEnumerator Coroutine()
    {
#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
        var gdkService = ServiceLocator.Get<IGdkRuntimeService>() as GdkGameRuntimeService;
        var cooldown = new WaitForSecondsRealtime(CALL_COOLDOWN);
#endif
        while (_pendingValidations.Count > 0 || _listFuture != null)
        {
            yield return null; // wait at least one frame to complete futures

            if (_pendingValidations.Count == 0 && _listFuture == null) break;

            var callTime = Time.unscaledTime;

#if LOG
            Debug.Log($"*<color=cyan>BBBLLL</color> Will call block list @ {callTime}s");
#endif

#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
            SDK.XBL.XblPrivacyGetAvoidListAsync(gdkService.UserData.contextHandle,
                (int hresult, ulong[] blockedUsers) =>
                {
                    var returnTime = Time.unscaledTime;
                    var took = returnTime - callTime;

                    var succeeded = HR.SUCCEEDED(hresult);

#if LOG
                    Debug.Log($"*<color=cyan>BBBLLL</color> Block list {(succeeded ? "SUCCESS" : "FAIL")} took {took}s");
#endif

                    if (succeeded)
                    {
                        _blockedUsers = blockedUsers.ToList();
                        _listFuture?.ForceComplete(_blockedUsers);
                        _listFuture = null;
                    }
                    else
                    {
                        _listFuture?.ForceComplete(new());
                        _listFuture = null;
                    }

                    foreach (var pending in _pendingValidations)
                    {
                        var blocked = succeeded && _blockedUsers.Contains(pending.Key);
                        pending.Value.ForceComplete(blocked);
                    }

                    _pendingValidations.Clear();
                }
            );
            yield return cooldown;
            _blockedUsers = null;
#else
            //Outside xbox ecosystem we do not check for blocked users, everyone is unblocked
            foreach (var pending in _pendingValidations) pending.Value.ForceComplete(false);
            _pendingValidations.Clear();
#endif
        }

        _coroutine = null;
        _blockedUsers = null; // Void block list after cooldown
    }
}
