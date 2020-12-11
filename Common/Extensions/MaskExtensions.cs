using System;

public static class MaskExtensions
{
	public static bool AsMaskContains<T>( this T data, T mask ) where T : struct, IConvertible => ( data.ToInt32( null ) & mask.ToInt32( null ) ) == mask.ToInt32( null );
	public static bool AsMaskContainsID<T>(this T mask, T elementID) where T : struct, IConvertible => mask.ToInt32(null).AsMaskContains(1 << elementID.ToInt32(null));

	public static void AsMaskWith<T>( this ref int mask, T element ) where T : struct, IConvertible => mask |= element.ToInt32( null );
	public static void AsMaskSet( this ref int mask, int id, bool value )
	{
		if( value ) mask |= 1 << id;
		else mask &= ~(1 << id);
	} 
}
