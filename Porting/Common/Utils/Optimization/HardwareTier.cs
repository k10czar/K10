#if CODE_METRICS
#define DEBUG_NOTIFY
#endif
using System;
using UnityEngine;

public static class HardwareTier
{
#if UNITY_ANDROID || UNITY_IOS
    static int[] _memoryTiers = { 6000, 8000, 12000, };
#else
    static int[] _memoryTiers = { 6000, 0, 0, };
#endif

    static bool _cached = false;
    static Tier _cachedTier;

    public enum Tier { Low, Mid, High, Extreme }

    public static bool IsExtremeOrAbove => Get() >= Tier.Extreme;
    public static bool IsHighOrAbove => Get() >= Tier.High;
    public static bool IsMidOrAbove => Get() >= Tier.Mid;
    public static bool IsHighOrLower => Get() <= Tier.High;
    public static bool IsMidOrLower => Get() <= Tier.Mid;
    public static bool IsLow => Get() <= Tier.Low;

    static EventSlot _onHardwareTierChanged;

    /// <summary> Fires when the hardware tier changes, only on forced change via <see cref="Force(Tier)"/>. </summary>
    public static IEventRegister OnHardwareTierChanged => _onHardwareTierChanged ??= new EventSlot();

    public static Tier Get()
    {
        if( _cached ) return _cachedTier;
        _cached = true;

        int mem = SystemInfo.systemMemorySize;
        int cores = SystemInfo.processorCount;

        if (mem <= _memoryTiers[0] ) _cachedTier = Tier.Low;
        else if (mem <= _memoryTiers[1] ) _cachedTier = Tier.Mid;
        else if (mem <= _memoryTiers[2]) _cachedTier = Tier.High;
        else _cachedTier = Tier.Extreme;

        DebugTier();

        return _cachedTier;
    }

    public static void SetMemoryTiers( int low, int mid = 0, int high = 0 )
    {
        _memoryTiers[0] = low;
        _memoryTiers[1] = mid;
        _memoryTiers[2] = high;
        _cached = false;
    }

    public static void Force(Tier tierToForce)
    {
        _cachedTier = tierToForce;
        _cached = true;
        DebugTier();
        _onHardwareTierChanged?.Trigger();
    }

    public static void Reset()
    {
        _cached = false;
    }

    static void DebugTier()
    {
#if CODE_METRICS
        var logMesage = $"Hardware Tier: {_cachedTier}";
#if DEBUG_NOTIFY
        NotificationConsole.Notify( logMesage );
#else
        Debug.Log( logMesage );
#endif
#endif
    }
}