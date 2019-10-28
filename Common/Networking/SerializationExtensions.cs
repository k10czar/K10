using UnityEngine;

public static class SerializationExtensions
{
	public static void Clear( this byte[] byteArray )
	{
		for( int i = 0; i < byteArray.Length; i++ ) byteArray[i] = 0;
	}

	public static string DebugBitMask( this byte[] byteArray, int startingBit, int bitsToRead )
	{
		string mask = "";

		for( int i = 0; i < bitsToRead; i++ )
		{
			var id = startingBit + i;
			var arrayId = id >> 3;
			var bitId = (byte)( id - ( arrayId << 3 ) );
			var bit = 1 << bitId;
			mask += ( ( ( byteArray[arrayId] & bit ) != 0 ) ) ? '1' : '0';
		}
		return mask;
	}

	public static byte ReadByteAsBits( this byte[] byteArray, ref int startingBit, byte bitsToRead ) => (byte)ReadUIntAsBits( byteArray, ref startingBit, bitsToRead );
	public static int ReadUIntAsBits( this byte[] byteArray, ref int startingBit, byte bitsToRead )
	{
		var ret = ReadUIntAsBits( byteArray, startingBit, bitsToRead );
		startingBit += bitsToRead;
		return ret;
	}

	public static byte ReadByteAsBits( this byte[] byteArray, int startingBit, byte bitsToRead ) => (byte)ReadUIntAsBits( byteArray, startingBit, bitsToRead );
	public static int ReadUIntAsBits( this byte[] byteArray, int startingBit, byte bitsToRead )
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

	public static void WriteByteAsBits( this byte[] byteArray, byte data, ref int startingBit, byte bitsToWrite )
	{
		WriteByteAsBits( byteArray, data, startingBit, bitsToWrite );
		startingBit += bitsToWrite;
	}

	public static void WriteUIntAsBits( this byte[] byteArray, int data, ref int startingBit, byte bitsToWrite )
	{
		WriteUIntAsBits( byteArray, data, startingBit, bitsToWrite );
		startingBit += bitsToWrite;
	}

	public static void WriteByteAsBits( this byte[] byteArray, byte data, int startingBit, byte bitsToWrite ) => WriteUIntAsBits( byteArray, data, startingBit, bitsToWrite );
	public static void WriteUIntAsBits( this byte[] byteArray, int data, int startingBit, byte bitsToWrite )
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

	public static bool ReadBit( this byte[] byteArray, ref int bitPos ) 
	{
		var ret = ReadBit( byteArray, bitPos );
		bitPos++;
		return ret;
	}
	public static bool ReadBit( this byte[] byteArray, int bitPos )
	{		
		var id = bitPos;
		var arrayId = id >> 3;
		var bitId = (byte)( id - ( arrayId << 3 ) );
		var bit = (byte)( 1 << bitId );
		return ( byteArray[arrayId] & bit ) != 0;
	}

	public static void WriteBit( this byte[] byteArray, bool data, ref int bitPos ) 
	{
		WriteBit( byteArray, data, bitPos );
		bitPos++;
	}
	public static void WriteBit( this byte[] byteArray, bool data, int bitPos )
	{
		var id = bitPos;
		var arrayId = id >> 3;
		var bitId = (byte)( id - ( arrayId << 3 ) );
		var bit = (byte)( ( 1 ) << bitId );
		if( data ) byteArray[arrayId] |= bit;
		else byteArray[arrayId] &= (byte)( ~bit );
	}

	public static float ReadFloatAsFixedOnBits( this byte[] byteArray, ref int startingBit, byte bitsToRead, float minRange, float maxRange )
	{
		var ret = ReadFloatAsFixedOnBits( byteArray, startingBit, bitsToRead, minRange, maxRange );
		startingBit += bitsToRead;
		return ret;
	}

	public static float ReadFloatAsFixedOnBits( this byte[] byteArray, int startingBit, byte bitsToRead, float minRange, float maxRange )
	{
		var maxValue = (float)( ( 1 << bitsToRead ) - 1 );
		float value = ( byteArray.ReadUIntAsBits( startingBit, bitsToRead ) / maxValue ) * ( maxRange - minRange );
		return minRange + value;
	}

