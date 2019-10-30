using System.Collections.Generic;
using UnityEngine;

public static class Cache
{
	private static readonly Dictionary<int, List<GameObject>> _cache = new Dictionary<int, List<GameObject>>();

	private static Transform _parent = null;
	public static Transform CacheParent
	{
		get
		{
#if UNITY_EDITOR
			if( _parent == null ) _parent = new GameObject( "Cache" ).transform;
#endif
			return _parent;
		}
	}

	public static void Add( GameObject reference, int copies )
	{
		List<GameObject> cacheList;
		if( !_cache.TryGetValue( reference.GetInstanceID(), out cacheList ) )
		{
			cacheList = new List<GameObject>();
			_cache[reference.GetInstanceID()] = cacheList;
		}

		var parent = CacheParent;
		var template = GameObject.Instantiate( reference, Vector3.zero, Quaternion.identity, parent );
		template.SetActive( false );
		cacheList.Add( template );

		for( int i = 1; i < copies; i++ ) cacheList.Add( GameObject.Instantiate( template, Vector3.zero, Quaternion.identity, parent ) );
	}

	public static GameObject RequestObject( GameObject reference, Transform transform = null, bool logErrorOnCacheMiss = true )
	{
		List<GameObject> list;
		GameObject go;
		if( _cache.TryGetValue( reference.GetInstanceID(), out list ) && list.Count > 0 )
		{
			go = list[list.Count - 1];
			list.RemoveAt( list.Count - 1 );
			if( list.Count == 0 ) _cache.Remove( reference.GetInstanceID() );
			if( transform != null )
			{
				go.transform.SetParent( transform );
				go.transform.SetPositionAndRotation( transform.position, transform.rotation );
			}
		}
		else
		{
			if( logErrorOnCacheMiss ) Debug.Log( $"Cannot find cache of {reference.NameOrNull()} so we create a new one" );
			if( transform != null ) go = GameObject.Instantiate( reference, transform.position, transform.rotation, transform );
			else go = GameObject.Instantiate( reference );
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
