

using System.Collections.Generic;

public static class SerializationUIntExtensions
{
	public static void SerializeByteAsBits( this IList<byte> byteArray, bool read, ref byte value, ref int startingBit, byte bitsCount )
	{
		if( read ) value = ReadByteAsBits( byteArray, ref startingBit, bitsCount );
		else WriteByteAsBits( byteArray, value, ref startingBit, bitsCount );
	}

	public static void SerializeUIntAsBits( this IList<byte> byteArray, bool read, ref int value, ref int startingBit, byte bitsCount )
	{
		if( read ) value = ReadUIntAsBits( byteArray, ref startingBit, bitsCount );
		else WriteUIntAsBits( byteArray, value, ref startingBit, bitsCount );
	}

	public static byte ReadByteAsBits( this IList<byte> byteArray, ref int startingBit, byte bitsToRead = 8 ) => (byte)ReadUIntAsBits( byteArray, ref startingBit, bitsToRead );
	public static int ReadUIntAsBits( this IList<byte> byteArray, ref int startingBit, byte bitsToRead )
	{
		var ret = ReadUIntAsBits( byteArray, startingBit, bitsToRead );
		startingBit += bitsToRead;
		return ret;
	}

	public static byte ReadByteAsBits( this IList<byte> byteArray, int startingBit, byte bitsToRead = 8 ) => (byte)ReadUIntAsBits( byteArray, startingBit, bitsToRead );
	public static int ReadUIntAsBits( this IList<byte> byteArray, int startingBit, byte bitsToRead )
	{
		int value = 0;

		if (bitsToRead >= 31) bitsToRead = 30;

		for( int i = 0; i < bitsToRead; i++ )
		{
			var id = startingBit + i;
			var arrayId = id >> 3;
			var bitId = (byte)( id - ( arrayId << 3 ) );
			var bit = (byte)( 1 << bitId );
			if( ( ( byteArray[arrayId] & bit ) != 0 ) ) value |= ( 1 << i );
		}
		return value;
	}

	public static void WriteByteAsBits( this IList<byte> byteArray, byte data, ref int startingBit, byte bitsToWrite = 8 )
	{
		WriteByteAsBits( byteArray, data, startingBit, bitsToWrite );
		startingBit += bitsToWrite;
	}

	public static void WriteUIntAsBits( this IList<byte> byteArray, int data, ref int startingBit, byte bitsToWrite )
	{
		WriteUIntAsBits( byteArray, data, startingBit, bitsToWrite );
		startingBit += bitsToWrite;
	}

	public static void WriteByteAsBits( this IList<byte> byteArray, byte data, int startingBit, byte bitsToWrite = 8 ) => WriteUIntAsBits( byteArray, data, startingBit, bitsToWrite );
	public static void WriteUIntAsBits( this IList<byte> byteArray, int data, int startingBit, byte bitsToWrite )
	{
		if (bitsToWrite >= 31) bitsToWrite = 30;
		var max = ( 1 << bitsToWrite ) - 1;
		if( data > max ) data = max;
		
		for( int i = 0; i < bitsToWrite; i++ )
		{
			var b = ( ( data & ( 1 << i ) ) != 0 );
			var id = startingBit + i;
			var arrayId = id >> 3;
			var bitId = (byte)( id - ( arrayId << 3 ) );
			var bit = (byte)( ( 1 ) << bitId );
			if( b ) byteArray[arrayId] |= bit;
			else byteArray[arrayId] &= (byte)( ~bit );
		}
	}
}
