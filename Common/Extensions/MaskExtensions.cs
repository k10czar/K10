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
}
