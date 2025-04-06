using System.Text;
using K10;

public static class StringBuilderPool
{
	public static StringBuilder RequestEmpty()
	{
		ObjectPool.Request<StringBuilder>( out var stringBuilder );
		stringBuilder.Clear();
		return stringBuilder;
	}

	public static StringBuilder RequestWith( string firstLine )
	{
		var stringBuilder = RequestEmpty();
		stringBuilder.AppendLine( firstLine );
		return stringBuilder;
	}

	public static void ReturnAndClear( StringBuilder stringBuilder)
	{
		stringBuilder.Clear();
		ObjectPool.Return( stringBuilder );
	}

	public static void ReturnToPool( this StringBuilder stringBuilder)
	{
		stringBuilder.Clear();
		ObjectPool.Return( stringBuilder );
	}

	public static string ReturnClearAndCast( StringBuilder stringBuilder)
	{
		string str = stringBuilder.ToString();
		stringBuilder.Clear();
		ObjectPool.Return( stringBuilder );
		return str;
	}

	public static string ReturnToPoolAndCast( this StringBuilder stringBuilder)
	{
		string str = stringBuilder.ToString();
		stringBuilder.Clear();
		ObjectPool.Return( stringBuilder );
		return str;
	}
}