using System;
using K10;
using UnityEngine;

public class GuaranteedSO<T> : IReferenceOf<T> where T : ScriptableObject, new()
{
	private T _reference;
	readonly string _path;
	public T Reference
	{
		get
		{
			if (_reference == null) //RequestSO(ref _reference, _path);
			{
				if (_reference == null)
				{
					var metricsCode = $"Resources.Load( <color=cyan>{_path}</color> )";
#if CODE_METRICS
					CodeMetrics.Start( metricsCode );
#endif
					_reference = Resources.Load<T>(_path);
#if CODE_METRICS
					CodeMetrics.Finish( metricsCode );
#endif
				}
#if UNITY_EDITOR
				if (_reference == null && !Application.isPlaying)
				{
					// _reference = RequestResource(_path);

					_reference = Resources.Load<T>(_path);
					if (_reference != null) return _reference;

					var alreadyExists = false;
					var resourcesFolders = ResourcesFolderFinder.GetAllResourcesFolders();
					for (int i = 0; i < resourcesFolders.Count && !alreadyExists; i++)
					{
						string folder = resourcesFolders[i];
						alreadyExists |= FileAdapter.Exists($"{Application.dataPath}/{folder}/{_path}.asset");
					}

					if (!alreadyExists)
					{
						var stack = StackTraceUtility.ExtractStackTrace();
						if (stack.Length > 1000) stack = stack.Substring(0, 1000);
						var create = UnityEditor.EditorUtility.DisplayDialog("Constant not Found", $"Did not find asset at {_path}, do you want to create new one?\n\n{stack}\n", "Sure", "No");
						if (create)
						{
							_reference = Create(RESOURCES_DEFAULT_PATH + _path);
							Debug.LogWarning("Created new Resource at " + RESOURCES_DEFAULT_PATH + _path);
						}
					}
				}
#endif
				if (_reference == null) _reference = new T();
			}
			return _reference;
		}
	}

	public GuaranteedSO(string path) { _path = path; }

#if UNITY_EDITOR
	const string RESOURCES_DEFAULT_PATH = "_FIRST/Resources/";

	static T Create(string newPath, bool focus = false)
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