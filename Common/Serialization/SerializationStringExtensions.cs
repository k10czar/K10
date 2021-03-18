using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class SerializationStringExtensions
{
	public const int STRING_BITS_COUNT = 8;
	public const int STRING_MAX_COUNT = ( 1 << STRING_BITS_COUNT ) - 1;
	public const int CHAR_BITS_COUNT = 16;

	private static readonly StringBuilder SB = new StringBuilder();

	public static int CountBitsToSerialize( this string str ) => STRING_BITS_COUNT + CHAR_BITS_COUNT * Mathf.Min( str.Length, STRING_MAX_COUNT );

	public static void SerializeStringAsBits( this IList<byte> bytes, bool read, ref string value, ref int startingBit )
	{
		if( read ) value = ReadStringAsBits( bytes, ref startingBit );
		else WriteStringAsBits( bytes, value, ref startingBit );
	}

	public static void SerializeStringAsBitsIfValid( this IList<byte> bytes, bool read, ref string value, ref int startingBit )
	{
		bool isValid = false;
		if( !read ) isValid = !string.IsNullOrEmpty( value );
		bytes.SerializeBit( read, ref isValid, ref startingBit );
		if( isValid ) bytes.SerializeStringAsBits( read, ref value, ref startingBit );
	}

	public static string ReadStringAsBits( this IList<byte> bytes, ref int startingBit )
	{
		string returnValue = null;
		lock( SB )
		{
			SB.Clear();
			var len = bytes.ReadUIntAsBits( ref startingBit, STRING_BITS_COUNT );
			for( int i = 0; i < len; i++ ) SB.Append( (char)bytes.ReadUIntAsBits( ref startingBit, CHAR_BITS_COUNT ) );
			returnValue = SB.ToString();
		}
		return returnValue;
	}

	public static void WriteStringAsBits( this IList<byte> bytes, string data, ref int startingBit )
	{
		var len = Mathf.Min( data.Length, STRING_MAX_COUNT );
		bytes.WriteUIntAsBits( data.Length, ref startingBit, STRING_BITS_COUNT );
		for( int i = 0; i < len; i++ ) bytes.WriteUIntAsBits( data[i], ref startingBit, CHAR_BITS_COUNT );
	}
}
