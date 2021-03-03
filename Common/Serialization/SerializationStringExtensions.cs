using System;
using System.Text;
using UnityEngine;

public static class SerializationStringExtensions
{
	public const int STRING_BITS_COUNT = 8;
	public const int STRING_MAX_COUNT = 1 << STRING_BITS_COUNT;
	public const int CHAR_BITS_COUNT = 16;

	private static readonly StringBuilder SB = new StringBuilder();

	public static int CountBitsToSerialize( this string str ) => STRING_BITS_COUNT + CHAR_BITS_COUNT * Mathf.Min( str.Length, STRING_MAX_COUNT );

	public static void SerializeStringAsBits( this byte[] byteArray, bool read, ref string value, ref int startingBit )
	{
		if( read ) value = ReadStringAsBits( byteArray, ref startingBit );
		else WriteStringAsBits( byteArray, value, ref startingBit );
	}

	public static string ReadStringAsBits( this byte[] byteArray, ref int startingBit )
	{
		SB.Clear();
		var len = byteArray.ReadUIntAsBits( ref startingBit, STRING_BITS_COUNT );
		for( int i = 0; i < len; i++ ) SB.Append( (char)byteArray.ReadUIntAsBits( ref startingBit, CHAR_BITS_COUNT ) );
		return SB.ToString();
	}

	public static void WriteStringAsBits( this byte[] byteArray, string data, ref int startingBit )
	{
		var len = Mathf.Min( data.Length, STRING_MAX_COUNT );
		byteArray.WriteUIntAsBits( data.Length, ref startingBit, STRING_BITS_COUNT );
		for( int i = 0; i < len; i++ ) byteArray.WriteUIntAsBits( data[i], ref startingBit, CHAR_BITS_COUNT );
	}
}
