#define LOG
using System.Collections;
using UnityEngine;
using K10.Promises;
using System;
using System.Collections.Generic;
using System.Linq;
using K10.Platforms;


#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
using Unity.XGamingRuntime;
#endif

public static class NameValidation
{
    const float CALL_COOLDOWN = 5;
    const string DEFAULT_NAME = "Player";

    static Dictionary<string, Future<string>> _pendingValidations = new();
    static Dictionary<string, string> _validatedNames = new();
    static Coroutine _coroutine;

    public static bool TryGetValidateName(string name, out string validatedName)
    {
#if !UNITY_GAMECORE && !MICROSOFT_GDK_SUPPORT
        //Outside xbox ecosystem we do not check for name validation
        validatedName = name;
        return true;
#endif

        if (name.Equals(DEFAULT_NAME))
        {
            validatedName = name;
            return true;
        }

        if (_validatedNames.TryGetValue(name, out validatedName)) return true;

        // TODO: Reenbla this code when PlatformManager code is reenabled
        // does not validate xbox gamertags since we believe that microsoft already validate those
		// var platform = PlatformManager.GetPlatformFamilyFromNickname( name );
        // if (platform == EPlatformFamily.Xbox)
        // {
        //     _validatedNames[name] = name;
        //     validatedName = name;
        //     return true;
        // }

        return false;
    }

    public static Future<string> ValidateStringAsync(string candidate)
    {
        if (string.IsNullOrEmpty(candidate)) candidate = "NULL";
        if (_pendingValidations.TryGetValue(candidate, out var result)) return result;
        var fut = new Future<string>();
        _pendingValidations[candidate] = fut;
        if (_coroutine == null) _coroutine = ExternalCoroutine.StartCoroutine(Coroutine());
#if LOG
        Debug.Log( $"*<color=magenta>VVVSSS</color> ValidateStringAsync( {candidate} ) created a new Request" );
#endif
        return fut;
    }

    private static IEnumerator Coroutine()
    {
#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT
        var gdkService = ServiceLocator.Get<IGdkRuntimeService>() as GdkGameRuntimeService;
        var xboxDelay = new WaitForSecondsRealtime( CALL_COOLDOWN );
#endif 
        while (_pendingValidations.Count > 0)
        {
            yield return null; // wait at least one frame to complete futures or accumulate cases to call once

            if (_pendingValidations.Count == 0) break;

#if UNITY_GAMECORE || MICROSOFT_GDK_SUPPORT

            var candidates = _pendingValidations.Keys.ToList();
            var futures = _pendingValidations.Values.ToList();
            var candidatesWithoutPrefixs = new string[candidates.Count];

            var callTime = Time.unscaledTime;

#if LOG
            var SBD = StringBuilderPool.RequestEmpty();
            SBD.AppendLine($"*<color=magenta>VVVSSS</color> Will call batched String validation for {candidates.Count} cases @ {callTime}s");
#endif

            for (int i = 0; i < candidates.Count; i++)
            {
                string candidate = candidates[i];
                candidatesWithoutPrefixs[i] = PartyMember.GetNicknameWithoutPlatformCode(candidate);
#if LOG
                SBD.AppendLine($"   {i}){candidate}");
#endif
            }


#if LOG
            Debug.Log(SBD.ReturnToPoolAndCast());
#endif

            SDK.XBL.XblStringVerifyStringsAsync(gdkService.UserData.contextHandle, candidatesWithoutPrefixs,
                (Int32 hresult, XblVerifyStringResult[] results) =>
                {
                    var returnTime = Time.unscaledTime;
                    var took = returnTime - callTime;

                    bool failed = HR.FAILED(hresult);
                    var len = results.Length;

#if LOG
                    var SB = StringBuilderPool.RequestEmpty();
                    SB.AppendLine($"*<color=yellow>VVVSSS</color> Batched String validation {(failed ? "FAIL" : "SUCCESS")} for {len} cases took {took}s");
#endif

                    for (int i = 0; i < len; i++)
                    {
                        var result = results[i];
                        var candidate = candidates[i];
                        var future = futures[i];

#if LOG
                        SB.AppendLine($"   {i}){candidate} - {result.ResultCode}");
#endif

                        if (!failed && result.ResultCode == XblVerifyStringResultCode.Success)
                        {
                            _validatedNames[candidate] = candidate;
                            _pendingValidations.Remove(candidate);
                            future.ForceComplete(candidate);
                        }
                        else
                        {
                            if (failed)
                                Debug.LogError($"Failed to validate string. HR: {hresult}");
                            else
                            {
                                string reason = result.ResultCode == XblVerifyStringResultCode.Offensive ? "OFFENSIVE" : result.ResultCode == XblVerifyStringResultCode.TooLong ? "TOO LONG" : "UNKNWON";
                                Debug.Log($"*<color=yellow>VVVSSS</color> String {candidate} is not adequate to Xbox services because it is {reason}");
                            }

                            string obfuscatedName = NameChecker.ObfuscateName(candidate);
                            _validatedNames[candidate] = obfuscatedName;
                            _pendingValidations.Remove(candidate);
                            future.ForceComplete(obfuscatedName);
                        }
                    }

#if LOG
                    Debug.Log(SB.ReturnToPoolAndCast());
#endif
                }
            );
            yield return xboxDelay;
#else
            foreach( var pending in _pendingValidations )
            {
                var candidate = pending.Key;
                _validatedNames[candidate] = candidate;
                pending.Value.ForceComplete(candidate);
            }
            _pendingValidations.Clear();
#endif 
        }

        _coroutine = null;
    }
}