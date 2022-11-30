using UnityEngine;
using System.Collections;
using System;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class OnlyOnPlaymodeObject : MonoBehaviour
{
	void Start()
	{
		if( Application.isEditor && !Application.isPlaying )
		{
			GameObject.DestroyImmediate( gameObject );
		}
	}
}

#endif


public abstract class Singleton<T> where T : UnityEngine.Component
{
	private readonly static AutoClearedReference<T> _instance = new AutoClearedReference<T>();

	public static T Instance
	{
		get
		{
			if( !_instance.IsValid )
			{
				var candidate = (T)MonoBehaviour.FindObjectOfType( typeof( T ) );
				_instance.RegisterNewReference( candidate );
				if( candidate == null )
				{
					//TODO: Here we can detect a possible performance leak, since we cannot find the instance the requester could keep asking this every frame triggering heavy FindObjectOfType every time
				}
				else
				{
					Debug.Log( $"Singleton<<color=lime>{(typeof(T))}</color>> found with {candidate.HierarchyNameOrNull()}" );
				}

				// if( !_instance.IsValid )
				// {
				// 	//                    Debug.LogError( "An instance of " + typeof(T) + " is needed in the scene, but there is none." );
				// }
			}

			return _instance.Reference;
		}
	}

	public static T GetInstance()
	{
		return GetAutoClearedReference().Reference;
	}

	public static AutoClearedReference<T> GetAutoClearedReference()
	{
		if (!_instance.IsValid)
		{
			_instance.RegisterNewReference( FindObjectOfTypeAll() );
		}

		return _instance;
	}

	public static void SayHello( T candidate ) 
	{
		if( _instance.IsValid ) return;
		Debug.Log( $"Singleton<<color=lime>{(typeof(T))}</color>> Hello with {candidate.HierarchyNameOrNull()}" );
		_instance.RegisterNewReference( candidate );
	}

	/// <summary>
	/// Finds the first object (active or inactive)
	/// </summary>
	public static T FindObjectOfTypeAll()
	{
		for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
		{
			var s =  UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
			var allGameObjects = s.GetRootGameObjects();
			for (int j = 0; j < allGameObjects.Length; j++)
			{
				var go = allGameObjects[j];
				var obj = go.GetComponentInChildren<T>(true);
				if (obj != null && obj is T)
				{
					return obj;
				}
			}
		}
		return default;
	}
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
	private readonly AutoClearedReference<T> _instance = new AutoClearedReference<T>();
	private GameObject _cachedObject;
	private string _name;
	private IEventRegister _currentReferenceDestroyEvent;

	public NamedSingletonComponent( string objectAttachedName )
	{
		_name = objectAttachedName;
	}

	public T Instance
	{
		get
		{
			if( !_instance.IsValid )
			{
				if( _cachedObject == null )
					_cachedObject = GameObject.Find( _name );

				if( _cachedObject != null )
				{
					_instance.RegisterNewReference( _cachedObject.GetComponent<T>() );
					_currentReferenceDestroyEvent = _cachedObject.EventRelay().OnDestroy;
					_currentReferenceDestroyEvent.Register( OnDestroy );
				}
			}

			return _instance.Reference;
		}
	}

	void OnDestroy()
	{
		_cachedObject = null;
		_currentReferenceDestroyEvent.Unregister( OnDestroy );
		_currentReferenceDestroyEvent = null;
	}
}

public abstract class Eternal<T> : Guaranteed<T> where T : UnityEngine.Component
{
	public new static T Instance { get { return GetInstance( true ); } }
	public new static void Request() { GetInstance( true ); }

	//TODO: Need to implements the DontDestroy on Load?
}

public abstract class Guaranteed<T> where T : UnityEngine.Component
{
	private readonly static AutoClearedReference<T> _instance = new AutoClearedReference<T>();
	private static bool _markedEternal = false;

	protected static T GetInstance( bool eternal )
	{
		if( !_instance.IsValid )
		{
			_markedEternal = false;
			var instance = (T)MonoBehaviour.FindObjectOfType( typeof( T ) );

			if( instance == null )
			{
				GameObject obj = new GameObject( string.Format( "_GS_{0}", ( typeof( T ) ).ToString() ) );
				instance = obj.AddComponent<T>();
#if UNITY_EDITOR
				obj.AddComponent<OnlyOnPlaymodeObject>();
#endif
				// Debug.Log( $"GuaranteedSingleton created for {typeof(T)}" );
			}

			_instance.RegisterNewReference( instance );
			Singleton<T>.SayHello( instance );
		}

		if( eternal && !_markedEternal )
		{
			var reference = _instance.Reference;
			GameObject.DontDestroyOnLoad( reference.transform );
			_markedEternal = true;
			reference.name = string.Format( "_ES_{0}", ( typeof( T ) ).ToString() );

			reference.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;// | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
		}

		return _instance.Reference;
	}

	public static T Instance { get { return GetInstance( false ); } }
	public static void Request() { GetInstance( false ); }

	public static void SayHello( T candidate ) 
	{
		if( !_instance.IsValid )
		{
			_instance.RegisterNewReference( candidate );
			Singleton<T>.SayHello( candidate );
		}
	}
}
