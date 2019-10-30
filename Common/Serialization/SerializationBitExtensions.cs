public static class SerializationBitExtensions
{
	public static void SerializeBit( this byte[] byteArray, bool read, ref bool value, ref int startingBit )
	{
		if( read ) value = ReadBit( byteArray, ref startingBit );
		else WriteBit( byteArray, value, ref startingBit );
	}

	public static bool ReadBit( this byte[] byteArray, ref int bitPos ) { return ReadBit( byteArray, bitPos++ ); }
	public static bool ReadBit( this byte[] byteArray, int bitPos )
	{
		var id = bitPos;
		var arrayId = id >> 3;
		var bitId = (byte)( id - ( arrayId << 3 ) );
		var bit = (byte)( 1 << bitId );
		return ( byteArray[arrayId] & bit ) != 0;
	}

	public static void WriteBit( this byte[] byteArray, bool data, ref int bitPos ) { WriteBit( byteArray, data, bitPos++ ); }
	public static void WriteBit( this byte[] byteArray, bool data, int bitPos )
	{
		var id = bitPos;
		var arrayId = id >> 3;
		var bitId = (byte)( id - ( arrayId << 3 ) );
		var bit = (byte)( ( 1 ) << bitId );
		if( data ) byteArray[arrayId] |= bit;
		else byteArray[arrayId] &= (byte)( ~bit );
	}
}
