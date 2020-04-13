using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using K10.EditorGUIExtention;
using System.Linq;

public static class K10EditorGUIUtils
{
	public static TerrainData CreateTerrainData( string path )
	{
		return CreateTerrainData( path, new Vector3( 2000, 600, 2000 ), 512, 1024 );
	}

	public static TerrainData CreateTerrainData( string path, Vector3 size, int hResolution, int bResolution )
	{
		TerrainData terrainData = new TerrainData();
		terrainData.size = size;

		terrainData.heightmapResolution = hResolution;
		terrainData.baseMapResolution = bResolution;
		terrainData.SetDetailResolution( 1024, 16 );

		FileAdapter.RequestFilePath( path );
		AssetDatabase.CreateAsset( terrainData, path );
		return terrainData;
	}

	static void InitGO<T>( T t, string name ) where T : Behaviour
	{
		if( typeof( T ) == typeof( Terrain ) )
		{
			var terrain = t as Terrain;
			terrain.terrainData = CreateTerrainData( "Assets/Prefabs/TerrainData/" + name + "Data.asset" );
			var col = t.gameObject.AddComponent<TerrainCollider>();
			col.terrainData = terrain.terrainData;
		}
	}

	public static T CreateSceneGO<T>( string name ) where T : Behaviour
	{
		GameObject go = new GameObject( name );
		var ret = go.AddComponent<T>();
		InitGO<T>( ret, name );
		return ret;
	}

	public static bool GoPropHasComponent<T>( SerializedProperty prop ) where T : Component
	{
		if( prop == null ) return false;
		var oref = prop.objectReferenceValue;
		if( oref == null ) return false;
		var go = oref as GameObject;
		if( go == null ) return false;
		var comp = go.GetComponent<T>();
		if( comp == null ) return false;

		return true;
	}

	public static void RemoveItemFromArray( SerializedProperty listProp, int id )
	{
		for( int j = id + 1; j < listProp.arraySize; j++ )
		{
			listProp.MoveArrayElement( j, j - 1 );
			var prop = listProp.GetArrayElementAtIndex( j - 1 );
		}
		listProp.arraySize--;
	}

	public static T CreateSequentialGO<T>( string newPrefabPrefix ) where T : Component
	{
		if( !newPrefabPrefix.StartsWith( "Assets/" ) ) newPrefabPrefix = "Assets/" + newPrefabPrefix;
		if( !FileAdapter.Exists( newPrefabPrefix + ".prefab" ) ) return CreateGO<T>( newPrefabPrefix );
		int id = 2;
		while( FileAdapter.Exists( newPrefabPrefix + id + ".prefab" ) ) id++;

		return CreateGO<T>( newPrefabPrefix + id );
	}

	public static T CreateGO<T>( string newPath ) where T : Component
	{
		if( !newPath.StartsWith( "Assets/" ) ) newPath = "Assets/" + newPath;
		T ret = null;
		GameObject go = new GameObject();
		try
		{
			FileAdapter.RequestFilePath( newPath );
			go.AddComponent<T>();

			if( !FileAdapter.Exists( newPath + ".prefab" ) || EditorUtility.DisplayDialog( "Warning!", "File (" + newPath + ".prefab" + ") already exists you want to replace that file? This will delete the already existing Prefab", "Yes", "No" ) )
			{
				var refGo = PrefabUtility.SaveAsPrefabAsset( go, newPath + ".prefab", out var success );
				ret = refGo.GetComponent<T>();
			}
		}
		finally
		{
			GameObject.DestroyImmediate( go );
		}

		return ret;
	}

	public static void CheckEndHorizontal( float cw, ref float aw )
	{
		if( aw + cw > Screen.width )
		{
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			aw = 0;
		}
		aw += cw;
	}

	public static void Semaphore( IValueStateObserver<bool> s, string name )
	{
		Semaphore( s != null ? s.Value : false, name );
	}

