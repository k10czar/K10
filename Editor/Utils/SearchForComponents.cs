//Assets/Editor/SearchForComponents.cs
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[System.Serializable]
public class ReflectedPath
{
    [SerializeField] string _path = "";
	[SerializeField] System.Reflection.BindingFlags _bindingAttr = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    public bool IsValid => !string.IsNullOrEmpty( _path );
    public string Path { get { return _path; } set { _path = value; } }
    public System.Reflection.BindingFlags BindingAttr  { get { return _bindingAttr; } set { _bindingAttr = value; } }

	public object GetMemberObject( object rootObject, System.Type type, out string lastMemberName )
	{
		lastMemberName = "";
		var obj = rootObject;
		var stack = _path.Split( '.' );
		var sLen = stack.Length;
		for( int i = 0; i < sLen; i++ )
		{
			lastMemberName = stack[i];
			obj = GetMember( lastMemberName, obj, ref type );
			if( obj == null ) return null;
		}
		return obj;
	}

	public void Execute( object rootObject, System.Type type, out string lastMemberName, params object[] parameters )
	{
		lastMemberName = "";
		var obj = rootObject;
		var stack = _path.Split( '.' );
		var sLen = stack.Length;
		for( int i = 0; i < sLen - 1; i++ )
		{
			lastMemberName = stack[i];
			obj = GetMember( lastMemberName, obj, ref type );
			if( obj == null ) return;
		}

        if( sLen > 0 )
        {
            var method = type.GetMethod( stack[sLen - 1], _bindingAttr );
            method.Invoke( obj, parameters );
        }
	}

	public object GetMember( string memberName, object element, ref System.Type type )
	{
		if( element == null ) return element;
		var field = type.GetField( memberName, _bindingAttr );
		if( field != null )
		{
			type = field.FieldType;
			object ret = null;
			try { ret = field.GetValue( element ); }
			catch { }
			return ret;
			//return field.GetValue( element );
		}
		var property = type.GetProperty( memberName, _bindingAttr );
		if( property != null )
		{
			type = property.PropertyType;
			object ret = null;
			try { ret = property.GetValue( element ); }
			catch { }
			return ret;
			// return property.GetValue( element );
		}
		type = type.BaseType;
		if( type.BaseType != null ) return GetMember( memberName, element, ref type );
		return element;
	}
}

public class SearchForComponents : EditorWindow {
    [MenuItem( "K10/Editors/Search For Components" )]
    static void Init () {
        SearchForComponents window = (SearchForComponents) EditorWindow.GetWindow( typeof( SearchForComponents ) );
        window.Show();
        window.position = new Rect( 20, 80, 550, 500 );
    }

    string[] modes = new string[] { "Search for component usage", "Search for missing components" };
    string[] checkType = new string[] { "Check single component", "Check all components" };

    List<string> listResult;
    List<ComponentNames> prefabComponents,notUsedComponents, addedComponents, existingComponents, sceneComponents;
    int editorMode, selectedCheckType;
    MonoScript targetComponent;
    string componentName = "";

    ReflectedPath filterProp = new ReflectedPath();
    ReflectedPath methodToExecute = new ReflectedPath();

    bool showPrefabs, showAdded, showScene, showUnused = true;
    Vector2 scroll, scroll1, scroll2, scroll3, scroll4;

    class ComponentNames {
        public string componentName;
        public string namespaceName;
        public string assetPath;
        public List<string> usageSource;
        public ComponentNames ( string comp, string space, string path ) {
            this.componentName = comp;
            this.namespaceName = space;
            this.assetPath = path;
            this.usageSource = new List<string>();
        }
        public override bool Equals ( object obj ) {
            return ( (ComponentNames) obj ).componentName == componentName && ( (ComponentNames) obj ).namespaceName == namespaceName;
        }
        public override int GetHashCode () {
            return componentName.GetHashCode() + namespaceName.GetHashCode();
        }
    }

