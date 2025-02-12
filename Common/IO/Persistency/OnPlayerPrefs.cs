using UnityEngine;

public static class OnPlayerPrefsInt<T> where T : struct, System.IConvertible
{
	static bool inited = false;

	private static readonly ValueState<T> _current = new ValueState<T>();
	public static IValueStateObserver<T> Current
	{
		get
		{
			if( !inited )
			{
				Set( PlayerPrefsAdapter.GetInt( typeof( T ).ToString() ) );
				_current.OnChange.Register( UpdatePlayerPrefs );
				inited = true;
			}
			return _current;
		}
	}

	private static void UpdatePlayerPrefs( T value )
	{
		PlayerPrefsAdapter.SetInt( typeof( T ).ToString(), value.ToInt32( null ) );
		PlayerPrefsAdapter.Save();
	}

	public static void Set( T value ) { _current.Setter( value ); }
	public static void Set( int valueID ) { _current.Setter( (T)(object)valueID ); }
}