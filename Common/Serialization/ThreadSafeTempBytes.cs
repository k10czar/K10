using System.Collections.Generic;

public interface ICustomTempByteSerialization
{
	byte[] WriteBytes();
}

public static class ThreadSafeTempBytes
{
	private static readonly Dictionary<System.Threading.Thread, Dictionary<int, byte[]>> _arrays = new Dictionary<System.Threading.Thread, Dictionary<int, byte[]>>();

	public static byte[] Get( byte bytes )
	{
		var collection = new Dictionary<int, byte[]>();
		var thread = System.Threading.Thread.CurrentThread;
		if( !_arrays.TryGetValue( thread, out collection ) )
		{
			collection = new Dictionary<int, byte[]>();
			_arrays.Add( thread, collection );
		}
		byte[] array;
		if( !collection.TryGetValue( bytes, out array ) )
		{
			array = new byte[bytes];
			collection.Add( bytes, array );
		}
		return array;
	}

	public static byte[] GetClean( byte bytes )
	{
		var array = Get( bytes );
		array.Clear();
		return array;
	}

	public static void Clear()
	{
		_arrays.Clear();
	}
}