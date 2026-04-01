using UnityEngine;

public abstract class Guaranteed<T> where T : UnityEngine.Component
{
	private static bool _markedEternal = false;

	protected static T GetInstance( bool eternal )
	{
		if( !Singleton<T>.IsValid )
		{
			_markedEternal = false;
			if( Singleton<T>.Instance == null )
			{
				GameObject obj = new GameObject( string.Format( "_GS_{0}", ( typeof( T ) ).ToString() ) );
#if UNITY_EDITOR
				obj.AddComponent<OnlyOnPlaymodeObject>();
#endif
				// Debug.Log( $"GuaranteedSingleton created for {typeof(T)}" );
				Singleton<T>.SayHello( obj.AddComponent<T>() );
			}
		}

		var instance = Singleton<T>.Instance;
		if( eternal && !_markedEternal )
		{
			GameObject.DontDestroyOnLoad( instance.transform );
			_markedEternal = true;
			instance.name = string.Format( "_ES_{0}", ( typeof( T ) ).ToString() );

			instance.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;// | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
		}

		return instance;
	}

	public static T Instance { get { return GetInstance( false ); } }
	public static void Request() { GetInstance( false ); }
}