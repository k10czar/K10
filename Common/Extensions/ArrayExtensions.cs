public static class ArrayExtensions
{
    public static T[] With<T>( this T[] terms, T newElement )
    {
        var len = terms?.Length ?? 0;
        var newArray = new T[len+1];
        for( int i = 0; i < len; i++ ) newArray[i] = terms[i];
        newArray[len] = newElement;
        return newArray;
    }
}
