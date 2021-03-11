using System;
using System.Collections.Generic;

public static class SerializationEnumExtensions
{
	public static void SerializeEnumAsBits<T>( this IList<byte> byteArray, bool read, ref T value, ref int startingBit, byte bitsCount ) where T : struct, IConvertible
	{
		if( read ) value = ReadEnumAsBits<T>( byteArray, ref startingBit, bitsCount );
		else WriteEnumAsBits( byteArray, value, ref startingBit, bitsCount );
	}

	public static T ReadEnumAsBits<T>( this IList<byte> byteArray, ref int startingBit, byte bitsCount ) where T : struct, IConvertible
	{
		var id = byteArray.ReadUIntAsBits( ref startingBit, bitsCount );
		return (T)(object)id;
	}

	public static void WriteEnumAsBits<T>( this IList<byte> byteArray, T data, ref int startingBit, byte bitsCount ) where T : struct, IConvertible
	{
		byteArray.WriteUIntAsBits( data.ToInt32( null ), ref startingBit, bitsCount );
	}
}
