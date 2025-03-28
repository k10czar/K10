using System.Collections;
using System.Collections.Generic;
using K10;

public static class CollectionExtensions
{
    public static bool ContainsElementsFromType<T>( this System.Collections.IEnumerable collection )
    {
        foreach( var element in collection )
        {
            if( element is T ) return true;
        }
        return false;
    }

    public static void ClearAndReturnToPool<T,K>( this K collection ) where K : ICollection<T>, new()
    {
        collection.Clear();
        ObjectPool.Return( collection );
    }

    public static bool ContainsElementsFromType( this System.Collections.IEnumerable collection, System.Type typeOfElements )
    {
        foreach( var element in collection )
        {
            if( element != null && element.GetType() == typeOfElements ) return true;
        }
        return false;
    }

    public static bool RemoveAllFromType<T>( this IList list )
    {
        bool removed = false;
        for (int i = list.Count - 1; i >= 0 ; i--)
        {
            if( list[i] is T ) 
            {
                removed = true;
                list.RemoveAt( i );
            }
        }
        return removed;
    }

    public static bool RemoveAllFromType( this IList list, System.Type typeOfElements )
    {
        bool removed = false;
        for (int i = list.Count - 1; i >= 0 ; i--)
        {
            var element = list[i];
            if( element != null && element.GetType() == typeOfElements )
            {
                removed = true;
                list.RemoveAt( i );
            }
        }
        return removed;
    }

    public static int IndexOfFirstElementFromType<T>( this IList list, int notFoundReturnValue = -1 )
    {
        for (int i = 0; i < list.Count; ++i)
            if (list[i] is T)
                return i;
        return notFoundReturnValue;
    }

    public static int IndexOfFirstElementFromType( this IList list, System.Type typeOfElement, int notFoundReturnValue = -1 )
    {
        for (int i = 0; i < list.Count; ++i)
        {
            var element = list[i];
            if( element != null && element.GetType() == typeOfElement )
                return i;
        }
        return notFoundReturnValue;
    }

    public static IReadOnlyList<T> With<T>( this IReadOnlyList<T> terms, IReadOnlyList<T> newElements )
    {
        var len = terms?.Count ?? 0;
        var addedLen = newElements?.Count ?? 0;
        var newArray = new T[len+addedLen];
        for( int i = 0; i < len; i++ ) newArray[i] = terms[i];
        for( int i = 0; i < addedLen; i++ ) newArray[len+i] = newElements[i];
        return newArray;
    }
}