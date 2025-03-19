#define AGGRESSIVE_INLINING
using System.Runtime.CompilerServices;

public static class Optimizations
{
    public const MethodImplOptions INLINING = 
#if AGGRESSIVE_INLINING
        MethodImplOptions.AggressiveInlining;
#else
        MethodImplOptions.NoInlining;
#endif
}
