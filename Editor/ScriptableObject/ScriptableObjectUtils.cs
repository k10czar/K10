using System.Linq;
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


	static void CreationObjectAndFile( object type, string newPath, bool focus, System.Action<ScriptableObject> OnObjectCreated = null )
	{
		var rt = (System.Type)type;
		ScriptableObject asset = ScriptableObject.CreateInstance(rt);

		if( FileAdapter.Exists( newPath + ".asset") )
		{
			int id = 2;
			while( FileAdapter.Exists( newPath + id + ".asset" ) ) id++;
			newPath = newPath + id;
		}

		var assetRef = SetSO( ref newPath, focus, asset );
		if (OnObjectCreated != null) OnObjectCreated(assetRef);
	}

	public static void CreateMenu( string newPath, System.Type type, bool focus = false, System.Action<ScriptableObject> OnObjectCreated = null )
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
				GenericMenu.MenuFunction2 onTypedElementCreated = (tp) => CreationObjectAndFile( tp, newPath + "_" + tp.ToStringOrNull(), focus, OnObjectCreated );
				menu.AddItem( new GUIContent( t.ToStringOrNull() ), false, onTypedElementCreated, t );
			}

			menu.ShowAsContext();

			Debug.LogError(type.ToStringOrNull() + " is a abstract ScriptableObject click again the button holding some of the following keys to choose some of the inherited type:\n" + string.Join("\n", types ) + "\n\n");
			return;
		}

		CreationObjectAndFile( selectedType, newPath, focus, OnObjectCreated );
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

	private static T SetSO<T>( ref string newPath, bool focus, T asset ) where T : ScriptableObject
	{
		if( !newPath.StartsWith( "Assets/" ) && !newPath.StartsWith( "Assets\\" ) ) newPath = "Assets/" + newPath;
		AssetDatabaseUtils.RequestPath( newPath );
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( newPath + ".asset" );

		Debug.Log( "Try create(" + asset.ToStringOrNull() + ") asset at " + assetPathAndName );

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
