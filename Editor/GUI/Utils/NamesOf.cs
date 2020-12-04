public class NamesOf<T> where T : IHashedSO, new()
{
	private static string[] _array;

	public static string[] Array
	{
		get
		{
			if( _array == null )
			{
				var element = new T();
				var col = element.GetCollection();
				_array = new string[col?.Count ?? 0];
				for( int i = 0; i < _array.Length; i++ ) _array[i] = col.GetElementBase( i ).ToStringOrNull();
			}
			return _array;
		}
	}
}
