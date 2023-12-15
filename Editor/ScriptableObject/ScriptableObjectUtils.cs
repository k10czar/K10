using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using UnityEditor;
using UnityEngine;

public static partial class ScriptableObjectUtils
{
    const string RESOURCES_DEFAULT_PATH = "Resources/";

	public static T RequestResource<T>( string resourcePath ) where T : ScriptableObject
	{
        var t = Resources.Load<T>( resourcePath );
        if( t != null ) return t;

		t = ScriptableObjectUtils.Create<T>( RESOURCES_DEFAULT_PATH + resourcePath );
		Debug.LogWarning( "Created new Resource at " + RESOURCES_DEFAULT_PATH  + resourcePath );
        return t;
	}


	public static ScriptableObject CreationObjectInsideAssetFile( System.Type type, string rootFilePath, string newInsideFileName, bool focus, System.Action<ScriptableObject> OnObjectCreated = null )
	{
		// if( OnObjectCreated != null ) OnObjectCreated( null );
		// return null;
		ScriptableObject asset = ScriptableObject.CreateInstance( type );
		asset.name = newInsideFileName;

		var assetRef = SetInsideSO( rootFilePath, newInsideFileName, focus, asset, false );
		if( OnObjectCreated != null ) OnObjectCreated( assetRef );
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		return assetRef;
	}


	public static T CreationObjectInsideAssetFile<T>( string rootFilePath, string newInsideFileName, bool focus ) where T : ScriptableObject
	{
		var asset = ScriptableObject.CreateInstance<T>();
		asset.name = newInsideFileName;

		var assetRef = SetInsideSO<T>( rootFilePath, newInsideFileName, focus, asset );
		return assetRef;
	}

	public static void CreationObjectInsideAssetFile( string selectedAssetPath, string v )
	{
		throw new NotImplementedException();
	}

	static void CreationObjectAndFile( System.Type type, string newPath, bool focus, System.Action<ScriptableObject> OnObjectCreated = null )
	{
		ScriptableObject asset = ScriptableObject.CreateInstance( type );

		if( FileAdapter.Exists( newPath + ".asset") )
		{
			int id = 2;
			while( FileAdapter.Exists( newPath + id + ".asset" ) ) id++;
			newPath = newPath + id;
		}

		var assetRef = SetSO( ref newPath, focus, asset );
		if (OnObjectCreated != null) OnObjectCreated(assetRef);
	}

	public static void CreateInsideMenu( string rootAssetPath, SerializedProperty prop, System.Type type, bool focus = false, System.Action<ScriptableObject> OnObjectCreated = null, string name = null )
	{
		System.Type selectedType = null;

		var types = System.AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(s => s.GetTypes())
					.Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);

		var count = types.Count();
		if (count <= 1)
		{
			selectedType = type;
			foreach (var t in types) selectedType = t;
			Debug.Log( selectedType.ToStringOrNull() + " is the only non Abstract type that implements " + type + ". So dont need to show menu" );
			
			Debug.Log( $"{rootAssetPath} + {name ?? (prop.PropPathParsed() + " + _ + " + selectedType.ToStringOrNull())}" );
		}
		else
		{
			GenericMenu menu = new GenericMenu();
			Debug.Log( selectedType.ToStringOrNull() + " is the only non Abstract type that implements " + type + ". So dont need to show menu" );

			foreach (var t in types)
			{
				var pathAtt = t.GetCustomAttribute<CreationPathAttribute>();
				var tParsed = ( pathAtt != null ? pathAtt.Path : t.ToStringOrNull() ).Replace( ".", "/" );

				var objName = name ?? (prop.PropPathParsed() + " + _ + " + t.ToStringOrNull());
				Debug.Log( $"{rootAssetPath} + {objName}" );

				GenericMenu.MenuFunction2 onTypedElementCreatedInside = ( tp ) => CreationObjectInsideAssetFile( (System.Type)tp, rootAssetPath, objName, focus, OnObjectCreated );
				menu.AddItem( new GUIContent( tParsed ), false, onTypedElementCreatedInside, t );
			}

			menu.ShowAsContext();

			Debug.Log(type.ToStringOrNull() + " is a abstract ScriptableObject click again the button holding some of the following keys to choose some of the inherited type:\n" + string.Join("\n", types ) + "\n\n");
			return;
		}

