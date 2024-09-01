using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using UnityEngine.Rendering.VirtualTexturing;

using static Colors.Console;
using K10.EditorGUIExtention;

public enum EProcessType
{
	Project = 1,
	Scene = 2,
}

public sealed class EditorAssetProcessingWindow : EditorWindow
{
	static EditorAssetProcessingWindow _instance;
	public static EditorAssetProcessingWindow Instance
	{
		get
		{
			if (_instance == null) _instance = GetWindow<EditorAssetProcessingWindow>("Editor Asset Processing");
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

	EProcessType _searchMonoBehavioursProcess = EProcessType.Project | EProcessType.Scene;

	[MenuItem("K10/Editors/Asset Processing")] static void Init() { var i = Instance; }
	void OnEnable()
	{
		ScanTypes();
	}

	void ScanTypes()
	{
		_soTypes.Clear();
		_mbTypes.Clear();
		Type interfaceType = typeof(IEditorAssetProcessingProcess);
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
        UnityEngine.Debug.Log( $"Counted {totalSO} {nameof(IEditorAssetProcessingProcess)} valid ScriptableObject\n{sb.ToString()}" );
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
			if( TypeToggle( type, ref tggl, ( i < _soTypesCount.Count ) ? $"{NumToString(_soTypesCount[i])}" : null ) )
			{
				if( tggl ) _soSelectionIgnore.Remove( i );
				else _soSelectionIgnore.Add( i );
			}
		}
		if( GUILayout.Button( $"Process ({_soTypes.Count - _soSelectionIgnore.Count}) ScriptableObject Types" ) )
		{
			var sb = ObjectPool<StringBuilder>.Request();
			var noTypes = new List<(Type type,double duration, int objectsCount)>();
			var noTypesDuration = 0.0;
			for( int j = 0; j < _soTypes.Count; j++ )
            {
                if (_soSelectionIgnore.Contains(j)) continue;
                var typeClass = _soTypes[j];
                var validationResult = RunAssetValidationInAll( typeClass, _searchMonoBehavioursProcess, sb );
				if( validationResult.transfers == 0 ) 
				{
					noTypes.Add( ( typeClass, validationResult.duration, validationResult.objectsCount ) );
					noTypesDuration += validationResult.duration;
				}
            }
			
			sb.AppendLine( $"{noTypes.Count.ToStringColored( Numbers )} types were {"not processed".ToStringColored( LightDanger )} and took {$"{noTypesDuration}ms".ToStringColored( Negation )}:" );
			for( int j = 0; j < noTypes.Count; j++ )
			{
				var data = noTypes[j];
				sb.AppendLine( $"  -{data.objectsCount.ToStringColored( Numbers )} {data.type.Name.Colorfy( TypeName )} took {$"{data.duration}ms".ToStringColored( Negation )}" );
			}
			UnityEngine.Debug.Log( sb );
			ObjectPool<StringBuilder>.Return( sb );
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
            var tggl = !_mbSelectionIgnore.Contains(i);
            if ( TypeToggle( type, ref tggl, (i < _mbTypesCount.Count && i < _mbPrefabsCount.Count) ? $"{NumToString(_mbTypesCount[i])}/{NumToString(_mbPrefabsCount[i])}" : null ) )
            {
                if (tggl) _mbSelectionIgnore.Remove(i);
                else _mbSelectionIgnore.Add(i);
            }
        }

        EditorGUILayout.BeginHorizontal();
		if( GUILayout.Button( $"Process ({_mbTypes.Count - _mbSelectionIgnore.Count}) MonoBehaviour Types" ) ) ProcessMonoBehaviours( _searchMonoBehavioursProcess );
        _searchMonoBehavioursProcess = (EProcessType)EditorGUILayout.EnumFlagsField( GUIContent.none, _searchMonoBehavioursProcess, GUILayout.MaxWidth( 80 ) );
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}

    private bool TypeToggle(Type type, ref bool tggl, string details)
    {
		var initialTggl = tggl;
        EditorGUILayout.BeginHorizontal();
        if ( string.IsNullOrEmpty( details ) ) tggl = EditorGUILayout.ToggleLeft(type.FullName, tggl);
        else tggl = EditorGUILayout.ToggleLeft($"{type.FullName} ({details})", tggl);
        var script = type?.EditorGetScript() ?? null;
        if (script != null && IconButton.Layout("script", 's', Colors.Celeste)) AssetDatabase.OpenAsset(script);
        EditorGUILayout.EndHorizontal();
		return initialTggl != tggl;
    }

    private void ProcessMonoBehaviours(EProcessType searchProcess)
    {
        var sw = new Stopwatch();
        sw.Start();
		var sb = ObjectPool<StringBuilder>.Request();
		var sb2 = ObjectPool<StringBuilder>.Request();
        var transfers = 0;
        var objects = 0;
        var total = 0;
        var components = 0;


        if ((searchProcess & EProcessType.Project) != 0)
        {
			sb.AppendLine($"In Project:");
			var psw = new Stopwatch();
			psw.Start();
            string[] guids = AssetDatabase.FindAssets("t:Prefab");

            for (int j = _mbPrefabsCount.Count; j < _mbTypes.Count; j++) _mbPrefabsCount.Add(int.MinValue);
            for (int j = _mbTypesCount.Count; j < _mbTypes.Count; j++) _mbTypesCount.Add(int.MinValue);

            for (int j = 0; j < _mbTypes.Count; j++)
            {
                if (_mbSelectionIgnore.Contains(j)) continue;
                _mbPrefabsCount[j] = 0;
                _mbTypesCount[j] = 0;
            }

			total += guids.Length;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                try
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (prefab == null) continue;

                    var modifiedPrefab = false;
                    for (int j = 0; j < _mbTypes.Count; j++)
                    {
                        if (_mbSelectionIgnore.Contains(j)) continue;
                        var typeClass = _mbTypes[j];
                        var refs = prefab.GetComponentsInChildren(typeClass);
                        if (refs == null || refs.Length == 0) continue;
                        objects++;
                        _mbPrefabsCount[j]++;
                        for (int i = 0; i < refs.Length; i++)
                        {
                            var component = refs[i];
                            _mbTypesCount[j]++;
                            components++;
                            var transferable = component as IEditorAssetProcessingProcess;
                            if (transferable == null) continue;
                            var transfered = transferable.EDITOR_ExecuteAssetValidationProcessOnObject();
                            if (!transfered) continue;
                            modifiedPrefab = true;
                            EditorUtility.SetDirty(component);
                            sb.AppendLine($"\t-{typeClass.Name}[{i}] = {component.NameOrNull()}");
                            transfers++;
                        }
                    }

                    if (!modifiedPrefab) continue;

                    EditorUtility.SetDirty(prefab);
                    PrefabUtility.SavePrefabAsset(prefab, out var savedSuccessfully);
                    sb.AppendLine($"{prefab.name} @ {assetPath} {(savedSuccessfully ? "<color=lime>Successfully" : "<color=red>Failed")}</color>");
                }
                catch (Exception)
                {
                    UnityEngine.Debug.LogError($"{assetPath} <color=red>Failed to Load</color>");
                    sb.AppendLine($"{assetPath} <color=red>Failed execute</color>");
                }
            }
        	psw.Stop();

        }