	public static void WriteFloatAsFixedOnBits( this byte[] byteArray, float data, ref int startingBit, byte bitsToWrite, float minRange, float maxRange )
	{
		WriteFloatAsFixedOnBits( byteArray, data, startingBit, bitsToWrite, minRange, maxRange );
		startingBit += bitsToWrite;
	}
	
	public static void WriteFloatAsFixedOnBits( this byte[] byteArray, float data, int startingBit, byte bitsToWrite, float minRange, float maxRange )
	{
		var maxValue = ( 1 << bitsToWrite ) - 1;
		var value = Mathf.RoundToInt( Mathf.Clamp01( ( data - minRange ) / ( maxRange - minRange ) ) * maxValue );
		byteArray.WriteUIntAsBits( value, startingBit, bitsToWrite );
	}

	public static Vector3 ReadNormalizedVector3AsBits( this byte[] byteArray, ref int startingBit, byte bitsToRead )
	{
		var ret = ReadNormalizedVector3AsBits( byteArray, startingBit, bitsToRead );
		startingBit += bitsToRead;
		return ret;
	}
	public static Vector3 ReadNormalizedVector3AsBits( this byte[] byteArray, int startingBit, byte bitsToRead )
	{
		byte lessBits = (byte)( bitsToRead >> 1 );
		byte moreBits = (byte)( bitsToRead - lessBits );

		int rotYcompact = byteArray.ReadUIntAsBits( 0, moreBits );
		int rotXZcompact = byteArray.ReadUIntAsBits( moreBits, lessBits );
		var rotY = ( rotYcompact * PI_2 ) / ( 1 << moreBits );
		var rotXZ = ( ( rotXZcompact * Mathf.PI ) / ( ( 1 << lessBits ) - 2 ) ) - HALF_PI;

		var y = Mathf.Sin( rotXZ );
		var projY = Mathf.Cos( rotXZ );
		var x = projY * Mathf.Sin( rotY );
		var z = projY * Mathf.Cos( rotY );

		return new Vector3( x, y, z );
	}

	private static float PI_2 = 2 * Mathf.PI;
	private static float HALF_PI = Mathf.PI / 2;
	public static void WriteNormalizedVector3AsBits( this byte[] byteArray, Vector3 data, ref int startingBit, byte bitsPrecision )
	{
		WriteNormalizedVector3AsBits( byteArray, data, startingBit, bitsPrecision );
		startingBit += bitsPrecision;
	}
	public static void WriteNormalizedVector3AsBits( this byte[] byteArray, Vector3 data, int startingBit, byte bitsPrecision )
	{
		byte lessBits = (byte)( bitsPrecision >> 1 );
		byte moreBits = (byte)( bitsPrecision - lessBits );

		float rotY = Mathf.Atan2( data.x, data.z );
		var projXZ = Mathf.Sqrt( data.x * data.x + data.z * data.z );
		float rotXZ = 2 * ( Mathf.Atan2( data.y, projXZ ) + HALF_PI );
		int rotYcompact = RadPositiveToInt( RadPositiveRange( rotY ), moreBits );
		int rotXZcompact = HalfRadPositiveToInt( rotXZ, lessBits );
		byteArray.WriteUIntAsBits( rotYcompact, 0, moreBits );
		byteArray.WriteUIntAsBits( rotXZcompact, moreBits, lessBits );
	}

	public static float RadPositiveRange( float radAngle ) => ( ( radAngle + PI_2 ) % PI_2 );
	public static float RadSafePositiveRange( float radAngle ) => ( ( ( radAngle % PI_2 ) + PI_2 ) % PI_2 );
	public static int RadToInt( float rads, int bitsPrecision ) => RadPositiveToInt( RadSafePositiveRange( rads ), bitsPrecision );
	public static int RadPositiveToInt( float angle, int bitsPrecision ) => Mathf.RoundToInt( ( angle / PI_2 ) * ( 1 << bitsPrecision ) );
	public static int HalfRadPositiveToInt( float halfRadAngle, int bitsPrecision ) => Mathf.RoundToInt( ( halfRadAngle / PI_2 ) * ( ( 1 << bitsPrecision ) - 2 ) );
}