		CreationObjectInsideAssetFile( selectedType, rootAssetPath, name ?? (prop.PropPathParsed() + " + _ + " + selectedType.ToStringOrNull()), focus, OnObjectCreated );
	}

	public static void CreateMenu( string rootAssetPath, SerializedProperty prop, System.Type type, bool focus = false, System.Action<ScriptableObject> OnObjectCreated = null )
	{
		System.Type selectedType = null;

		var types = System.AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(s => s.GetTypes())
					.Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);

					

		var count = types.Count();
		if (count <= 1)
		{
			selectedType = type;
			foreach (var t in types) selectedType = t;
			Debug.LogError( selectedType.ToStringOrNull() + " is the only non Abstract type that implements " + type + ". So dont need to show menu" );
		}
		else
		{
			GenericMenu menu = new GenericMenu();

			foreach (var t in types)
			{
				var pathAtt = t.GetCustomAttribute<CreationPathAttribute>();
				var tParsed = ( pathAtt != null ? pathAtt.Path : t.ToStringOrNull() ).Replace( ".", "/" );
				
				GenericMenu.MenuFunction2 onTypedElementCreatedOutside = ( tp ) =>
				{
					var rootFolder = System.IO.Path.GetDirectoryName( rootAssetPath );
					var newFolderName = System.IO.Path.GetFileNameWithoutExtension( rootAssetPath );
					CreationObjectAndFile( (System.Type)tp, rootFolder + "\\" + prop.ToFileName() + "_" + tp.ToStringOrNull(), focus, OnObjectCreated );
				};

				menu.AddItem( new GUIContent( tParsed + "/Separated File" ), false, onTypedElementCreatedOutside, t );

				GenericMenu.MenuFunction2 onTypedElementCreatedOutsideInFolder = ( tp ) =>
				{
					var rootFolder = System.IO.Path.GetDirectoryName( rootAssetPath );
					var newFolderName = System.IO.Path.GetFileNameWithoutExtension( rootAssetPath );
					CreationObjectAndFile( (System.Type)tp, rootFolder + "\\" + newFolderName + "\\" + prop.ToFileName() + "_" + tp.ToStringOrNull(), focus, OnObjectCreated );
				};

				menu.AddItem( new GUIContent( tParsed + "/Separated File in Folder" ), false, onTypedElementCreatedOutsideInFolder, t );

				GenericMenu.MenuFunction2 onTypedElementCreatedInside = ( tp ) => CreationObjectInsideAssetFile( (System.Type)tp, rootAssetPath, prop.PropPathParsed() + "_" + tp.ToStringOrNull(), focus, OnObjectCreated );
				menu.AddItem( new GUIContent( tParsed + "/Nested inside this SO" ), false, onTypedElementCreatedInside, t );
			}

			menu.ShowAsContext();

			Debug.Log(type.ToStringOrNull() + " is a abstract ScriptableObject click again the button holding some of the following keys to choose some of the inherited type:\n" + string.Join("\n", types ) + "\n\n");
			return;
		}

		CreationObjectAndFile( selectedType, rootAssetPath, focus, OnObjectCreated );
	}

	public static ScriptableObject Create(string newPath, System.Type type, bool focus = false)
	{
		ScriptableObject asset = ScriptableObject.CreateInstance(type);
		return SetSO(ref newPath, focus, asset);
	}

	public static T Create<T>( string newPath, bool focus = false ) where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T>();
		return SetSO( ref newPath, focus, asset );
	}


	private static T SetInsideSO<T>( string rootFilePath, string insideFile, bool focus, T asset, bool saveAndRefresh = true ) where T : ScriptableObject
	{
		if( !rootFilePath.StartsWith( "Assets/" ) && !rootFilePath.StartsWith( "Assets\\" ) ) rootFilePath = "Assets/" + rootFilePath;

		var rootFile = AssetDatabase.LoadAssetAtPath( rootFilePath, typeof(ScriptableObject) );
		if( rootFile == null )
		{
			Debug.LogError( "Cannot load root file:" + rootFilePath + " = " + rootFile.NameOrNull() + ".asset" );
			return null;
		}

		AssetDatabase.AddObjectToAsset( asset, rootFile );

		if( saveAndRefresh )
		{
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		if( focus )
		{
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}

		return asset;
	}

	private static T SetSO<T>( ref string newPath, bool focus, T asset ) where T : ScriptableObject
	{
		if( !newPath.StartsWith( "Assets/" ) && !newPath.StartsWith( "Assets\\" ) ) newPath = "Assets/" + newPath;
		AssetDatabaseUtils.RequestPath( newPath );
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( newPath + ".asset" );

		Debug.Log( "Try create(" + asset.ToStringOrNull() + ") asset at " + assetPathAndName + " from " + newPath + ".asset" );

		AssetDatabase.CreateAsset( asset, assetPathAndName );

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		if( focus )
		{
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}

		return asset;
	}

	public static T CreateSequential<T>( string newPrefabPrefix, bool focus = false ) where T : ScriptableObject
	{
		if( !newPrefabPrefix.StartsWith( "Assets/" ) && !newPrefabPrefix.StartsWith( "Assets\\" ) ) newPrefabPrefix = "Assets/" + newPrefabPrefix;
		if( !FileAdapter.Exists( newPrefabPrefix + ".asset" ) ) return Create<T>( newPrefabPrefix, focus );

		int id = 2;
		while( FileAdapter.Exists( newPrefabPrefix + id + ".asset" ) ) id++;

		return Create<T>( newPrefabPrefix + id, focus );
	}

	public static ScriptableObject CreateSequential( string newPrefabPrefix, System.Type type, bool focus = false )
	{
		if( !newPrefabPrefix.StartsWith( "Assets/" ) && !newPrefabPrefix.StartsWith( "Assets\\" ) ) newPrefabPrefix = "Assets/" + newPrefabPrefix;
		if( !FileAdapter.Exists( newPrefabPrefix + ".asset" ) ) return Create( newPrefabPrefix, type, focus );

		int id = 2;
		while( FileAdapter.Exists( newPrefabPrefix + id + ".asset" ) ) id++;

		return Create( newPrefabPrefix + id, type, focus );
	}
}
