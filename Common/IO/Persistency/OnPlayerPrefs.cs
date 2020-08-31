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
				Set( PlayerPrefs.GetInt( typeof( T ).ToString() ) );
				_current.OnChange.Register( UpdatePlayerPrefs );
				inited = true;
			}
			return _current;
		}
	}

	private static void UpdatePlayerPrefs( T privacy )
	{
		PlayerPrefs.SetInt( typeof( T ).ToString(), privacy.ToInt32( null ) );
		PlayerPrefs.Save();
	}

	public static void Set( T privacy ) { _current.Setter( privacy ); }
	public static void Set( int privacyCode ) { _current.Setter( (T)(object)privacyCode ); }
}