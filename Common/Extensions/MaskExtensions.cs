using System;
using System.Runtime.CompilerServices;

public static class MaskExtensions
{
	const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;
	
	[MethodImpl( AggrInline )] public static bool AsMaskContains<T>( this T data, T mask ) where T : struct, IConvertible => ( data.ToInt32( null ) & mask.ToInt32( null ) ) == mask.ToInt32( null );
	[MethodImpl( AggrInline )] public static bool AsMaskContainsID<T>(this T mask, T elementID) where T : struct, IConvertible => mask.ToInt32(null).AsMaskContains(1 << elementID.ToInt32(null));

	[MethodImpl( AggrInline )] public static void AsMaskRefWith<T>( this ref int mask, T element ) where T : struct, IConvertible => mask |= element.ToInt32( null );
	[MethodImpl( AggrInline )] public static void AsMaskRefSet( this ref int mask, int id, bool value )
	{
		if( value ) mask |= 1 << id;
		else mask &= ~( 1 << id );
	}

	[MethodImpl( AggrInline )] public static int AsMaskWith<T>( this int mask, T element ) where T : struct, IConvertible => mask |= element.ToInt32( null );
	[MethodImpl( AggrInline )] public static int AsMaskSet( this int mask, int id, bool value )
	{
		if( value ) mask |= 1 << id;
		else mask &= ~( 1 << id );
		return mask;
	}

	public static string DebugMaskValues<T>( this T mask ) where T : struct, IConvertible
	{
		string str = "";
		var intMask = mask.ToInt32(null);
		foreach( T t in System.Enum.GetValues( typeof(T) ) )
		{
			if( ( intMask & t.ToInt32(null) ) == 0 ) continue;
			if( string.IsNullOrEmpty( str ) ) str += t.ToStringOrNull();
			else str += $", {t.ToStringOrNull()}";
		}
		return str;
	}

	public static void WriteBitsAsRef( this ref int data, int initialBit, int bitCount, int value ) 
	{
		data = WriteBits( data, initialBit, bitCount, value );
	}


	public static int WriteBits(this int data, int initialBit, int bitCount, int value)
	{
		var mask = ((1 << bitCount) - 1) << initialBit;
		data = data & (~mask);

		return data | ((value << initialBit) & mask);
	}

	public static int ReadBits(this int data, int initialBit, int bitCount)
	{
		var mask = ((1 << bitCount) - 1) << initialBit;
		return (data & mask) >> initialBit;
	}
}
