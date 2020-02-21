using UnityEngine;

public static class SerializationFloatExtensions
{
	public static void SerializeFloatAsFixedAsBits( this byte[] byteArray, bool read, ref float value, ref int startingBit, byte bitsCount, float minRange, float maxRange )
	{
		if( read ) value = ReadFloatAsFixedAsBits( byteArray, ref startingBit, bitsCount, minRange, maxRange );
		else WriteFloatAsFixedAsBits( byteArray, value, ref startingBit, bitsCount, minRange, maxRange );
	}

	public static void ConditionalSerializeFloatAsFixedAsBits( this byte[] byteArray, bool read, ref float value, ref bool condition, ref int startingBit, byte bitsCount, float minRange, float maxRange, float defaultValue = 0 )
	{
		if( read ) value = ConditionalReadFloatAsFixedAsBits( byteArray, out condition, ref startingBit, bitsCount, minRange, maxRange, defaultValue );
		else ConditionalWriteFloatAsFixedAsBits( byteArray, value, condition, ref startingBit, bitsCount, minRange, maxRange );
	}

	public static float ReadFloatAsFixedAsBits( this byte[] byteArray, ref int startingBit, byte bitsToRead, float minRange, float maxRange )
	{
		var ret = ReadFloatAsFixedAsBits( byteArray, startingBit, bitsToRead, minRange, maxRange );
		startingBit += bitsToRead;
		return ret;
	}

	public static float ConditionalReadFloatAsFixedAsBits( this byte[] byteArray, out bool condition, ref int startingBit, byte bitsToRead, float minRange, float maxRange, float defaultValue = 0 )
	{
		condition = byteArray.ReadBit( ref startingBit );
		if( condition ) return ReadFloatAsFixedAsBits( byteArray, ref startingBit, bitsToRead, minRange, maxRange );
		return defaultValue;
	}

	public static float ConditionalReadFloat01AsFixedAsBits( this byte[] byteArray, out bool condition, ref int startingBit, byte bitsToRead, float defaultValue = 0 )
	{
		return byteArray.ConditionalReadFloatAsFixedAsBits( out condition, ref startingBit, bitsToRead, 0, 1 );
	}

	public static float ReadFloat01AsFixedAsBits( this byte[] byteArray, ref int startingBit, byte bitsToRead ) => ReadFloatAsFixedAsBits( byteArray, ref startingBit, bitsToRead, 0, 1 );

	public static float ReadFloatAsFixedAsBits( this byte[] byteArray, int startingBit, byte bitsToRead, float minRange, float maxRange )
	{
		var maxValue = ( 1 << bitsToRead ) - 1;
		var step = ( maxRange - minRange ) / maxValue;
		float value = minRange + byteArray.ReadUIntAsBits( startingBit, bitsToRead ) * step;
		var roundBase = K10.Math.Base10( Mathf.Max( (int)(1 / step), 1 ) );
		var roundedValue = Mathf.Round( value * roundBase ) / roundBase;
		return roundedValue;
	}

	public static void WriteFloatAsFixedAsBits( this byte[] byteArray, float data, ref int startingBit, byte bitsToWrite, float minRange, float maxRange )
	{
		WriteFloatAsFixedAsBits( byteArray, data, startingBit, bitsToWrite, minRange, maxRange );
		startingBit += bitsToWrite;
	}

	public static void ConditionalWriteFloatAsFixedAsBits( this byte[] byteArray, float data, ref int startingBit, byte bitsToWrite, float minRange, float maxRange, float ignoredValue = 0 )
	{
		var condition = !Mathf.Approximately( data, ignoredValue );
		byteArray.WriteBit( condition, ref startingBit );
		if( condition ) WriteFloatAsFixedAsBits( byteArray, data, ref startingBit, bitsToWrite, minRange, maxRange );
	}

	public static void ConditionalWriteFloatAsFixedAsBits( this byte[] byteArray, float data, bool condition, ref int startingBit, byte bitsToWrite, float minRange, float maxRange )
	{
		byteArray.WriteBit( condition, ref startingBit );
		if( condition ) WriteFloatAsFixedAsBits( byteArray, data, ref startingBit, bitsToWrite, minRange, maxRange );
	}

	public static void ConditionalWriteFloat01AsFixedAsBits( this byte[] byteArray, float data, bool condition, ref int startingBit, byte bitsToWrite )
	{
		byteArray.ConditionalWriteFloatAsFixedAsBits( data, condition, ref startingBit, bitsToWrite, 0, 1 );
	}

	public static void ConditionalWriteFloat01AsFixedAsBits( this byte[] byteArray, float data, ref int startingBit, byte bitsToRead, float ignoredValue = 0 )
	{
		byteArray.ConditionalWriteFloatAsFixedAsBits( data, ref startingBit, bitsToRead, 0, 1, ignoredValue );
	}

	public static void WriteFloat01AsFixedAsBits( this byte[] byteArray, float data, ref int startingBit, byte bitsToWrite ) { WriteFloatAsFixedAsBits( byteArray, data, ref startingBit, bitsToWrite, 0, 1 ); }

	public static void WriteFloatAsFixedAsBits( this byte[] byteArray, float data, int startingBit, byte bitsToWrite, float minRange, float maxRange )
	{
		var maxValue = ( 1 << bitsToWrite ) - 1;
		var value = Mathf.RoundToInt( Mathf.Clamp01( ( data - minRange ) / ( maxRange - minRange ) ) * maxValue );
		byteArray.WriteUIntAsBits( value, startingBit, bitsToWrite );
	}
}
