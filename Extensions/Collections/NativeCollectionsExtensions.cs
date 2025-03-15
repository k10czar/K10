

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

public static class NativeArrayExtensions
{
	// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeArray<T> ToNativeArray<T>( this IList<T> collection, Allocator allocator ) where T : struct
    {
        var count = collection.Count;
        var natArray = new NativeArray<T>( count, allocator, NativeArrayOptions.UninitializedMemory );
        for( int i = 0; i < count; i++ ) natArray[i] = collection[i];
        return natArray;
    }

    // public static NativeArray<T> CombineToNativeArray<T>( this List<List<T>> collection, Allocator allocator, out NativeArray<int> indexes, out NativeArray<int> counts ) where T : struct
    //     => CombineToNativeArray<T>( (IList<IList<T>>) collection, allocator, out indexes, out counts );

    public static NativeArray<T> CombineToNativeArray<T>( this List<List<T>> collection, Allocator allocator, out NativeArray<int> indexes, out NativeArray<int> counts ) where T : struct
    {
        var count = 0;
        var arrays = collection.Count;
        foreach( var item in collection ) count += item.Count;
        var natArray = new NativeArray<T>( count, allocator, NativeArrayOptions.UninitializedMemory );
        indexes = new NativeArray<int>( arrays, allocator, NativeArrayOptions.UninitializedMemory );
        counts = new NativeArray<int>( arrays, allocator, NativeArrayOptions.UninitializedMemory );

        var acc = 0;
        for (int i = 0; i < collection.Count; i++) 
        {
            var item = collection[i];
            indexes[i] = acc;
            counts[i] = item.Count;
            for ( int j = 0; j < item.Count; j++ ) natArray[acc++] = item[j];
        }

        return natArray;
    }
}