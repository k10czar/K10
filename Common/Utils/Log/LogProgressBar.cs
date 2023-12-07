using UnityEngine;

public static class ConsoleProgressBar
{
	const char FULL_BLOCK = '█';
	const char BLOCK_3_4 = '▓';
	const char BLOCK_2_4 = '▒';
	const char BLOCK_1_4 = '░';

	static readonly Color LOW_COLOR = Color.green;
	static readonly Color HIGH_COLOR = Color.red;

	static readonly System.Text.StringBuilder SB = new System.Text.StringBuilder();

	public static string Create( float pct, int totalBlocks, bool showPercentage = true, bool colored = true )
	{
		SB.Clear();

		var blocksPct = pct * totalBlocks / 100;
		var fullblocksPct = Mathf.FloorToInt( blocksPct );
		var lastBlock = blocksPct - fullblocksPct;

		for( int i = 0; i < fullblocksPct; i++ ) SB.Append( FULL_BLOCK );
		if( totalBlocks > fullblocksPct && lastBlock > .0001 )
		{
			if( lastBlock > .6666 ) SB.Append( BLOCK_3_4 );
			if( lastBlock > .3333 ) SB.Append( BLOCK_2_4 );
			else SB.Append( BLOCK_1_4 );
		}

		if( pct > 4 ) SB.Append( $" {pct:N0}%" );
		else if( pct > .4 ) SB.Append( $" {pct:N1}%" );
		else if( pct > .04 ) SB.Append( $" {pct:N2}%" );
		else if( pct > .004 ) SB.Append( $" {pct:N3}%" );
		else if( pct > .0004 ) SB.Append( $" {pct:N4}%" );
		else if( pct > .00004 ) SB.Append( $" {pct:N5}%" );
		else SB.Append( $" {pct:N6}%" );
		
		var ret = SB.ToString();
		SB.Clear();
		#if UNITY_EDITOR
		return ret.Colorfy( Color.Lerp( LOW_COLOR, HIGH_COLOR, pct / 100 ) );
		#else
		return ret;
		#endif //UNITY_EDITOR
	}
}
