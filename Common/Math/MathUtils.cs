namespace K10
{
	public static class Math
	{
		public static int Log2( int v )
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
	}
}