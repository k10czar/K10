#if UNITY_WINRT
using UnityEngine.Windows;
#else
#endif

public static class SerializationExtensions
{
	public static int ReadIntAsBits( this byte[] byteArray, byte bitsToRead, int startingBit )
	{
		int value = 0;
		for( int i = 0; i < bitsToRead; i++ )
		{
			var id = startingBit + i;
			var arrayId = id >> 3;
			var bitId = (byte)( id & ( ( arrayId << 4 ) - 1 ) );
			var bit = 1 << bitId;
			if( ( ( byteArray[arrayId] & bit ) != 0 ) ) value |= bit;
		}
		return value;
	}

	public static void WriteIntAsBits( this byte[] byteArray, int data, byte bitsToWrite, int startingBit )
	{
		for( int i = 0; i < bitsToWrite; i++ )
		{
			var b = ( ( data & ( 1 << i ) ) != 0 );
			var id = startingBit + i;
			var arrayId = id >> 3;
			var bitId = (byte)( id & ( ( arrayId << 4 ) - 1 ) );
			var bit = (byte)( ( 1 ) << bitId );
			if( b ) byteArray[arrayId] |= bit;
			else byteArray[arrayId] &= (byte)( ~bit );
		}
	}
}
