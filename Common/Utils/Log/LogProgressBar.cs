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

	public static string Create( float fill, int totalBlocks, bool showPercentage = true, bool colored = true )
	{
		SB.Clear();

		var blocksPct = fill * totalBlocks;
		var fullblocksPct = Mathf.FloorToInt( blocksPct );
		var lastBlock = blocksPct - fullblocksPct;

		for( int i = 0; i < fullblocksPct; i++ ) SB.Append( FULL_BLOCK );
		if( totalBlocks > fullblocksPct && lastBlock > .0001 )
		{
			if( lastBlock > .6666 ) SB.Append( BLOCK_3_4 );
			else if( lastBlock > .3333 ) SB.Append( BLOCK_2_4 );
			else SB.Append( BLOCK_1_4 );
		}

		if( showPercentage )
		{
			if( fill > .04 ) SB.Append( $" {100*fill:N0}%" );
			else if( fill > .004 ) SB.Append( $" {100*fill:N1}%" );
			else if( fill > .0004 ) SB.Append( $" {100*fill:N2}%" );
			else if( fill > .00004 ) SB.Append( $" {100*fill:N3}%" );
			else if( fill > .000004 ) SB.Append( $" {100*fill:N4}%" );
			else if( fill > .0000004 ) SB.Append( $" {100*fill:N5}%" );
			else SB.Append( $" {100*fill:N6}%" );
		}
		
		var ret = SB.ToString();
		SB.Clear();
		#if UNITY_EDITOR
		return ret.Colorfy( Color.Lerp( LOW_COLOR, HIGH_COLOR, fill ) );
		#else
		return ret;
		#endif //UNITY_EDITOR
	}
}
