using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using UnityEngine.Rendering.VirtualTexturing;

public sealed class EditorAssetValidationProcessWindow : EditorWindow
{
	static EditorAssetValidationProcessWindow _instance;
	public static EditorAssetValidationProcessWindow Instance
	{
		get
		{
			if (_instance == null) _instance = GetWindow<EditorAssetValidationProcessWindow>("Editor Asset Validation Processes");
			return _instance;
		}
	}

	List<Type> _soTypes = new List<Type>();
	List<int> _soTypesCount = new List<int>();
	HashSet<int> _soSelectionIgnore = new HashSet<int>();

	List<Type> _mbTypes = new List<Type>();
	List<int> _mbTypesCount = new List<int>();
	List<int> _mbPrefabsCount = new List<int>();
	HashSet<int> _mbSelectionIgnore = new HashSet<int>();

	[MenuItem("K10/Editor Asset Validation Process")] static void Init() { var i = Instance; }
	void OnEnable()
	{
		ScanTypes();
	}

	void ScanTypes()
	{
		_soTypes.Clear();
		_mbTypes.Clear();
		Type interfaceType = typeof(IEditorAssetValidationProcess);
		Type soType = typeof(ScriptableObject);
		Type mbType = typeof(MonoBehaviour);
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach( Assembly assembly in assemblies )
		{
			Type[] types = assembly.GetTypes();
			var allSoClasses = types.Where( type => interfaceType.IsAssignableFrom( type ) && type.IsSubclassOf( soType ) && type.IsClass && !type.IsAbstract ).ToList();
			for( int j = 0; j < allSoClasses.Count; j++ )
			{
				var typeClass = allSoClasses[j];
				_soTypes.Add( typeClass );
			}
			var allMbClasses = types.Where( type => interfaceType.IsAssignableFrom( type ) && type.IsSubclassOf( mbType ) && type.IsClass && !type.IsAbstract ).ToList();
			for( int j = 0; j < allMbClasses.Count; j++ )
			{
				var typeClass = allMbClasses[j];
				_mbTypes.Add( typeClass );
			}
		}
	}

	void CountSoTypes()
	{
		_soTypesCount.Clear();
		var totalSO = 0;
		var sb = new StringBuilder();
		for( int j = 0; j < _soTypes.Count; j++ )
		{
			var typeClass = _soTypes[j];
			var transferables = AssetDatabaseUtils.GetAll( typeClass );
			_soTypesCount.Add( transferables.Length );
			sb.AppendLine( $"\t-<color=lime>{_soTypes[j]}</color> as <color=orange>{transferables.Length}</color> objects" );
			totalSO += transferables.Length;
		}
        UnityEngine.Debug.Log( $"Counted {totalSO} {nameof(IEditorAssetValidationProcess)} valid ScriptableObject\n{sb.ToString()}" );
	}

	void CountMbTypes()
	{
		_mbTypesCount.Clear();
		_mbPrefabsCount.Clear();

		var components = 0;
		var prefabs = 0;
		var totalPrefabs = 0;
		
		for( int j = 0; j < _mbTypes.Count; j++ )
		{
			_mbTypesCount.Add(0);
			_mbPrefabsCount.Add(0);
		}

		string[] guids = AssetDatabase.FindAssets("t:Prefab");
		totalPrefabs += guids.Length;
		
		foreach( string guid in guids )
		{
			string assetPath = AssetDatabase.GUIDToAssetPath( guid );
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>( assetPath );
			if( prefab == null ) continue;

			for( int j = 0; j < _mbTypes.Count; j++ )
			{
				if( _mbSelectionIgnore.Contains( j ) ) continue;
				var typeClass = _mbTypes[j];
				var refs = prefab.GetComponentsInChildren( typeClass );
				if( refs == null || refs.Length == 0 ) continue;
				_mbPrefabsCount[j]++;
				prefabs++;
				for( int i = 0; i < refs.Length; i++ )
				{
					_mbTypesCount[j]++;
					components++;
				}
			}
		}
		
		var sb = new StringBuilder();
		for( int j = 0; j < _mbTypes.Count; j++ )
		{
			sb.AppendLine( $"\t-<color=lime>{_mbTypes[j]}</color> as <color=orange>{_mbTypesCount[j]}</color> components in <color=orange>{_mbPrefabsCount[j]}</color>" );
		}
        UnityEngine.Debug.Log( $"Counted {components} components in {prefabs} prefabs of total of {totalPrefabs} prefabs of the project\n{sb.ToString()}" );
	}