    void OnGUI () {
        GUILayout.Label(position+"");
        GUILayout.Space( 3 );
        int oldValue = GUI.skin.window.padding.bottom;
        GUI.skin.window.padding.bottom = -20;
        Rect windowRect = GUILayoutUtility.GetRect( 1, 17 );
        windowRect.x += 4;
        windowRect.width -= 7;
        editorMode = GUI.SelectionGrid( windowRect, editorMode, modes, 2, "Window" );
        GUI.skin.window.padding.bottom = oldValue;

        switch ( editorMode ) {
            case 0:
                selectedCheckType = GUILayout.SelectionGrid( selectedCheckType, checkType, 2, "Toggle" );
                GUI.enabled = selectedCheckType == 0;
                targetComponent = (MonoScript) EditorGUILayout.ObjectField( targetComponent, typeof( MonoScript ), false );
                GUI.enabled = true;

                if ( GUILayout.Button( "Check component usage" ) ) {
                    AssetDatabase.SaveAssets();
                    switch ( selectedCheckType ) {
                        case 0:
                            componentName = targetComponent.name;
                            string targetPath = AssetDatabase.GetAssetPath( targetComponent );
                            string[] allPrefabs = GetAllPrefabs();
                            listResult = new List<string>();
                            var type = targetComponent.GetClass();
                            foreach ( string prefab in allPrefabs ) {
                                string[] single = new string[] { prefab };
                                string[] dependencies = AssetDatabase.GetDependencies( single );
                                bool hasDependency = false;
                                foreach ( string dependedAsset in dependencies ) {
                                    if ( dependedAsset != targetPath ) continue;
                                    hasDependency = true;
                                    break;
                                }
                                if( !hasDependency ) continue;
                                var obj = AssetDatabase.LoadMainAssetAtPath( prefab );
                                if( obj is GameObject go )
                                {
                                    var comp = go.GetComponentInChildren( type, true );
                                    if( comp != null ) listResult.Add( prefab );
                                }
                            }
                            break;
                        case 1:
                            List<string> scenesToLoad = new List<string>();
                            existingComponents = new List<ComponentNames>();
                            prefabComponents = new List<ComponentNames>();
                            notUsedComponents = new List<ComponentNames>();
                            addedComponents = new List<ComponentNames>();
                            sceneComponents = new List<ComponentNames>();

                            if ( EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() ) {
                                string projectPath = Application.dataPath;
                                projectPath = projectPath.Substring( 0, projectPath.IndexOf( "Assets" ) );

                                string[] allAssets = AssetDatabase.GetAllAssetPaths();

                                foreach ( string asset in allAssets ) {
                                    int indexCS = asset.IndexOf( ".cs" );
                                    int indexJS = asset.IndexOf( ".js" );
                                    if ( indexCS != -1 || indexJS != -1 ) {
                                        ComponentNames newComponent = new ComponentNames( NameFromPath( asset ), "", asset );
                                        try {
                                            System.IO.FileStream FS = new System.IO.FileStream( projectPath + asset, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite );
                                            System.IO.StreamReader SR = new System.IO.StreamReader( FS );
                                            string line;
                                            while ( !SR.EndOfStream ) {
                                                line = SR.ReadLine();
                                                int index1 = line.IndexOf( "namespace" );
                                                int index2 = line.IndexOf( "{" );
                                                if ( index1 != -1 && index2 != -1 ) {
                                                    line = line.Substring( index1 + 9 );
                                                    index2 = line.IndexOf( "{" );
                                                    line = line.Substring( 0, index2 );
                                                    line = line.Replace( " ", "" );
                                                    newComponent.namespaceName = line;
                                                }
                                            }
                                        } catch {
                                        }

                                        existingComponents.Add( newComponent );

                                        try {
                                            System.IO.FileStream FS = new System.IO.FileStream( projectPath + asset, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite );
                                            System.IO.StreamReader SR = new System.IO.StreamReader( FS );

                                            string line;
                                            int lineNum = 0;
                                            while ( !SR.EndOfStream ) {
                                                lineNum++;
                                                line = SR.ReadLine();
                                                int index = line.IndexOf( "AddComponent" );
                                                if ( index != -1 ) {
                                                    line = line.Substring( index + 12 );
                                                    if ( line[0] == '(' ) {
                                                        line = line.Substring( 1, line.IndexOf( ')' ) - 1 );
                                                    } else if ( line[0] == '<' ) {
                                                        line = line.Substring( 1, line.IndexOf( '>' ) - 1 );
                                                    } else {
                                                        continue;
                                                    }
                                                    line = line.Replace( " ", "" );
                                                    line = line.Replace( "\"", "" );
                                                    index = line.LastIndexOf( '.' );
                                                    ComponentNames newComp;
                                                    if ( index == -1 ) {
                                                        newComp = new ComponentNames( line, "", "" );
                                                    } else {
                                                        newComp = new ComponentNames( line.Substring( index + 1, line.Length - ( index + 1 ) ), line.Substring( 0, index ), "" );
                                                    }
                                                    string pName = asset + ", Line " + lineNum;
                                                    newComp.usageSource.Add( pName );
                                                    index = addedComponents.IndexOf( newComp );
                                                    if ( index == -1 ) {
                                                        addedComponents.Add( newComp );
                                                    } else {
                                                        if ( !addedComponents[index].usageSource.Contains( pName ) ) addedComponents[index].usageSource.Add( pName );
                                                    }
                                                }
                                            }
                                        } catch {
                                        }
                                    }
                                    int indexPrefab = asset.IndexOf( ".prefab" );

                                    if ( indexPrefab != -1 ) {
                                        string[] single = new string[] { asset };
                                        string[] dependencies = AssetDatabase.GetDependencies( single );
                                        foreach ( string dependedAsset in dependencies ) {
                                            if ( dependedAsset.IndexOf( ".cs" ) != -1 || dependedAsset.IndexOf( ".js" ) != -1 ) {
                                                ComponentNames newComponent = new ComponentNames( NameFromPath( dependedAsset ), GetNamespaceFromPath( dependedAsset ), dependedAsset );
                                                int index = prefabComponents.IndexOf( newComponent );
                                                if ( index == -1 ) {
                                                    newComponent.usageSource.Add( asset );
                                                    prefabComponents.Add( newComponent );
                                                } else {
                                                    if ( !prefabComponents[index].usageSource.Contains( asset ) ) prefabComponents[index].usageSource.Add( asset );
                                                }
                                            }
                                        }
                                    }
                                    int indexUnity = asset.IndexOf( ".unity" );
                                    if ( indexUnity != -1 ) {
                                        scenesToLoad.Add( asset );
                                    }
                                }

                                for ( int i = addedComponents.Count - 1; i > -1; i-- ) {
                                    addedComponents[i].assetPath = GetPathFromNames( addedComponents[i].namespaceName, addedComponents[i].componentName );
                                    if ( addedComponents[i].assetPath == "" ) addedComponents.RemoveAt( i );

                                }

                                foreach ( string scene in scenesToLoad ) {
                                    EditorSceneManager.OpenScene( scene );
                                    GameObject[] sceneGOs = GetAllObjectsInScene();
                                    foreach ( GameObject g in sceneGOs ) {
                                        Component[] comps = g.GetComponentsInChildren<Component>( true );
                                        foreach ( Component c in comps ) {

                                            if ( c != null && c.GetType() != null && c.GetType().BaseType != null && c.GetType().BaseType == typeof( MonoBehaviour ) ) {
                                                SerializedObject so = new SerializedObject( c );
                                                SerializedProperty p = so.FindProperty( "m_Script" );
                                                string path = AssetDatabase.GetAssetPath( p.objectReferenceValue );
                                                ComponentNames newComp = new ComponentNames( NameFromPath( path ), GetNamespaceFromPath( path ), path );
                                                newComp.usageSource.Add( scene );
                                                int index = sceneComponents.IndexOf( newComp );
                                                if ( index == -1 ) {
                                                    sceneComponents.Add( newComp );
                                                } else {
                                                    if ( !sceneComponents[index].usageSource.Contains( scene ) ) sceneComponents[index].usageSource.Add( scene );
                                                }
                                            }
                                        }
                                    }
                                }

                                foreach ( ComponentNames c in existingComponents ) {
                                    if ( addedComponents.Contains( c ) ) continue;
                                    if ( prefabComponents.Contains( c ) ) continue;
                                    if ( sceneComponents.Contains( c ) ) continue;
                                    notUsedComponents.Add( c );
                                }

                                addedComponents.Sort( SortAlphabetically );
                                prefabComponents.Sort( SortAlphabetically );
                                sceneComponents.Sort( SortAlphabetically );
                                notUsedComponents.Sort( SortAlphabetically );
                            }
                            break;
                    }
                }
                break;
            case 1:
                if ( GUILayout.Button( "Search!" ) ) {
                    string[] allPrefabs = GetAllPrefabs();
                    listResult = new List<string>();
                    foreach ( string prefab in allPrefabs ) {
                        UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath( prefab );
                        GameObject go;
                        try {
                            go = (GameObject) o;
                            Component[] components = go.GetComponentsInChildren<Component>( true );
                            foreach ( Component c in components ) {
                                if ( c == null ) {
                                    listResult.Add( prefab );
                                    break;
                                }
                            }
                        } catch {
                            Debug.Log( "For some reason, prefab " + prefab + " won't cast to GameObject" );
                        }
                    }
                }
                break;
        }
        if ( editorMode == 1 || selectedCheckType == 0 ) {
            if ( listResult != null ) {
                if ( listResult.Count == 0 ) {
                    GUILayout.Label( editorMode == 0 ? ( componentName == "" ? "Choose a component" : "No prefabs use component " + componentName ) : ( "No prefabs have missing components!\nClick Search to check again" ) );
                } else {
                    GUILayout.Label( editorMode == 0 ? ( $"The following {listResult.Count} prefabs use component " + componentName + ":" ) : ( $"The following prefabs have {listResult.Count} missing components:" ) );
                    var count = listResult.Count;
                    if( editorMode == 0 )
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label( "Filter:", GUILayout.MaxWidth( 35 ) );
                        filterProp.BindingAttr = (System.Reflection.BindingFlags)EditorGUILayout.EnumFlagsField( filterProp.BindingAttr, GUILayout.MaxWidth( 80 ) );
                        filterProp.Path = EditorGUILayout.TextField( filterProp.Path );
                        GUILayout.EndHorizontal();


                        GUILayout.BeginHorizontal();
                        methodToExecute.BindingAttr = (System.Reflection.BindingFlags)EditorGUILayout.EnumFlagsField( methodToExecute.BindingAttr, GUILayout.MaxWidth( 80 ) );
                        methodToExecute.Path = EditorGUILayout.TextField( methodToExecute.Path );
                        var exec = GUILayout.Button( "Execute", GUILayout.MaxWidth( 64 ) );
                        GUILayout.EndHorizontal();

                        if( exec )
                        {
                            var sw = new System.Diagnostics.Stopwatch();
                            sw.Start();
                            // count = 1;
                            for( int i = count - 1; i >= 0; i-- )
                            {
                                var result = listResult[i];
                                var obj = AssetDatabase.LoadMainAssetAtPath( result );
                                if( obj is GameObject go )
                                {
                                    var type = targetComponent.GetClass();
                                    var comps = go.GetComponentsInChildren( type, true );

                                    bool compOnRoot = false;

                                    var exists = !filterProp.IsValid;
                                    foreach( var c in comps )
                                    {
                                        if( c == null ) continue;
                                        if( c.transform.parent == null ) compOnRoot = true;
                                        if( filterProp.IsValid )
                                        {
                                            var filter = filterProp.GetMemberObject( c, type, out var name );
                                            if( filter is System.Boolean b && b )
                                            {
                                                exists = true;
                                                Debug.Log( $"{c.HierarchyNameOrNull()}.{filterProp.Path} {b} - {name.ToStringOrNull()}" );
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                        if( methodToExecute.IsValid )
                                        {
                                            methodToExecute.Execute( c, type, out var name, new object[0] );
                                            // Debug.Log( methodToExecute.GetMemberObject( c, type, out var name ) + $" - {name.ToStringOrNull()}" );
                                        }
                                    }
                                    if( !exists )
                                    {
                                        listResult.RemoveAt( i );
                                    }

                                    if( !compOnRoot ) Debug.Log( $"{go.HierarchyNameOrNull()} does not have component on Root" );
                                }
                                else listResult.RemoveAt( i );
                            }
                            sw.Stop();
                            Debug.Log( $"Took {sw.Elapsed.TotalMilliseconds}ms to Recheck {count} {componentName}" );
                        }
                    }
                    else if( editorMode == 1 && GUILayout.Button( "Remove Missing Components" ) ) RemoveMissingComponents(count);
                    scroll = GUILayout.BeginScrollView( scroll );
                    foreach ( string s in listResult ) {
                        GUILayout.BeginHorizontal();
                        // GUILayout.Label( s, GUILayout.Width( position.width / 2 ) );
                        if ( GUILayout.Button( "Select", GUILayout.MaxWidth( 64 ) ) ) {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath( s );
                        }
                        GUILayout.Label( s, GUILayout.Width( position.width - 74 ) );
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();
                }
            }
        } else {
            showPrefabs = GUILayout.Toggle( showPrefabs, "Show prefab components" );
            if ( showPrefabs ) {
                GUILayout.Label( "The following components are attatched to prefabs:" );
                DisplayResults( ref scroll1, ref prefabComponents );
            }
            showAdded = GUILayout.Toggle( showAdded, "Show AddComponent arguments" );
            if ( showAdded ) {
                GUILayout.Label( "The following components are AddComponent arguments:" );
                DisplayResults( ref scroll2, ref addedComponents );
            }
            showScene = GUILayout.Toggle( showScene, "Show Scene-used components" );
            if ( showScene ) {
                GUILayout.Label( "The following components are used by scene objects:" );
                DisplayResults( ref scroll3, ref sceneComponents );
            }
            showUnused = GUILayout.Toggle( showUnused, "Show Unused Components" );
            if ( showUnused ) {
                GUILayout.Label( "The following components are not used by prefabs, by AddComponent, OR in any scene:" );
                DisplayResults( ref scroll4, ref notUsedComponents );
            }
        }
    }

    private void RemoveMissingComponents(int count)
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        for (int i = count - 1; i >= 0; i--)
        {
            var result = listResult[i];
            var obj = AssetDatabase.LoadMainAssetAtPath(result);
            if (obj is GameObject go)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (HasMissingComponents(go))
                {
                    RecursiveRemoveMissingComponents(go.transform);
                    if (HasMissingComponents(go)) Debug.Log($"{result} still has Missing Components");
                    else listResult.RemoveAt(i);
                }
                else listResult.RemoveAt(i);
                AssetDatabase.SaveAssetIfDirty(go);
            }
        }
        sw.Stop();
        Debug.Log($"Took {sw.Elapsed.TotalMilliseconds}ms to Remove Missing");
    }

