using System.Collections.Generic;
using System.Text;

namespace K10
{
	public static class ObjectPool<T> where T : new()
	{
		private static readonly Dictionary<System.Threading.Thread,List<T>> _pools = new();

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
				// UnityEngine.Debug.Log( $"New Thread for ObjectPool<{typeof(T)}>( {thread?.Name ?? "NULL"} )[ {(thread?.ManagedThreadId ?? -1)} ]" );
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
}