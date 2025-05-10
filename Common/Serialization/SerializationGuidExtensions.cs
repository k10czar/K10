

using System.Collections.Generic;
using K10;

public static class SerializationGuidExtensions
{
	public const int GUID_BYTES = 16;

	public static void SerializeGuidAsBitsIfValid( this IList<byte> byteArray, bool read, ref System.Guid value, ref int startingBit )
	{
		var isValid = false;
		if( !read ) isValid = System.Guid.Empty != value;
		byteArray.SerializeBit( read, ref isValid, ref startingBit );
		if( isValid ) byteArray.SerializeGuidAsBits( read, ref value, ref startingBit );
	}

	public static void SerializeGuidAsBits( this IList<byte> byteArray, bool read, ref System.Guid value, ref int startingBit )
	{
		if( read ) value = ReadGuidAsBits( byteArray, ref startingBit );
		else WriteGuidAsBits( byteArray, value, ref startingBit );
	}

	private static System.Guid ReadGuidAsBits( this IList<byte> byteArray, ref int startingBit )
	{
		var data = new byte[GUID_BYTES];
		for( int i = 0; i < GUID_BYTES; i++ ) data[i] = byteArray.ReadByteAsBits( ref startingBit );
		return new System.Guid( data );
	}

	private static void WriteGuidAsBits( this IList<byte> byteArray, System.Guid value, ref int startingBit )
	{
		var array = value.ToByteArray();
		for( int i = 0; i < GUID_BYTES; i++ ) byteArray.WriteByteAsBits( array[i], ref startingBit );
	}

    private static char ToHexDigitChar( int i ) => (char) (i < 10 ? ('0' + i) : ('a' + (i - 10)));
    private static byte HexDigitToByte( char c )
    {
		if( c < '0' || c > 'f' ) throw new System.Exception( $"Invalid hex character: {c}" );
        if( /*c >= '0' &&*/ c <= '9' ) return (byte)(c - '0');
        if( c >= 'a' /*&& c <= 'f'*/ ) return (byte)((c - 'a') + 10);
        if( c >= 'A' && c <= 'F' ) return (byte)((c - 'A') + 10);
        throw new System.Exception( $"Invalid hex character: {c}" );
    }
	
    private static string ShortenGuidFormat( this string bigString )
    {
        var SB = ObjectPool<System.Text.StringBuilder>.Request();

        for( int i = 0; i < bigString.Length; i+=2 )
        {
            var c = (char)( HexDigitToByte(bigString[i]) * 16 + HexDigitToByte(bigString[i+1]));
            SB.Append( c );
        }

        var shorten = SB.ToString();
        ObjectPool<System.Text.StringBuilder>.Return( SB );
        return shorten;
    }

    private static string ExpandedGuidFormat( this string shortenString )
    {
        var SB = ObjectPool<System.Text.StringBuilder>.Request();

        for( int i = 0; i < shortenString.Length; i++ )
        {
            var c = (byte)shortenString[i];
            SB.Append( ToHexDigitChar( c / 16 ) );
            SB.Append( ToHexDigitChar( c % 16 ) );
        }

        var big = SB.ToString();
        ObjectPool<System.Text.StringBuilder>.Return( SB );
        return big;
    }
}
