using System.Collections.Generic;

public static class ObjectPool<T> where T : new()
{
	private static readonly List<T> _pool = new List<T>();

	public static T Request()
	{
		T obj;
		var count = _pool.Count;
		if( count == 0 ) obj = new T();
		else
		{ 
			obj = _pool[count - 1];
			_pool.RemoveAt( count - 1 );
		}
		return obj;
	}

	public static void Return( T t ) 
	{
		if( t is System.Collections.IList list ) list.Clear();
		_pool.Add( t ); 
	}

	public static void Cache( int size )
	{
		var elementsToAdd = size - _pool.Count;
		for( int i = 0; i < elementsToAdd; i++ ) _pool.Add( new T() );
	}

	public static void Clear() { _pool.Clear(); }
}
