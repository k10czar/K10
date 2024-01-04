using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AssetDatabaseUtils
{
    static readonly string OP = "(".Colorfy( Colors.Console.Punctuations );
    static readonly string CP = ")".Colorfy( Colors.Console.Punctuations );
    static readonly string OSB = "[".Colorfy( Colors.Console.Punctuations );
    static readonly string CSB = "]".Colorfy( Colors.Console.Punctuations );
    static readonly string OAB = "<".Colorfy( Colors.Console.Punctuations );
    static readonly string CAB = ">".Colorfy( Colors.Console.Punctuations );
    static readonly string DOT = ".".Colorfy( Colors.Console.Punctuations );

    public static void RequestPath( string path )
    {
#if UNITY_EDITOR
        Debug.Log( $"{"RequestPath".Colorfy( Colors.Console.Verbs )}( {path.Colorfy( Colors.Console.Names )} )" );
        var last = -1;
        for( int i = 0; i < path.Length; i++ )
        {
            var c = path[i];
            if( c == '/' || c == '\\' )
            {
                var root = ( last >= 0 ) ? path.Substring( 0, last ) : "";
                var folder = path.Substring( last + 1, i - ( last + 1 ) );
                var current = path.Substring( 0, i );
                if( !UnityEditor.AssetDatabase.IsValidFolder( path.Substring( 0, i ) ) )
                {
                    UnityEditor.AssetDatabase.CreateFolder( root, folder );
                    Debug.Log( $"{"CreateFolder".Colorfy( Colors.Console.Verbs )} folder at {root.Colorfy( Colors.Console.Names )}" );
                }
                last = i;
            }
        }
#else //UNITY_EDITOR
        Debug.Log( $"AssetDatabaseUtils{DOT}<color=yellow>GetAll</color>{OP}{type.Name.Colorfy( Colors.Lime )}{CP} {ConsoleMessage.ONLY_EDITOR}" );
        return null;
#endif //UNITY_EDITOR
    }
    
    public static T GetFirst<T>( string name, bool debug = true ) where T : ScriptableObject
    {
#if UNITY_EDITOR
        string[] assetNames = UnityEditor.AssetDatabase.FindAssets( $"{name} t:{typeof(T).Name}" );
        for( int i = 0; i < assetNames.Length; i++ )
        {
            var SOpath = UnityEditor.AssetDatabase.GUIDToAssetPath( assetNames[i] );
            if( !SOpath.AsPathIsFilename( name ) ) continue;
            var element = UnityEditor.AssetDatabase.LoadAssetAtPath<T>( SOpath );
		    if( debug ) Debug.Log( $"{"AssetDatabaseUtils".Colorfy( Colors.Console.Types )}{DOT}{"GetFirst".Colorfy( Colors.Console.Verbs )}{OAB}{typeof(T).Name.Colorfy( Colors.Console.Types )}{CAB} at {SOpath.Colorfy( Colors.ArcticLime )}" );
            return element;
        }
		if( debug ) Debug.Log( $"{"AssetDatabaseUtils".Colorfy( Colors.Console.Types )}{DOT}{"GetFirst".Colorfy( Colors.Console.Verbs )}{OAB}{typeof(T).Name.Colorfy( Colors.Console.Types )}{CAB}: {"NOT FOUND".Colorfy( Colors.Console.Negation )}" );
        return null;
#else //UNITY_EDITOR
        Debug.Log( $"AssetDatabaseUtils{DOT}<color=yellow>GetAll</color>{OP}{type.Name.Colorfy( Colors.Lime )}{CP} {ConsoleMessage.ONLY_EDITOR}" );
        return null;
#endif //UNITY_EDITOR
    }
    
    public static T[] GetAll<T>( string name, bool debug = true ) where T : ScriptableObject
    {
#if UNITY_EDITOR
        string[] assetNames = UnityEditor.AssetDatabase.FindAssets( $"{name} t:{typeof(T).Name}" );
        var collection = new List<T>();
        for( int i = 0; i < assetNames.Length; i++ )
        {
            var SOpath = UnityEditor.AssetDatabase.GUIDToAssetPath( assetNames[i] );
            var element = UnityEditor.AssetDatabase.LoadAssetAtPath<T>( SOpath );
            if( !SOpath.AsPathIsFilename( name ) ) continue;
            collection.Add( element );
        }
		if( debug ) Debug.Log( $"{"AssetDatabaseUtils".Colorfy( Colors.Console.Types )}{DOT}{"GetAll".Colorfy( Colors.Console.Verbs )}{OAB}{typeof(T).Name.Colorfy( Colors.Console.Types )}{CAB} found {collection.Count}:\n{string.Join( ", ", collection.ToList().ConvertAll<string>( ( so ) => so.NameOrNull() ) ).Colorfy( Colors.ArcticLime )}" );
        return collection.ToArray();
#else //UNITY_EDITOR
        Debug.Log( $"AssetDatabaseUtils{DOT}<color=yellow>GetAll</color>{OP}{type.Name.Colorfy( Colors.Lime )}{CP} {ConsoleMessage.ONLY_EDITOR}" );
        return null;
#endif //UNITY_EDITOR
    }
    
    public static T[] GetAll<T>( bool debug = true ) where T : ScriptableObject
    {
#if UNITY_EDITOR
        string[] assetNames = UnityEditor.AssetDatabase.FindAssets( $"t:{typeof(T).Name}" );
        var collection = new T[assetNames.Length];
        for( int i = 0; i < assetNames.Length; i++ )
        {
            var SOpath = UnityEditor.AssetDatabase.GUIDToAssetPath( assetNames[i] );
            var element = UnityEditor.AssetDatabase.LoadAssetAtPath<T>( SOpath );
            collection[ i ] = element;
        }
		if( debug ) Debug.Log( $"{"AssetDatabaseUtils".Colorfy( Colors.Console.Types )}{DOT}{"GetAll".Colorfy( Colors.Console.Verbs )}{OAB}{typeof(T).Name.Colorfy( Colors.Console.Types )}{CAB} found {collection.Length.ToString().Colorfy( Colors.Console.Numbers )}:\n{string.Join( ", ", collection.ToList().ConvertAll<string>( ( so ) => so.NameOrNull() ) ).Colorfy( Colors.ArcticLime )}" );
        return collection;
#else //UNITY_EDITOR
        Debug.Log( $"AssetDatabaseUtils{DOT}<color=yellow>GetAll</color>{OP}{type.Name.Colorfy( Colors.Lime )}{CP} {ConsoleMessage.ONLY_EDITOR}" );
        return null;
#endif //UNITY_EDITOR
    }
    
    public static ScriptableObject[] GetAll( System.Type type, bool debug = true ) 
    {
#if UNITY_EDITOR
        string[] assetNames = UnityEditor.AssetDatabase.FindAssets( $"t:{type.Name}" );
        var collection = new ScriptableObject[assetNames.Length];
        for( int i = 0; i < assetNames.Length; i++ )
        {
            var SOpath = UnityEditor.AssetDatabase.GUIDToAssetPath( assetNames[i] );
            var element = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>( SOpath );
            collection[ i ] = element;
        }
		if( debug ) Debug.Log( $"AssetDatabaseUtils{DOT}<color=yellow>GetAll</color>{OP}{type.Name.Colorfy( Colors.Lime )}{CP} found {collection.Length.ToString().Colorfy( Colors.Console.Numbers )}:\n   -{string.Join( ",\n   -", collection.ToList().ConvertAll<string>( ( so ) => so.NameOrNull() ) )}" );
        return collection;
#else //UNITY_EDITOR
        Debug.Log( $"AssetDatabaseUtils{DOT}<color=yellow>GetAll</color>{OP}{type.Name.Colorfy( Colors.Lime )}{CP} {ConsoleMessage.ONLY_EDITOR}" );
        return null;
#endif //UNITY_EDITOR
    }
}
