#if CODE_METRICS
#define DEBUG_NOTIFY
#endif
using UnityEngine;

public static class HardwareTier
{
    // Default bounds in GB; overridden at runtime by the baked project settings via SetMemoryTiers.
    static float[] _memoryTiers = { 6f, 0f, 0f, };

    static bool _cached = false;
    static Tier _cachedTier;

    public enum Tier { Low, Mid, High, Extreme }

    public static bool IsExtreme => Get() == Tier.Extreme;
    public static bool IsHighOrAbove => Get() >= Tier.High;
    public static bool IsHighOrLower => Get() <= Tier.High;
    public static bool IsMidOrAbove => Get() >= Tier.Mid;
    public static bool IsMidOrLower => Get() <= Tier.Mid;
    public static bool IsLow => Get() == Tier.Low;

    public static Tier Get()
    {
        if( _cached ) return _cachedTier;
        _cached = true;

        float memGB = SystemInfo.systemMemorySize / 1000f; // systemMemorySize is in MB
        int cores = SystemInfo.processorCount;

        if (memGB <= _memoryTiers[0] ) _cachedTier = Tier.Low;
        else if (memGB <= _memoryTiers[1] ) _cachedTier = Tier.Mid;
        else if (memGB <= _memoryTiers[2]) _cachedTier = Tier.High;
        else _cachedTier = Tier.Extreme;

        DebugTier();

        return _cachedTier;
    }

    public static void SetMemoryTiers( float low, float mid = 0, float high = 0 )
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