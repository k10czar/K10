

using System.Collections.Generic;

public static class SerializationIntExtensions
{
	public static void SerializeIntAsBits( this IList<byte> byteArray, bool read, ref int value, ref int startingBit, byte bitsCount )
	{
		if( read ) value = ReadIntAsBits( byteArray, ref startingBit, bitsCount );
		else WriteIntAsBits( byteArray, value, ref startingBit, bitsCount );
	}

	public static int ReadIntAsBits( this IList<byte> byteArray, ref int startingBit, byte bitsToRead )
	{
		var ret = ReadIntAsBits( byteArray, startingBit, bitsToRead );
		startingBit += bitsToRead;
		return ret;
	}

	public static int ReadIntAsBits( this IList<byte> byteArray, int startingBit, byte bitsToRead )
	{
		int value = 0;
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

	public static void WriteIntAsBits( this IList<byte> byteArray, int data, ref int startingBit, byte bitsToWrite )
	{
		WriteIntAsBits( byteArray, data, startingBit, bitsToWrite );
		startingBit += bitsToWrite;
	}

	public static void WriteIntAsBits( this IList<byte> byteArray, int data, int startingBit, byte bitsToWrite )
	{		
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
