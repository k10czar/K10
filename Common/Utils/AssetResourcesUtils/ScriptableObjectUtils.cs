using UnityEngine;

using static Colors.Console;

public static class ScriptableObjectUtils
{
    const string RESOURCES_DEFAULT_PATH = "Resources/";

	public static T RequestResource<T>( string resourcePath ) where T : ScriptableObject
	{
        var t = Resources.Load<T>( resourcePath );
        if( t != null ) return t;

		t = Create<T>( RESOURCES_DEFAULT_PATH + resourcePath );
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
		
		SaveAssets();
		return assetRef;
	}

	public static void SaveAssets()
	{
#if UNITY_EDITOR
		UnityEditor.AssetDatabase.SaveAssets();
		UnityEditor.AssetDatabase.Refresh();
#endif
	}


	public static void FocusObject( UnityEngine.Object assetToFocus )
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.FocusProjectWindow();
		UnityEditor.Selection.activeObject = assetToFocus;
#endif
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
		throw new System.NotImplementedException();
	}

	public static void CreationObjectAndFile( System.Type type, string newPath, bool focus, System.Action<ScriptableObject> OnObjectCreated )
	{
		var assetRef = CreationObjectAndFile( type, newPath, focus );
		if (OnObjectCreated != null) OnObjectCreated( assetRef );
	}

	public static ScriptableObject CreationObjectAndFile( System.Type type, string newPath, bool focus )
	{
		ScriptableObject asset = ScriptableObject.CreateInstance( type );

		if( FileAdapter.Exists( newPath + ".asset") )
		{
			int id = 2;
			while( FileAdapter.Exists( newPath + id + ".asset" ) ) id++;
			newPath = newPath + id;
		}

		return SetSO( ref newPath, focus, asset );
	}

	public static ScriptableObject Create( string newPath, System.Type type, bool focus = false, bool saveAndRefresh = true )
	{
		ScriptableObject asset = ScriptableObject.CreateInstance(type);
		var so = SetSO( ref newPath, focus, asset, saveAndRefresh );
        Debug.Log( $"🏗 {"Created".Colorfy( Verbs )} new {type.FullName.Colorfy( TypeName )} at {newPath.Colorfy( Keyword )}" );
		return so;
	}

	public static T Create<T>( string newPath, bool focus = false, bool saveAndRefresh = true ) where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T>();
		var so = SetSO( ref newPath, focus, asset, saveAndRefresh );
        Debug.Log( $"🏗 {"Created".Colorfy( Verbs )} new {typeof(T).FullName.Colorfy( TypeName )} at {newPath.Colorfy( Keyword )}" );
		return so;
	}

	public static T CreateSibiling<T>( UnityEngine.Object sibiling, string name, bool focus = false, bool saveAndRefresh = true ) where T : ScriptableObject
    {
#if UNITY_EDITOR
        var modelPath = UnityEditor.AssetDatabase.GetAssetPath( sibiling );
        var path = modelPath.Remove( modelPath.Length - ( ".asset".Length + sibiling.name.Length )  ) + name;
		var newSO = ScriptableObjectUtils.Create<T>( path, focus, saveAndRefresh );
        return newSO;
#else //UNITY_EDITOR
		Debug.Log( $"CreateSibiling<{typeof(T).FullName}>( {sibiling.NameOrNull()}, {name} ) {ConsoleMessage.ONLY_EDITOR}" );
		return null;
#endif //UNITY_EDITOR
    }

	private static T SetInsideSO<T>( string rootFilePath, string insideFile, bool focus, T asset, bool saveAndRefresh = true) where T : ScriptableObject
	{
#if UNITY_EDITOR
		if( !rootFilePath.StartsWith( "Assets/" ) && !rootFilePath.StartsWith( "Assets\\" ) ) rootFilePath = "Assets/" + rootFilePath;

		var rootFile = UnityEditor.AssetDatabase.LoadAssetAtPath( rootFilePath, typeof(ScriptableObject) );
		if( rootFile == null )
		{
			Debug.LogError( "Cannot load root file:" + rootFilePath + " = " + rootFile.NameOrNull() + ".asset" );
			return null;
		}

		UnityEditor.AssetDatabase.AddObjectToAsset( asset, rootFile );

		if( saveAndRefresh ) SaveAssets();
		if( focus ) FocusObject( asset );
#else //UNITY_EDITOR
		Debug.Log( $"SetInsideSO<{typeof(T).FullName}>( {rootFilePath} ) {ConsoleMessage.ONLY_EDITOR}" );
#endif //UNITY_EDITOR

		return asset;
	}

	private static T SetSO<T>( ref string newPath, bool focus, T asset, bool saveAndRefresh = true ) where T : ScriptableObject
	{
#if UNITY_EDITOR
		if( !newPath.StartsWith( "Assets/" ) && !newPath.StartsWith( "Assets\\" ) ) newPath = "Assets/" + newPath;
		newPath = newPath.SanitizePathName();
		AssetDatabaseUtils.RequestPath( newPath );
		string assetPathAndName = UnityEditor.AssetDatabase.GenerateUniqueAssetPath( newPath + ".asset" );
		Debug.Log( $"{"Try".Colorfy( Verbs )} create({typeof(T).FullName.Colorfy( TypeName )}) asset at {assetPathAndName.Colorfy(Interfaces)} from {newPath.Colorfy(Names)}.asset" );
		UnityEditor.AssetDatabase.CreateAsset( asset, assetPathAndName );
		if( saveAndRefresh ) SaveAssets();
		if( focus ) FocusObject( asset );
#else //UNITY_EDITOR
		Debug.Log( $"SetSO<{typeof(T).FullName}>( ref {newPath} ) {ConsoleMessage.ONLY_EDITOR}" );
#endif //UNITY_EDITOR
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
