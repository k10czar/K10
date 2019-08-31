using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public static class PrefabCache<T> where T : Component
{
	static List<T> _cache;
	static List<string> _names;
	static string[] _namesWithNone;

	public static List<T> Cache { get { if( _cache == null ) _cache = ReadAllPrefabsWith(); return _cache; } }
	public static List<string> Names { get { if( _names == null ) _names = Cache.ConvertAll( ( T t ) => t.name ); return _names; } }

	public static string[] NamesWithNone 
	{ 
		get 
		{ 
			if( _namesWithNone == null )
			{
				var names = Names;
				_namesWithNone = new string[names.Count+1];
				_namesWithNone[0] = "None";
				for( int i = 0; i < names.Count; i++ ) _namesWithNone[i+1] = names[i];
			}
			return _namesWithNone; 
		} 
	}

	public static void Refresh() { _cache = null; _names = null; _namesWithNone = null; }

	static List<T> ReadAllPrefabsWith()
	{
		var guids = AssetDatabase.FindAssets( "t:GameObject" );

		List<T> ret = new List<T>();
		for( int i = 0; i < guids.Length; i++ )
		{
			var obj = AssetDatabase.LoadAssetAtPath<T>( AssetDatabase.GUIDToAssetPath( guids[i] ) );
			if( obj != null )
				ret.Add( obj );
		}

		return ret;
	}
}