    bool HasMissingComponents( GameObject go )
    {
        var components = go.GetComponentsInChildren<Component>( true );
        foreach ( Component c in components ) {
            if ( c == null )  return true;
        }
        return false;
    }

    void RecursiveRemoveMissingComponents( Transform t )
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript( t.gameObject );
        for (int i = t.childCount - 1; i >= 0; i--) {
            RecursiveRemoveMissingComponents( t.GetChild(i) );
        }
    }

    int SortAlphabetically ( ComponentNames a, ComponentNames b ) {
        return a.assetPath.CompareTo( b.assetPath );
    }

    GameObject[] GetAllObjectsInScene () {
        List<GameObject> objectsInScene = new List<GameObject>();
        GameObject[] allGOs = (GameObject[]) Resources.FindObjectsOfTypeAll( typeof( GameObject ) );
        foreach ( GameObject go in allGOs ) {
            //if ( go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave )
            //    continue;

            string assetPath = AssetDatabase.GetAssetPath( go.transform.root.gameObject );
            if ( !string.IsNullOrEmpty( assetPath ) )
                continue;

            objectsInScene.Add( go );
        }

        return objectsInScene.ToArray();
    }

    void DisplayResults ( ref Vector2 scroller, ref List<ComponentNames> list ) {
        if ( list == null ) return;
        scroller = GUILayout.BeginScrollView( scroller );
        foreach ( ComponentNames c in list ) {
            GUILayout.BeginHorizontal();
            GUILayout.Label( c.assetPath, GUILayout.Width( position.width / 5 *4 ) );
            if ( GUILayout.Button( "Select", GUILayout.Width( position.width / 5 - 30 ) ) ) {
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath( c.assetPath );
            }
            GUILayout.EndHorizontal();
            if ( c.usageSource.Count == 1 ) {
                GUILayout.Label( "   In 1 Place: " + c.usageSource[0] );
            }
            if ( c.usageSource.Count > 1 ) {
                GUILayout.Label( "   In " + c.usageSource.Count + " Places: " + c.usageSource[0] + ", " + c.usageSource[1] + ( c.usageSource.Count > 2 ? ", ..." : "" ) );
            }
        }
        GUILayout.EndScrollView();

    }

    string NameFromPath ( string s ) {
        s = s.Substring( s.LastIndexOf( '/' ) + 1 );
        return s.Substring( 0, s.Length - 3 );
    }

    string GetNamespaceFromPath ( string path ) {
        foreach ( ComponentNames c in existingComponents ) {
            if ( c.assetPath == path ) {
                return c.namespaceName;
            }
        }
        return "";
    }

    string GetPathFromNames ( string space, string name ) {
        ComponentNames test = new ComponentNames( name, space, "" );
        int index = existingComponents.IndexOf( test );
        if ( index != -1 ) {
            return existingComponents[index].assetPath;
        }
        return "";
    }

    public static string[] GetAllPrefabs () {
        string[] temp = AssetDatabase.GetAllAssetPaths();
        List<string> result = new List<string>();
        foreach ( string s in temp ) {
            if ( s.Contains( ".prefab" ) ) result.Add( s );
        }
        return result.ToArray();
    }
}