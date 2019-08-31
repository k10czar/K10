using UnityEngine;
using System.Collections;


public static class BinaryAdapter
{
	public static T Deserialize<T>( byte[] bytes )
	{
		if( bytes == null || bytes.Length == 0 )
			return default( T );

		var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
		T value = default( T );

		using( var stream = new System.IO.MemoryStream( bytes ) )
		{
			value = (T)binaryFormatter.Deserialize( stream );
			stream.Flush();
		}

		return value;
	}

	public static T DeserializeOrNew<T>( byte[] bytes ) where T : new()
	{
		if( bytes == null || bytes.Length == 0 )
			return new T();

		var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
		T value = default( T );

		using( var stream = new System.IO.MemoryStream( bytes ) )
		{
			value = (T)binaryFormatter.Deserialize( stream );
			stream.Flush();
		}

		return value;
	}

	public static byte[] Serialize<T>( T obj )
	{
		var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
		byte[] bytes;
		
		using( var stream = new System.IO.MemoryStream() )
		{
			binaryFormatter.Serialize( stream, obj );
			bytes = stream.ToArray();
		}
		
		return bytes;
	}
}