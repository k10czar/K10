using System.Collections.Generic;
using System.Collections;
using K10;
using UnityEngine;

public static class Cache
{
	private static readonly Dictionary<int, List<GameObject>> _cache = new Dictionary<int, List<GameObject>>();

	private static Transform _parent = null;
	public static Transform CacheParent
	{
		get
		{
			if (_parent == null)
			{
				_parent = new GameObject( "Cache" ).transform;
				GameObject.DontDestroyOnLoad(_parent);
			}
			return _parent;
		}
	}

	public static void DisableAndReturnToCacheList( this GameObject gameObject, GameObject listReference, float seconds )
	{
		ExternalCoroutine.StartCoroutine( CO_DisableAndReturnToCacheOn( gameObject, listReference, seconds ) );
	}

	static System.Collections.IEnumerator CO_DisableAndReturnToCacheOn( GameObject gameObject, GameObject listReference, float seconds )
	{
		yield return new WaitForSeconds( seconds );
		if( gameObject != null )
		{
			gameObject.SetActive( false );
			RequestCacheList( listReference ).Add( gameObject );
		}
	}

	static List<GameObject> RequestCacheList( GameObject listReference )
	{
		var id = listReference.GetInstanceID();
		if( !_cache.TryGetValue( id, out var cacheList ) ) { _cache[id] = cacheList = new List<GameObject>(); }
		return cacheList;
	}

	public static List<GameObject> Add( GameObject reference, int copies , bool dontDestroyOnLoad = false)
	{
		// Debug.Log( $"Cache {copies} {reference}" );
		var cacheList = RequestCacheList( reference );

		var parent = CacheParent;

		var template = GameObject.Instantiate( reference, Vector3.zero, Quaternion.identity, parent );
		template.SetActive( false );

		cacheList.Add( template );

		for (int i = 0; i <= copies; i++)
		{
			GameObject newObj = GameObject.Instantiate(template, Vector3.zero, Quaternion.identity, parent);
			cacheList.Add( newObj );
		}

		return cacheList;
	}

	public static IEnumerator AddCoroutine( GameObject reference, int copies )
	{
		var cacheList = RequestCacheList( reference );

		var parent = CacheParent;

		var template = GameObject.Instantiate( reference, Vector3.zero, Quaternion.identity, parent );
		template.SetActive( false );

		cacheList.Add( template );

		for (int i = 0; i < copies; i++)
		{
			GameObject newObj = GameObject.Instantiate( template, Vector3.zero, Quaternion.identity, parent );
			cacheList.Add( newObj );
			yield return null;
		}
	}


	public static GameObject RequestObject( GameObject reference, Transform transform = null, bool logErrorOnCacheMiss = true )
	{
		GameObject go;
		var id = reference.GetInstanceID();
		if( _cache.TryGetValue( id, out var list ) && list.Count > 0 )
		{
			go = list[list.Count - 1];
			list.RemoveAt( list.Count - 1 );
			if( list.Count == 0 ) _cache.Remove( id );
			if( transform != null )
			{
				go.transform.SetParent( transform );
				go.transform.SetPositionAndRotation( transform.position, transform.rotation );
			}
		}
		else
		{
			// if( logErrorOnCacheMiss ) Debug.Log( $"Cannot find cache of {reference.NameOrNull()} so we create a new one" );
			if( transform != null ) go = GameObject.Instantiate( reference, transform.position, transform.rotation, transform );
			else go = GameObject.Instantiate( reference );
		}
		return go;
	}

	public static GameObject RequestObject( GameObject reference, Vector3 position, Quaternion rotation, bool logErrorOnCacheMiss = true )
	{
		GameObject go;
		var id = reference.GetInstanceID();
		if( _cache.TryGetValue( id, out var list ) && list.Count > 0 )
		{
			go = list[list.Count - 1];
			list.RemoveAt( list.Count - 1 );
			if( list.Count == 0 ) _cache.Remove( id );
			go.transform.SetPositionAndRotation( position, rotation );
		}
		else
		{
			// if( logErrorOnCacheMiss ) Debug.Log( $"Cannot find cache of {reference.NameOrNull()} so we create a new one" );
			go = GameObject.Instantiate( reference, position, rotation );
		}
		return go;
	}

	public static void Clear()
	{
		foreach( var kvp in _cache )
		{
			var list = kvp.Value;
			for( int i = 0; i < list.Count; i++ )
			{
				var go = list[i];
				if( go == null ) continue;
				GameObject.Destroy( go );
			}
		}
		_cache.Clear();
	}
}