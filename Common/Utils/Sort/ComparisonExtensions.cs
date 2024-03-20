using System;

public static class ComparisonExtensions
{
    public static Comparison<T> Inverse<T>( this Comparison<T> comparison ) => ( T a, T b ) => comparison( b, a );
    public static Comparison<T> SetOrRevertOn<T>( this Comparison<T> comparison, ref Comparison<T> comparisonRef )
    {
        comparisonRef = ( comparisonRef != comparison ) ? comparison : comparison.Inverse();
        return comparisonRef;
    }
}