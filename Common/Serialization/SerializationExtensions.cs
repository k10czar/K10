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
}
