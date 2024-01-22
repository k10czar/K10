using UnityEngine;

public static class IntExtensions
{
    public static int CompareToAbs( this int value, int otherValue )
    {
        if( otherValue < 0 ) otherValue = -otherValue;
        if( value < 0 ) value = -value;
        return value.CompareTo( otherValue );
    }

    public static int GetDigitsCount( this int value )
    {
        if( value == 0 ) return 1;
        return Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(value)) + 1);
    }
}
