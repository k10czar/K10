using UnityEngine;

public static class SerializationFloatExtensions
{
	public static void SerializeFloatAsFixedAsBits( this byte[] byteArray, bool read, ref float value, ref int startingBit, byte bitsCount, float minRange, float maxRange )
	{
		if( read ) value = ReadFloatAsFixedAsBits( byteArray, ref startingBit, bitsCount, minRange, maxRange );
		else WriteFloatAsFixedAsBits( byteArray, value, ref startingBit, bitsCount, minRange, maxRange );
	}

	public static float ReadFloatAsFixedAsBits( this byte[] byteArray, ref int startingBit, byte bitsToRead, float minRange, float maxRange )
	{
		var ret = ReadFloatAsFixedAsBits( byteArray, startingBit, bitsToRead, minRange, maxRange );
		startingBit += bitsToRead;
		return ret;
	}

	public static float ReadFloatAsFixedAsBits( this byte[] byteArray, int startingBit, byte bitsToRead, float minRange, float maxRange )
	{
		var maxValue = (float)( ( 1 << bitsToRead ) - 1 );
		float value = ( byteArray.ReadUIntAsBits( startingBit, bitsToRead ) / maxValue ) * ( maxRange - minRange );
		return minRange + value;
	}

	public static void WriteFloatAsFixedAsBits( this byte[] byteArray, float data, ref int startingBit, byte bitsToWrite, float minRange, float maxRange )
	{
		WriteFloatAsFixedAsBits( byteArray, data, startingBit, bitsToWrite, minRange, maxRange );
		startingBit += bitsToWrite;
	}

	public static void WriteFloatAsFixedAsBits( this byte[] byteArray, float data, int startingBit, byte bitsToWrite, float minRange, float maxRange )
	{
		var maxValue = ( 1 << bitsToWrite ) - 1;
		var value = Mathf.RoundToInt( Mathf.Clamp01( ( data - minRange ) / ( maxRange - minRange ) ) * maxValue );
		byteArray.WriteUIntAsBits( value, startingBit, bitsToWrite );
	}
}
