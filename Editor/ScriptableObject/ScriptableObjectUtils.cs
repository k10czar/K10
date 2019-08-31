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

    public static T Create<T>( string newPath, bool focus = false ) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        if( !newPath.StartsWith( "Assets/" ) ) newPath = "Assets/" + newPath;
        AssetDatabaseUtils.RequestPath( newPath );
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( newPath + ".asset" );

        Debug.Log( "Try create asset at " + assetPathAndName );

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
        if( !newPrefabPrefix.StartsWith( "Assets/" ) ) newPrefabPrefix = "Assets/" + newPrefabPrefix;
        if( !FileAdapter.Exists( newPrefabPrefix + ".asset" ) ) return Create<T>( newPrefabPrefix, focus );

        int id = 2;
        while( FileAdapter.Exists( newPrefabPrefix + id + ".asset" ) ) id++;

        return Create<T>( newPrefabPrefix + id, focus );
    }
}
