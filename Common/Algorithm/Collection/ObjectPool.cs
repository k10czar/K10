using System.Collections.Generic;
using System.Text;

public static class ObjectPool
{
    public static void Return<T>(T obj) where T : new() => ObjectPool<T>.Return(obj);
    public static void Request<T>( out T obj ) where T : new()
    {
        obj = Request<T>();
    }
    public static void RequestList<T>( out List<T> obj )
    {
        obj = RequestList<T>();
    }
    public static T Request<T>() where T : new() => ObjectPool<T>.Request();
    public static List<T> RequestList<T>() =>  ObjectPool<List<T>>.Request();
    public static void RequestListWith<T>( out List<T> obj, IEnumerable<T> elements )
    {
        RequestList( out obj );
		obj.AddRange( elements );
    }
} 

public static class ObjectPool<T> where T : new()
{
	private static readonly Dictionary<System.Threading.Thread,List<T>> _pools = new Dictionary<System.Threading.Thread,List<T>>();

	public static T Request()
	{
		T obj;
		var thread = System.Threading.Thread.CurrentThread;
		if( !_pools.TryGetValue( thread, out var pool ) ) return new T();
		var count = pool.Count;
		if( count == 0 ) return new T();
		else
		{ 
			obj = pool[count - 1];
			pool.RemoveAt( count - 1 );
			return obj;
		}
	}

	public static void Return( T t ) 
	{
		if( t == null ) return;
		if( t is System.Collections.IList list ) list.Clear();
		if( t is StringBuilder sb ) sb.Clear();
		var thread = System.Threading.Thread.CurrentThread;
		if( !_pools.TryGetValue( thread, out var pool ) )
		{
			// UnityEngine.Debug.Log( $"New Thread for ObjectPool<{typeof(T)}>( {thread?.Name ?? ConstsK10.NULL_STRING} )[ {(thread?.ManagedThreadId ?? -1)} ]" );
			pool = new List<T>();
			_pools.Add( thread, pool );
		}
		pool.Add( t );
	}

	public static void Cache( int size )
	{
		var thread = System.Threading.Thread.CurrentThread;
		if( !_pools.TryGetValue( thread, out var pool ) )
		{
			pool = new List<T>();
			_pools.Add( thread, pool );
		}
		var elementsToAdd = size - pool.Count;
		for( int i = 0; i < elementsToAdd; i++ ) pool.Add( new T() );
	}

	public static void Clear()
	{
		foreach( var kvp in _pools )
		{
			kvp.Value?.Clear();
		}
		_pools.Clear();
	}
}
