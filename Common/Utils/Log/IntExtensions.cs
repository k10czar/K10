using UnityEngine;

public static class IntExtensions
{
    public static int GetDigitsCount( this int value )
    {
        if( value == 0 ) return 1;
        return Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(value)) + 1);
    }
}
