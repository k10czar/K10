using UnityEngine;
using System.Collections;
using System;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class OnlyOnPlaymodeObject : MonoBehaviour
{
	void Start()
	{
		if( Application.isEditor )
		{
			GameObject.DestroyImmediate( this );
		}
	}
}
#endif

public abstract class Singleton<T> where T : UnityEngine.Object
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if( _instance == null )
			{
				_instance = (T)MonoBehaviour.FindObjectOfType( typeof( T ) );

				if( _instance == null )
				{
					//                    Debug.LogError( "An instance of " + typeof(T) + " is needed in the scene, but there is none." );
				}
			}

			return _instance;
		}
	}

	public static void SayHello( T instance ) { if( _instance == null ) _instance = instance; }
}

public abstract class ClassSingleton<T> where T : new()
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if( _instance == null ) _instance = new T();
			return _instance;
		}
	}
}

public class NamedSingletonComponent<T> where T : UnityEngine.Component
{
	private T _instance;
	private GameObject _cachedObject;
	private string _name;

	public NamedSingletonComponent( string objectAttachedName )
	{
		_name = objectAttachedName;
	}

	public T Instance
	{
		get
		{
			if( _instance == null )
			{
				if( _cachedObject == null )
					_cachedObject = GameObject.Find( _name );

				if( _cachedObject != null )
					_instance = _cachedObject.GetComponent<T>();
			}

			return _instance;
		}
	}
}

public abstract class Eternal<T> : Guaranteed<T> where T : UnityEngine.Component
{
	public new static T Instance { get { return GetInstance( true ); } }
	public new static void Request() { GetInstance( true ); }
}

public abstract class Guaranteed<T> where T : UnityEngine.Component
{
	private static T _instance;
	private static bool _markedEternal = false;

	protected static T GetInstance( bool eternal )
	{
		if( _instance == null )
		{
			_markedEternal = false;
			_instance = (T)MonoBehaviour.FindObjectOfType( typeof( T ) );

			if( _instance == null )
			{
				GameObject obj = new GameObject( string.Format( "_GS_{0}", ( typeof( T ) ).ToString() ) );
				_instance = obj.AddComponent<T>();
				#if UNITY_EDITOR
				obj.AddComponent<OnlyOnPlaymodeObject>();
				#endif
			}
		}

		if( eternal && !_markedEternal )
		{
			GameObject.DontDestroyOnLoad( _instance.transform );
			_markedEternal = true;
			_instance.name = string.Format( "_ES_{0}", ( typeof( T ) ).ToString() );

			_instance.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;// | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
		}

		return _instance;
	}

	public static T Instance { get { return GetInstance( false ); } }
	public static void Request() { GetInstance( false ); }

	public static void SayHello( T instance ) { if( _instance == null ) _instance = instance; }
}
