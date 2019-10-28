using System.Collections.Generic;

public interface ICustomTempByteSerialization
{
	byte[] Serialize();
}

public static class TempBytes
{
	private static readonly Dictionary<int, byte[]> _arrays = new Dictionary<int, byte[]>();

	public static byte[] Get( byte bytes )
	{
		byte[] array;
		if( !_arrays.TryGetValue( bytes, out array ) )
		{
			array = new byte[bytes];
			_arrays[bytes] = array;
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