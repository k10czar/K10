using UnityEngine;
using System.Collections;
using System;

using static Colors.Console;

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
				var stopwatch = new System.Diagnostics.Stopwatch();
				stopwatch.Start();
				var candidate = (T)MonoBehaviour.FindObjectOfType( typeof( T ) );
				Debug.Log( $"{"Shame".Colorfy( Negation )} {"Singleton".Colorfy(TypeName)}<{typeof(T).Name.Colorfy(Keyword)}>.Instance Didn't have a cached object of type! So called {"FindObjectOfType()".Colorfy(Danger)} took:{$"{stopwatch.Elapsed.TotalMilliseconds:N2}ms".Colorfy(Numbers)} {((candidate!=null)?$"{"Found".Colorfy(Verbs)} @ {candidate.HierarchyNameOrNull()}":"Fail".Colorfy(Negation))} on {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Colorfy(Names)}" );
				stopwatch.Stop();
				_instance.RegisterNewReference( candidate );
				if( candidate == null )
				{
					//TODO: Here we can detect a possible performance leak, since we cannot find the instance the requester could keep asking this every frame triggering heavy FindObjectOfType every time
				}
				else
				{
					//Debug.Log( $"Singleton<<color=lime>{(typeof(T))}</color>> found with {candidate.HierarchyNameOrNull()}" );
				}

				// if( !_instance.IsValid )
				// {
				// 	//                    Debug.Log( "An instance of " + typeof(T) + " is needed in the scene, but there is none." );
				// }
			}

			return _instance.Reference;
		}
	}

	public static T GetInstance()
	{
		return GetAutoClearedReference().Reference;
	}

	public static bool IsValid => _instance.IsValid;

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
		if( _instance.IsValid ) 
		{
#if UNITY_EDITOR
			if( _instance.Reference != candidate ) Debug.LogError( $"<color=purple>Shame</color> Conflict with Singleton<<color=lime>{(typeof(T))}</color>>SayHello( {candidate.HierarchyNameOrNull()} ) conflicting with already setted {_instance.Reference.HierarchyNameOrNull()} on <color=yellow>{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}</color>" );
#else
			if( _instance.Reference != candidate ) Debug.LogError( $"Conflict with Singleton<{(typeof(T))}>SayHello( {candidate.HierarchyNameOrNull()} ) conflicting with already setted {_instance.Reference.HierarchyNameOrNull()} on {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}" );
#endif
			return;
		}
		_instance.RegisterNewReference( candidate );
	}

	/// <summary>
	/// Finds the first object (active or inactive)
	/// </summary>
	public static T FindObjectOfTypeAll()
	{
		var stopwatch = new System.Diagnostics.Stopwatch();
		stopwatch.Start();
		var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
		for (int i = 0; i < sceneCount; i++)
		{
			var s =  UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
			var allGameObjects = s.GetRootGameObjects();
			var objectsCount = allGameObjects.Length;
			for (int j = 0; j < objectsCount; j++)
			{
				var go = allGameObjects[j];
				var obj = go.GetComponentInChildren<T>(true);
				if (obj != null && obj is T)
				{
#if UNITY_EDITOR
					Debug.Log( $"<color=purple>Shame</color> Singleton<<color=lime>{typeof(T).Name}</color>>.GetInstance() Didn't have a cached object of type! So will call !!!<color=red>FindObjectOfTypeAll()</color>!!! took:<color=orange>{stopwatch.Elapsed.TotalMilliseconds:N2}</color>ms <color=cyan>Found</color> @ {obj.HierarchyNameOrNull()} on <color=yellow>{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}</color>" );
#else
					Debug.Log( $"Singleton<{typeof(T).Name}>.GetInstance() Didn't have a cached object of type! So will call !!!FindObjectOfTypeAll()!!! took:{stopwatch.Elapsed.TotalMilliseconds:N2}ms Found @ {obj.HierarchyNameOrNull()} on {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}" );
#endif
					stopwatch.Stop();
					return obj;
				}
			}
		}
#if UNITY_EDITOR
		Debug.Log( $"<color=purple>Shame</color> Singleton<<color=lime>{typeof(T).Name}</color>>.GetInstance() Didn't have a cached object of type! So will call !!!<color=red>FindObjectOfTypeAll()</color>!!! took:<color=orange>{stopwatch.Elapsed.TotalMilliseconds:N2}</color>ms <color=red>Fail</color> on <color=yellow>{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}</color>" );
#else
		Debug.Log( $"Singleton<{typeof(T).Name}>.GetInstance() Didn't have a cached object of type! So will call !!!FindObjectOfTypeAll()!!! took:{stopwatch.Elapsed.TotalMilliseconds:N2}ms Fail on {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}" );
#endif
		stopwatch.Stop();
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
}

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
