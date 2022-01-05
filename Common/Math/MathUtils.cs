using System.Runtime.CompilerServices;

namespace K10
{
	public static class Math
	{
		const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;
		[MethodImpl( AggrInline )] public static int Log2( int v )
		{
			int r = 0xFFFF - v >> 31 & 0x10;
			v >>= r;
			int shift = 0xFF - v >> 31 & 0x8;
			v >>= shift;
			r |= shift;
			shift = 0xF - v >> 31 & 0x4;
			v >>= shift;
			r |= shift;
			shift = 0x3 - v >> 31 & 0x2;
			v >>= shift;
			r |= shift;
			r |= ( v >> 1 );
			return r;
		}

		[MethodImpl( AggrInline )] public static byte GetBitsCount( int maxValue ) => (byte)( Log2( maxValue ) + 1 );
		[MethodImpl( AggrInline )] public static byte GetBytesCount( int bits ) => (byte)( ( ( bits - 1 ) >> 3 ) + 1 );

		private static readonly byte[] guess = new byte[]{
			0, 0, 0, 0, 1, 1, 1, 2, 2, 2,
			3, 3, 3, 3, 4, 4, 4, 5, 5, 5,
			6, 6, 6, 6, 7, 7, 7, 8, 8, 8,
			9, 9, 9
		};

		private static readonly int[] tenToThe = new int[]{
			1, 			10, 		100, 		1000, 		10000, 
			100000,		1000000,	10000000,	100000000,	1000000000,
		};

		[MethodImpl( AggrInline )] public static int Log10( int x ) => guess[Log2( x )];
		[MethodImpl( AggrInline )] public static int Pow10( int x ) => tenToThe[x];
		[MethodImpl( AggrInline )] public static int Base10( int x ) => tenToThe[guess[Log2( x )]];

		[MethodImpl( AggrInline )] public static float SafeDivision(float x, float y, float valueIfZero = 0)
		{
			if(y == 0) return valueIfZero;
			return x / y;
		}
	}
}