	public static void Semaphore( bool free, string name )
	{
		GUILayout.BeginHorizontal();
		K10.EditorGUIExtention.IconButton.Layout( free ? "on" : "off", 18, free ? 'O' : 'X', "", free ? Color.green : Color.red );
		GUILayout.Label( name, K10GuiStyles.smallStyle );
		// GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	static void Data( string data, string name )
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label( name + ":", K10GuiStyles.smallStyle );
		GUILayout.Label( data, K10GuiStyles.smallStyle );
		GUILayout.EndHorizontal();
	}

	public static bool DrawSemaphoresOnGUI<T, K>( object obj, float sizePerElements = 100 ) where T : class where K : IValueStateObserver<bool>
	{
		var t = obj as T;
		var props = typeof( T ).GetProperties();

		bool initialLabel = false;

		int items = 1;
		int itemsPerRow = (int)( EditorGUIUtility.currentViewWidth / sizePerElements );

		GUILayout.BeginHorizontal();
		for( int i = 0; i < props.Length; i++ )
		{
			var prop = props[i];
			if( prop.PropertyType == typeof( K ) )
			{
				if( !initialLabel )
				{
					initialLabel = true;
					GUILayout.Label( typeof( K ).Name + "(s)", K10GuiStyles.boldStyle );
				}
				var name = prop.Name;

				try 
				{
					var semaphore = (K)prop.GetValue( t, null );
					K10EditorGUIUtils.Semaphore( semaphore, name );
				}
				catch( System.Exception ex )
				{
					GUILayout.BeginHorizontal();
					K10.EditorGUIExtention.IconButton.Layout( "error", 18, 'X', "", Color.red );
					GUILayout.Label( $"{name}({ex.Message})", K10GuiStyles.smallStyle );
					GUILayout.EndHorizontal();
				}
				items++;

				if( items >= itemsPerRow )
				{
					items = 0;
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		return initialLabel;
	}

	public static bool DrawValueObserverOnGUI<T, K>( object obj, float sizePerElements = 100 ) where T : class where K : struct
	{
		var t = obj as T;
		var props = typeof( T ).GetProperties();

		bool initialLabel = false;

		int items = 1;
		int itemsPerRow = (int)( EditorGUIUtility.currentViewWidth / sizePerElements );

		GUILayout.BeginHorizontal();
		for( int i = 0; i < props.Length; i++ )
		{
			var prop = props[i];
			if( prop.PropertyType == typeof( IValueStateObserver<K> ) )
			{
				if( !initialLabel )
				{
					initialLabel = true;
					GUILayout.Label( typeof( K ).Name + "(s)", K10GuiStyles.boldStyle );
				}
				var name = prop.Name;
				var data = (IValueStateObserver<K>)prop.GetValue( t, null );

				K10EditorGUIUtils.Data( data.Value.ToStringOrNull(), name );
				items++;

				if( items >= itemsPerRow )
				{
					items = 0;
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		return initialLabel;
	}

	public static bool DrawReactiveProperties<T>( object target ) where T : class
	{
		var drawSomething = false;
		drawSomething |= K10EditorGUIUtils.DrawSemaphoresOnGUI<T, ISemaphore>( target );
		drawSomething |= K10EditorGUIUtils.DrawSemaphoresOnGUI<T, IBoolStateObserver>( target, 120 );
		drawSomething |= K10EditorGUIUtils.DrawSemaphoresOnGUI<T, IStateRequester>( target, 120 );
		drawSomething |= K10EditorGUIUtils.DrawValueObserverOnGUI<T, int>( target );
		drawSomething |= K10EditorGUIUtils.DrawValueObserverOnGUI<T, uint>( target );
		drawSomething |= K10EditorGUIUtils.DrawValueObserverOnGUI<T, float>( target );
		drawSomething |= K10EditorGUIUtils.DrawValueObserverOnGUI<T, Vector2>( target );
		drawSomething |= K10EditorGUIUtils.DrawValueObserverOnGUI<T, Rect>( target, 300 );
		drawSomething |= K10EditorGUIUtils.DrawValueObserverOnGUI<T, Ray>( target, 300 );
		return drawSomething;
	}
}