		if( (searchProcess & EProcessType.Scene) != 0 )
		{
			sb.AppendLine($"In Scene:");
			var ssw = new Stopwatch();
			ssw.Start();
			for (int j = 0; j < _mbTypes.Count; j++)
			{
				if (_mbSelectionIgnore.Contains(j)) continue;
                var typeClass = _mbTypes[j];
				var objs = FindObjectsByType( typeClass, FindObjectsInactive.Include, FindObjectsSortMode.None );
				total += objs.Length;
                for (int i = 0; i < objs.Length; i++)
				{
                    var item = objs[i];
                    var proc = item as IEditorAssetProcessingProcess;
					if( proc == null ) continue;
					var transfered = proc.EDITOR_ExecuteAssetValidationProcessOnObject();
					if (!transfered) continue;
                    EditorUtility.SetDirty(item);
					sb.AppendLine($"\t-{typeClass.Name}[{i}] = {item.NameOrNull()}");
					transfers++;
				}
			}

			ssw.Stop();
			sb2.AppendLine( $"Processed in Scene: took {$"{ssw.Elapsed.TotalMilliseconds}ms".ToStringColored(Negation)}" );
		}

		if (transfers > 0)
		{
			var rsw = new Stopwatch();
			rsw.Start();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
        	rsw.Stop();
		}
        sw.Stop();

        UnityEngine.Debug.Log($"Processed {transfers.ToStringColored(Numbers)} on {components.ToStringColored(TypeName)} components in {objects.ToStringColored(EventName)} valid objects of {total.ToStringColored(Danger)} total in {$"{sw.Elapsed.TotalMilliseconds}ms".ToStringColored(Negation)}:\n{sb2}\n{sb}");
		ObjectPool.Return( sb );
		ObjectPool.Return( sb2 );
    }

    public static (int transfers, double duration, int objectsCount) RunAssetValidationInAll( Type typeClass, EProcessType searchProcess, StringBuilder sb = null )
    {
		if( sb == null ) sb = new StringBuilder();
		var sw = new Stopwatch();
		sw.Start();
		var transfers = 0;
		var transferables = AssetDatabaseUtils.GetAll(typeClass, false);
		var projectTransfers = 0;
		for (int i = 0; i < transferables.Length; i++)
		{
			var transferable = transferables[i] as IEditorAssetProcessingProcess;
			if (transferable == null) continue;
			var transfered = false;
			try
			{
				transfered = transferable.EDITOR_ExecuteAssetValidationProcessOnObject();
			}
			catch (Exception ex)
			{
				var path = AssetDatabase.GetAssetPath(transferables[i]);
				UnityEngine.Debug.LogError($"Failed to execute {path}.EDITOR_ExecuteAssetValidationProcess():\n{ex}");
				sb.AppendLine($"{path} <color=red>Failed execute</color>");
			}
			if (!transfered) continue;
			EditorUtility.SetDirty(transferables[i]);
			sb.AppendLine($"\t-{typeClass.Name}[{i}] = {transferables[i].NameOrNull()}");
			transfers++;
			projectTransfers++;
		}
		sw.Stop();
		var total = transferables.Length;
        if( transfers > 0 ) UnityEngine.Debug.Log($"{"Processed".Colorfy(Verbs)} {transfers.ToStringColored( Numbers )}/{total.ToStringColored( Numbers )} {typeClass.FullName.Colorfy(TypeName)} in {$"{sw.Elapsed.TotalMilliseconds}ms".ToStringColored( Negation )}:\n{sb.ToString()}");
		return ( transfers, sw.Elapsed.TotalMilliseconds, total );
    }
}