	private string NumToString( int num )
	{
		if( num < 0 ) return "?";
		return num.ToString();
	}
	
	private void OnGUI()
	{
		EditorGUILayout.BeginHorizontal();
		if( GUILayout.Button( $"Rescan Types" ) )
		{
			ScanTypes();
		}
		if( GUILayout.Button( $"Count Types" ) )
		{
			CountSoTypes();
			CountMbTypes();
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical();
		if( GUILayout.Button( $"Select {((_soSelectionIgnore.Count == 0)?"None":"All")} Types" ) )
		{
			if( _soSelectionIgnore.Count == 0 )
			{
				for( int j = 0; j < _soTypes.Count; j++ )
				{
					_soSelectionIgnore.Add( j );
				}
			}
			else
			{
				_soSelectionIgnore.Clear();
			}
		}
		for( int i = 0; i < _soTypes.Count; i++ )
		{
			Type type = _soTypes[i];
			var tggl = !_soSelectionIgnore.Contains( i );
			var newTggl = tggl;
			if( i < _soTypesCount.Count ) newTggl = EditorGUILayout.ToggleLeft( $"{type.FullName} ({NumToString(_soTypesCount[i])})", tggl );
			else newTggl = EditorGUILayout.ToggleLeft( type.FullName, tggl );
			if( tggl != newTggl ) 
			{
				if( newTggl ) _soSelectionIgnore.Remove( i );
				else _soSelectionIgnore.Add( i );
			}
		}
		if( GUILayout.Button( $"Process ({_soTypes.Count - _soSelectionIgnore.Count}) ScriptableObject Types" ) )
		{
			var sb = new StringBuilder();
			for( int j = 0; j < _soTypes.Count; j++ )
			{
				if( _soSelectionIgnore.Contains( j ) ) continue;
				var sw = new Stopwatch();
				sw.Start();
				var transfers = 0;
				var typeClass = _soTypes[j];
				var transferables = AssetDatabaseUtils.GetAll( typeClass );
				sb.Clear();
				for( int i = 0; i < transferables.Length; i++ ) 
				{
					var transferable = transferables[i] as IEditorAssetValidationProcess;
					if( transferable == null ) continue;
					var transfered = false;
					try
					{
						transfered = transferable.EDITOR_ExecuteAssetValidationProcess();
					}
					catch( System.Exception ex )
					{
						UnityEngine.Debug.LogError( $"Failed to execute {AssetDatabase.GetAssetPath(transferables[i])}.{nameof(transferable.EDITOR_ExecuteAssetValidationProcess)}():\n{ex}" );
					}
					if( !transfered ) continue;
					EditorUtility.SetDirty( transferables[i] );
					sb.AppendLine( $"\t-{typeClass.Name}[{i}] = {transferables[i].NameOrNull()}" );
					transfers++;
				}
				sw.Stop();
            	UnityEngine.Debug.Log( $"Processed <color=yellow>{transfers}</color> {typeClass.Name} in <color=orange>{sw.Elapsed.TotalMilliseconds}</color>ms:\n{sb.ToString()}" );
			}
		}
		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical();
		if( GUILayout.Button( $"Select {((_mbSelectionIgnore.Count == 0)?"None":"All")} Types" ) )
		{
			if( _mbSelectionIgnore.Count == 0 )
			{
				for( int j = 0; j < _mbTypes.Count; j++ )
				{
					_mbSelectionIgnore.Add( j );
				}
			}
			else
			{
				_mbSelectionIgnore.Clear();
			}
		}
		for( int i = 0; i < _mbTypes.Count; i++ )
		{
			Type type = _mbTypes[i];
			var tggl = !_mbSelectionIgnore.Contains( i );
			var newTggl = tggl;
			if( i < _mbTypesCount.Count && i < _mbPrefabsCount.Count ) newTggl = EditorGUILayout.ToggleLeft( $"{type.FullName} ({NumToString(_mbTypesCount[i])}/{NumToString(_mbPrefabsCount[i])})", tggl );
			else newTggl = EditorGUILayout.ToggleLeft( type.FullName, tggl );
			if( tggl != newTggl ) 
			{
				if( newTggl ) _mbSelectionIgnore.Remove( i );
				else _mbSelectionIgnore.Add( i );
			}
		}
		if( GUILayout.Button( $"Process ({_mbTypes.Count - _mbSelectionIgnore.Count}) MonoBehaviour Types" ) )
		{
			var sw = new Stopwatch();
			sw.Start();
			var sb = new StringBuilder();
			var transfers = 0;
			var objects = 0;
			var components = 0;
			string[] guids = AssetDatabase.FindAssets("t:Prefab");

			for( int j = _mbPrefabsCount.Count; j < _mbTypes.Count; j++ ) _mbPrefabsCount.Add( int.MinValue );
			for( int j = _mbTypesCount.Count; j < _mbTypes.Count; j++ ) _mbTypesCount.Add( int.MinValue );
			
			for( int j = 0; j < _mbTypes.Count; j++ )
			{
				if( _mbSelectionIgnore.Contains( j ) ) continue;
				_mbPrefabsCount[j] = 0;
				_mbTypesCount[j] = 0;
			}
			
			foreach( string guid in guids )
			{
				string assetPath = AssetDatabase.GUIDToAssetPath( guid );
				try
				{
					GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>( assetPath );
					if( prefab == null ) continue;

					var modifiedPrefab = false;
					for( int j = 0; j < _mbTypes.Count; j++ )
					{
						if( _mbSelectionIgnore.Contains( j ) ) continue;
						var typeClass = _mbTypes[j];
						var refs = prefab.GetComponentsInChildren( typeClass );
						if( refs == null || refs.Length == 0 ) continue;
						objects++;
						_mbPrefabsCount[j]++;
						for( int i = 0; i < refs.Length; i++ )
						{
							var component = refs[i];
							_mbTypesCount[j]++;
							components++;
							var transferable = component as IEditorAssetValidationProcess;
							if( transferable == null ) continue;
							var transfered = transferable.EDITOR_ExecuteAssetValidationProcess();
							if( !transfered ) continue;
							modifiedPrefab = true;
							EditorUtility.SetDirty( refs[i] );
							sb.AppendLine( $"\t-{typeClass.Name}[{i}] = {refs[i].NameOrNull()}" );
							transfers++;
						}
					}
					
					if( !modifiedPrefab ) continue;

					EditorUtility.SetDirty( prefab );
					PrefabUtility.SavePrefabAsset( prefab, out var savedSuccessfully );
					sb.AppendLine( $"{prefab.name} @ {assetPath} {(savedSuccessfully?"<color=lime>Successfully":"<color=red>Failed")}</color>" );
				}
				catch( System.Exception ex )
				{
					UnityEngine.Debug.LogError( $"{assetPath} <color=red>Failed to Load</color>" );
					sb.AppendLine( $"{assetPath} <color=red>Failed to Load</color>" );
				}
			}
			sw.Stop();
			
			if( transfers > 0 )
			{
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

            UnityEngine.Debug.Log( $"Processed <color=yellow>{transfers}</color> on <color=lime>{components}</color> components in <color=green>{objects}</color> valid objects of <color=red>{guids.Length}</color> total in <color=orange>{sw.Elapsed.TotalMilliseconds}</color>ms:\n{sb.ToString()}" );
		}
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}
}