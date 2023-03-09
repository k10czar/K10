using UnityEngine;

public class GuaranteedSO<T> where T : ScriptableObject, new()
{
	private T _reference;
	readonly string _path;
	public T Reference
	{
		get
		{
			if( _reference == null ) RequestSO( ref _reference, _path );
			return _reference;
		}
	}

	public GuaranteedSO( string path ) { _path = path; }

	static T RequestSO( ref T field, string collectionName )
	{
		if( field == null ) field = Resources.Load<T>( collectionName );
#if UNITY_EDITOR
		if( field == null && !Application.isPlaying ) field = RequestResource( collectionName );
#endif
		if( field == null ) field = new T();
		return field;
	}

#if UNITY_EDITOR
	const string RESOURCES_DEFAULT_PATH = "_FIRST/Resources/";
	static T RequestResource( string resourcePath )
	{
		var t = Resources.Load<T>( resourcePath );
		if( t != null ) return t;

		var firstImport = FileAdapter.Exists( $"{Application.dataPath}/{RESOURCES_DEFAULT_PATH}{resourcePath}.asset" );
		if( firstImport ) return null;

		var create = UnityEditor.EditorUtility.DisplayDialog( "Constant not Found", $"Did not find asset at {resourcePath}, do you want to create new one?", "Sure", "No" );
		if( !create ) return null;

		t = Create( RESOURCES_DEFAULT_PATH + resourcePath );
		Debug.LogWarning( "Created new Resource at " + RESOURCES_DEFAULT_PATH + resourcePath );
		return t;
	}

	static T Create( string newPath, bool focus = false )
	{
		if( !newPath.StartsWith( "Assets/" ) ) newPath = "Assets/" + newPath;
		var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>( newPath );
		if( asset != null ) return asset;

		asset = ScriptableObject.CreateInstance<T>();

		int firstBar = newPath.Length - 1;
		for (; firstBar >= 0 && newPath[ firstBar ] != '/'; firstBar--) { }
		if( firstBar >= 0 ) FileAdapter.RequestDirectory( newPath.Substring( 0, firstBar ) );

		string assetPathAndName = UnityEditor.AssetDatabase.GenerateUniqueAssetPath( newPath + ".asset" );

		Debug.Log( $"Try create asset at {newPath} and created at {assetPathAndName}" );

		UnityEditor.AssetDatabase.CreateAsset( asset, assetPathAndName );

		UnityEditor.AssetDatabase.SaveAssets();
		UnityEditor.AssetDatabase.Refresh();

		if( focus )
		{
			UnityEditor.EditorUtility.FocusProjectWindow();
			UnityEditor.Selection.activeObject = asset;
		}

		return asset;
	}
#endif
}