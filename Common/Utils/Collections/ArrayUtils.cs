

public static class ArrayUtils<T>
{
	private static T[] _empty = null;

	public static T[] Empty()
	{
        if( _empty == null ) _empty = new T[0];
		return _empty;
	}
}