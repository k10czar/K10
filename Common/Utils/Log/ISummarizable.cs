using System.Collections;
using System.Text;
using K10;
using UnityEngine;

public interface ISummarizable
{
	string Summarize();
}

public static class SummarizableExtensions
{
	public static string TrySummarize( this object obj, string collectionSeparator = ", " )
	{
		if( obj is ISummarizable sum ) return sum.Summarize();
		if( obj is Object uObj ) return uObj.NameAndType();
		if( obj is IEnumerable collection ) 
		{
			ObjectPool.Request<StringBuilder>( out var SB );
			bool first = true;
			foreach( var item in collection ) 
			{
				if( !first ) SB.Append( item );
				SB.Append( item.TrySummarize( collectionSeparator ) );
				first = false;
			}
			var ret = SB.ToString();
			ObjectPool.Return( SB );
			return ret;
		}
		return obj.ToStringOrNull();
	}
}
