public interface IInterpolationFunc<T>
{
    T Interpolate( T a, T b, float delta );
}

public static class InterpolationFuncExtension
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static T SafeInterpolate<T>( this IInterpolationFunc<T> func, T a, T b, float delta )
    {
        if( func == null ) return ( delta < .5f ) ? a : b;
        return func.Interpolate( a, b, delta );
    }
}