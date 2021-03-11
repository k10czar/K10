

using System.Collections.Generic;

public static class SerializationGuidExtensions
{
	public const int GUID_BYTES = 16;

	public static void SerializeGuidAsBitsIfValid( this IList<byte> byteArray, bool read, ref System.Guid value, ref int startingBit )
	{
		var isValid = false;
		if( !read ) isValid = System.Guid.Empty == value;
		byteArray.SerializeBit( read, ref isValid, ref startingBit );
		if( isValid ) byteArray.SerializeGuidAsBits( read, ref value, ref startingBit );
	}

	public static void SerializeGuidAsBits( this IList<byte> byteArray, bool read, ref System.Guid value, ref int startingBit )
	{
		if( read ) value = ReadGuidAsBits( byteArray, ref startingBit );
		else WriteGuidAsBits( byteArray, value, ref startingBit );
	}

	private static System.Guid ReadGuidAsBits( this IList<byte> byteArray, ref int startingBit )
	{
		var data = new byte[GUID_BYTES];
		for( int i = 0; i < GUID_BYTES; i++ ) data[i] = byteArray.ReadByteAsBits( ref startingBit );
		return new System.Guid( data );
	}

	private static void WriteGuidAsBits( this IList<byte> byteArray, System.Guid value, ref int startingBit )
	{
		var array = value.ToByteArray();
		for( int i = 0; i < GUID_BYTES; i++ ) byteArray.WriteByteAsBits( array[i], ref startingBit );
	}
}
