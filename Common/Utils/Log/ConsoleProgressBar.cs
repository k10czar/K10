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

    public static string CreateColored(float fill, int totalBlocks = 10, bool showPercentage = true) => CreateColored(fill, LOW_COLOR, HIGH_COLOR, totalBlocks, showPercentage);
    public static string CreateColored(float fill, Color lowColor, Color highColor, int totalBlocks = 10, bool showPercentage = true) => CreateColored(fill, Color.Lerp(LOW_COLOR, HIGH_COLOR, fill), totalBlocks, showPercentage);
    public static string CreateColored( float fill, Color colorIfEditor, int totalBlocks = 10, bool showPercentage = true )
	{
		var ret = Create( fill, totalBlocks, showPercentage );
		#if UNITY_EDITOR
		return ret.Colorfy( colorIfEditor );
		#else
		return ret;
		#endif //UNITY_EDITOR
	}

    public static string Create( float fill, int totalBlocks = 10, bool showPercentage = true )
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

		if( showPercentage ) SB.Append( fill.ToPercentageString() );
		
		var ret = SB.ToString();
		SB.Clear();
		return ret;
	}
}
