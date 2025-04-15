// #define DO_NOT_OPTIMIZE
using System.Runtime.CompilerServices;

public static class Optimizations
{
    public const MethodImplOptions INLINE_IF_CAN = 
#if DO_NOT_OPTIMIZE
        MethodImplOptions.NoInlining;
#else
        MethodImplOptions.AggressiveInlining;
#endif
}
