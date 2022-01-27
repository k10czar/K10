using System.Collections.Generic;

public static class SerializationListExtensions
{
	public static void SerializeIntList( this IList<byte> bytes, bool read, ref int it, ref List<int> list, int maxElements = 255, int maxId = 511 )
	{
		var nullList = false;
		if( !read ) nullList = list == null;
		bytes.SerializeBit( read, ref nullList, ref it );

		if( read ) 
		{
			if( nullList ) return;

			if( list == null ) list = new List<int>();
			else list.Clear();
		}
		else if( nullList )
		{
			list = null;
			return;
		}

		int huntersCount = 0;
		if( !read ) huntersCount = list.Count;
		bytes.SerializeUIntAsBits( read, ref huntersCount, ref it, K10.Math.GetBitsCount( maxElements ) );

		var bytesToEachElements = K10.Math.GetBitsCount( maxId );
		for( int i = 0; i < huntersCount; i++ )
		{
			int huntersID = 0;
			if( !read ) huntersID = list[i];
			bytes.SerializeUIntAsBits( read, ref huntersID, ref it, bytesToEachElements );
			if( read ) list.Add( huntersID );
		}
	}
}
