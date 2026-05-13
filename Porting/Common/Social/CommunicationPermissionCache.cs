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

public static class CommunicationPermissionCache
{
    public const int CROSS_NETWORK_FAKE_XUID = 0;
    const float CALL_COOLDOWN = 3;

    static Dictionary<ulong, Future<bool>> _pendingValidations = new();

    static List<Future<Dictionary<ulong, bool>>> _pendingBatchFutures = new();

    static bool _checkedCrossPermission = false;
    static bool _canCrossPermission = false;
    static Future<bool> _crossFuture = null;

    static bool _checkedCommunicationsPrivilege = false;
    static bool _communicationsPrivilege = false;

    static Coroutine _coroutine;
    static Dictionary<ulong, bool> _communicationPermissionCache = new();

    public static void CallIfPermitted(ulong uid, Action actionToCall)
    {
        if (TryGetPermissionSync(uid, out var permitted))
        {
            if (permitted) actionToCall();
        }
        else
        {
            GetBlockAsync(uid).Register((permitted) => { if (permitted) actionToCall(); } );
        }
    }

    public static void CallIfPermittedCross(Action actionToCall)
    {
        if (!HasCommunicatePrivilege()) return;
        if (_checkedCrossPermission)
        {
            if (_canCrossPermission) actionToCall();
        }
        else
        {
            if (_crossFuture == null)
            {
                _crossFuture = new Future<bool>();
                _crossFuture.Register((permitted) => { if (permitted) actionToCall(); });
                if (_coroutine == null) _coroutine = ExternalCoroutine.StartCoroutine(Coroutine());
            }
        }
    }

    public static void BatchCall(IEnumerable<ulong> uids, Action<Dictionary<ulong, bool>> actionToCall)
    {
        var allCached = true;
        foreach (var uid in uids)
        {
            var alreadyCached = TryGetPermissionSync(uid, out var permitted);
            if (!alreadyCached) GetBlockAsync( uid );
            allCached &= alreadyCached;
        }

        if (allCached)
        {
            actionToCall(_communicationPermissionCache);
            return;
        }

        var bFut = new Future<Dictionary<ulong, bool>>();
        bFut.Register( actionToCall );
        _pendingBatchFutures.Add( bFut );
    }

    public static bool HasCommunicatePrivilege()
    {
        if (!_checkedCommunicationsPrivilege)
        {
#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
            var gdkService = ServiceLocator.Get<IGdkRuntimeService>() as GdkGameRuntimeService;
            _communicationsPrivilege = gdkService.UserData.Privileges.Communications.isEnabled;
#else
            _communicationsPrivilege = true;
#endif
            _checkedCommunicationsPrivilege = true;
        }
        return _communicationsPrivilege;
    }

    public static bool TryGetPermissionSync(ulong uid, out bool permitted)
    {
        if (_communicationPermissionCache.TryGetValue(uid, out permitted)) return true;

        permitted = true;
#if !UNITY_GAMECORE && !MICROSOFT_GDK_SUPPORT
        _communicationPermissionCache[uid] = true;
        //Outside xbox ecosystem we do not check for blocked users, everyone is unblocked
        return true;
#endif

        permitted = false;
        if (!HasCommunicatePrivilege())
        {
            _communicationPermissionCache[uid] = false;
            return true;
        }

        //Try from find on cache so could query synchronously
        return false;
    }

    public static Future<bool> GetBlockAsync(ulong uid)
    {
        if (_pendingValidations.TryGetValue(uid, out var result)) return result;
        var fut = new Future<bool>();
        _pendingValidations[uid] = fut;
        if (_coroutine == null) _coroutine = ExternalCoroutine.StartCoroutine(Coroutine());
        return fut;
    }
    
#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
    static XblPermission[] permissions = new XblPermission[] { XblPermission.CommunicateUsingText };
    static XblAnonymousUserType[] anonymousUserTypes = new XblAnonymousUserType[] { XblAnonymousUserType.CrossNetworkUser };
    static XblAnonymousUserType[] noAnonymousUserTypes = new XblAnonymousUserType[] { };
#endif

    private static IEnumerator Coroutine()
    {
#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
        var gdkService = ServiceLocator.Get<IGdkRuntimeService>() as GdkGameRuntimeService;
        var cooldown = new WaitForSecondsRealtime(CALL_COOLDOWN);
#endif 
        while (_pendingValidations.Count > 0 || _crossFuture != null)
        {
            yield return null; // wait at least one frame to complete futures

            if (_pendingValidations.Count == 0 && _crossFuture == null) break;

            var callTime = Time.unscaledTime;

#if LOG
            Debug.Log($"*<color=magenta>CCCPPP</color> Will call block list @ {callTime}s");
#endif

#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
            var queriedIds = _pendingValidations.Keys.ToArray();
            var futures = _pendingValidations.Values.ToArray();
            var len = queriedIds.Length;

            bool callAnonymous = !_checkedCrossPermission || _crossFuture != null;

            var batchedFuturesOnCall = _pendingBatchFutures.ToArray();

            SDK.XBL.XblPrivacyBatchCheckPermissionAsync(gdkService.UserData.contextHandle, permissions, queriedIds, callAnonymous ? anonymousUserTypes : noAnonymousUserTypes,
                (Int32 hresult, XblPermissionCheckResult[] permissionCheckResults) =>
                {
                    if (HR.FAILED(hresult))
                    {
                        for (int i = 0; i < len; i++)
                        {
                            ulong id = queriedIds[i];
                            var future = futures[i];
                            _pendingValidations.Remove( id );
                            future.ForceComplete( false );
                        }
                        if (callAnonymous && _crossFuture != null)
                        {
                            _crossFuture.ForceComplete(false);
                            _crossFuture = null;
                        }
                        foreach (var bf in batchedFuturesOnCall)
                        {
                            bf.ForceComplete(_communicationPermissionCache);
                            _pendingBatchFutures.Remove(bf);                         
                        }
                        return;
                    }

                    for (int i = 0; i < len; i++)
                    {
                        ulong id = queriedIds[i];
                        var permission = permissionCheckResults[i];
                        var future = futures[i];
                        _pendingValidations.Remove( id );
                        var permitted = permission.IsAllowed;
                        future.ForceComplete(permitted);
                        _communicationPermissionCache[id] = permitted;
                    }

                    if (callAnonymous && permissionCheckResults.Length > len)
                    {
                        _canCrossPermission = permissionCheckResults[len].IsAllowed;
                        _checkedCrossPermission = true;
                        _communicationPermissionCache[CROSS_NETWORK_FAKE_XUID] = _canCrossPermission;
                        if (_crossFuture != null)
                        {
                            _crossFuture.ForceComplete(_canCrossPermission);
                            _crossFuture = null;
                        }
                    }
                    
                    foreach (var bf in batchedFuturesOnCall)
                    {
                        bf.ForceComplete(_communicationPermissionCache);
                        _pendingBatchFutures.Remove(bf);
                    }
                }
            );
            yield return cooldown;
#else
            //Outside xbox ecosystem we do not check for blocked users, everyone is unblocked
            foreach (var pending in _pendingValidations) pending.Value.ForceComplete(true);
            _pendingValidations.Clear();
#endif
        }

        _coroutine = null;
    }
}