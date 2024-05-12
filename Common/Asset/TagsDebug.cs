using System.Collections.Generic;
using UnityEngine;

public class TagsDebug : ScriptableObject
{
    [SerializeField] List<string> _guids = new();
    [SerializeField] List<string> _names = new();
    Dictionary<string, string> _runtimeCache = null;

    public void Rebuild()
    {
#if UNITY_EDITOR

		var sw = new System.Diagnostics.Stopwatch();
		sw.Start();

        _guids.Clear();
        _names.Clear();

        var SB = new System.Text.StringBuilder();

        var tags = UnityEditor.AssetDatabase.FindAssets( $"t:{nameof( TagSO )}" );

        foreach( var t in tags )
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath( t );
            var tag = UnityEditor.AssetDatabase.LoadAssetAtPath<TagSO>( path );
            var name = tag.name;
            _guids.Add( t );
            _names.Add( name );
            
            SB.AppendLine( $"\t{name.Colorfy(Colors.Console.Names)}({tag.TypeNameOrNull().Colorfy(Colors.Console.TypeName)}) : {t.Colorfy(Colors.Console.Numbers)}" );	
        }
        
		sw.Stop();

        UnityEditor.EditorUtility.SetDirty( this );
        
        Debug.Log( $"{"Found".Colorfy(Colors.Console.Verbs)} {tags.Length.ToStringColored(Colors.Console.Numbers)} {"Tags".Colorfy(Colors.Console.TypeName)} that took {sw.Elapsed.TotalMilliseconds.ToStringColored(Colors.Console.Numbers)}ms:\n{SB}" );
#else
        Debug.LogError( "USELESS CALL to TagDebug.Rebuild on Runtime,\nit only can build this tables in Editor" );
#endif //UNITY_EDITOR
    }

    private static TagsDebug _instance;
    public static TagsDebug Instance
    {
        get
        {
            if( _instance == null )
            {
                _instance = Resources.Load<TagsDebug>( "TagsDebug" );// ?? ScriptableObject.CreateInstance<TagsDebug>();
#if UNITY_EDITOR
                if( _instance == null )
                {
                    _instance = CreateInstance<TagsDebug>();
                    AssetDatabaseUtils.RequestPath( "Assets/Resources/TagsDebug" );
                    UnityEditor.AssetDatabase.CreateAsset( _instance, "Assets/Resources/TagsDebug.asset" );
                    UnityEditor.AssetDatabase.SaveAssets();
                }
#endif //UNITY_EDITOR
            }
            return _instance;
        }
    }

    public string GetNameOf(string guid, string missing = "NOT_FOUND")
    {
        if( _runtimeCache == null )
        {
            _runtimeCache = new Dictionary<string, string>();
            for (int i = 0; i < _guids.Count; i++) _runtimeCache.Add( _guids[i], _names[i] );
        }
        if( _runtimeCache.TryGetValue( guid, out var name ) ) return name;
        return missing;
    }

    public static string Of( string guid, string missing = "NOT_FOUND" ) => Instance.GetNameOf( guid );
    public static bool TryFind(string guid, out string name)
    {
        var result = Instance.GetNameOf( guid, null );
        if( result == null ) 
        { 
            name = null;
            return false; 
        }
        name = result;
        return true;
    }
}
