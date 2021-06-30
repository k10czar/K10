using System.Collections.Generic;

public static class Temp<T> where T : new()
{
	public static List<T> _pool = new List<T>();

	public static void Clear() { _pool.Clear(); }
	
	public static T Request() 
	{
		var count = _pool.Count;
		if( count > 0 ) 
		{
			var t = _pool[count - 1];
			_pool.RemoveAt( count - 1 );
			return t;
		}
		return new T();
	}

	public static void Return( T t )
	{
		_pool.Add( t );
		if( t is System.Collections.IList list ) list.Clear();
		// if( t is IClearable c ) c.Clear();
	}
}