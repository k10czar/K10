using System.Collections.Generic;
using K10;

public static class DictionaryExtensions
{
    public static TValue Request<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, TKey key ) where TValue : new()
    {
        if (dictionary.TryGetValue( key, out var value )) return value;
        value = new TValue();
        dictionary.Add( key, value );
        return value;
    }

    public static void AppendCount<T>( this Dictionary<T, int> dictionary, T key, int countToAdd = 1 )
    {
        dictionary.TryGetValue( key, out var count );
        count += countToAdd;
        dictionary[key] = count;
    }

    public static void ClearAndReturnToPool<T,K>( this Dictionary<T,K> collection )
    {
        collection.Clear();
        ObjectPool.Return( collection );
    }
}
