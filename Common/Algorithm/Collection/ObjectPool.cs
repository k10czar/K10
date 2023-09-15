using System;
using System.Collections.Generic;
using System.Threading;

public static class ObjectPool<T> where T : new()
{
	private static readonly ThreadLocal<List<T>> _pool = new ThreadLocal<List<T>>(() => new List<T>());

	public static T Request()
	{
		if (!_pool.IsValueCreated) return new T();
		
		var pool = _pool.Value;
		var count = pool.Count;
		if (count == 0) return new T();
		T obj = pool[count - 1];
		pool.RemoveAt( count - 1 );
		return obj;
	}

	public static void Return( T t )
	{
		if( t is System.Collections.IList list ) list.Clear();
		_pool.Value.Add(t);
	}

	public static void Cache( int size )
	{
		var pool = _pool.Value;
		var elementsToAdd = size - pool.Count;
		for( int i = 0; i < elementsToAdd; i++ ) pool.Add( new T() );
	}

	public static void Clear()
	{
		foreach(var pool in _pool.Values) {
			pool.Clear();
		}
	}
}
