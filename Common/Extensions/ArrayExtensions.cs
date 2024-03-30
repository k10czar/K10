using System.Collections.Generic;

public static class ArrayExtensions
{
    public static T[] With<T>( this T[] terms, params T[] newElements )
    {
        var len = terms?.Length ?? 0;
        var addedLen = newElements?.Length ?? 0;
        var newArray = new T[len+addedLen];
        for( int i = 0; i < len; i++ ) newArray[i] = terms[i];
        for( int i = 0; i < addedLen; i++ ) newArray[len+i] = newElements[i];
        return newArray;
    }
}