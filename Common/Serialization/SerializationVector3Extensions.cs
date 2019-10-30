using UnityEngine;

public static class SerializationVector3Extensions
{
	public static void SerializeNormalizedVector3AsBits( this byte[] byteArray, bool read, ref Vector3 value, ref int startingBit, byte bitsCount )
	{
		if( read ) value = ReadNormalizedVector3AsBits( byteArray, ref startingBit, bitsCount );
		else WriteNormalizedVector3AsBits( byteArray, value, ref startingBit, bitsCount );
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
