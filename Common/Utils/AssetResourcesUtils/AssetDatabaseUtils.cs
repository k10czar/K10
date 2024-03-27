using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Colors.Console;

public static class AssetDatabaseUtils
{
    static readonly string OP = "(".Colorfy( Punctuations );
    static readonly string CP = ")".Colorfy( Punctuations );
    static readonly string OSB = "[".Colorfy( Punctuations );
    static readonly string CSB = "]".Colorfy( Punctuations );
    static readonly string OAB = "<".Colorfy( Punctuations );
    static readonly string CAB = ">".Colorfy( Punctuations );
    static readonly string DOT = ".".Colorfy( Punctuations );

    public static void RequestPath( string path )
    {
#if UNITY_EDITOR
        Debug.Log( $"{"RequestPath".Colorfy( Verbs )}( {path.Colorfy( Names )} )" );
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
                    Debug.Log( $"{"CreateFolder".Colorfy( Verbs )} folder at {root.Colorfy( Names )}" );
                }
                last = i;
            }
        }
#else //UNITY_EDITOR
        Debug.LogError( $"RequestPath( {path} ) {ConsoleMessage.ONLY_EDITOR}" );
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
		    if( debug ) Debug.Log( $"{"AssetDatabaseUtils".Colorfy( TypeName )}{DOT}{"GetFirst".Colorfy( Verbs )}{OAB}{typeof(T).Name.Colorfy( TypeName )}{CAB} at {SOpath.Colorfy( Names )}" );
            return element;
        }
		if( debug ) Debug.Log( $"{"AssetDatabaseUtils".Colorfy( TypeName )}{DOT}{"GetFirst".Colorfy( Verbs )}{OAB}{typeof(T).Name.Colorfy( TypeName )}{CAB}: {"NOT FOUND".Colorfy( Negation )}" );
        return null;
#else //UNITY_EDITOR
        Debug.LogError( $"AssetDatabaseUtils.GetFirst<{typeof(T)}>( {name} ) {ConsoleMessage.ONLY_EDITOR}" );
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
		if( debug ) Debug.Log( $"{"AssetDatabaseUtils".Colorfy( TypeName )}{DOT}{"GetAll".Colorfy( Verbs )}{OAB}{typeof(T).Name.Colorfy( TypeName )}{CAB} found {collection.Count}:\n{string.Join( ", ", collection.ToList().ConvertAll<string>( ( so ) => so.NameOrNull() ) ).Colorfy( Names )}" );
        return collection.ToArray();
#else //UNITY_EDITOR
        Debug.Log( $"AssetDatabaseUtils.GetAll<{typeof(T)}>( {name} ) {ConsoleMessage.ONLY_EDITOR}" );
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
		if( debug ) Debug.Log( $"{"AssetDatabaseUtils".Colorfy( TypeName )}{DOT}{"GetAll".Colorfy( Verbs )}{OAB}{typeof(T).Name.Colorfy( TypeName )}{CAB} found {collection.Length.ToString().Colorfy( Numbers )}:\n{string.Join( ", ", collection.ToList().ConvertAll<string>( ( so ) => so.NameOrNull() ) ).Colorfy( Colors.ArcticLime )}" );
        return collection;
#else //UNITY_EDITOR
        Debug.Log( $"AssetDatabaseUtils.GetAll<{typeof(T)}>() {ConsoleMessage.ONLY_EDITOR}" );
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
		if( debug ) Debug.Log( $"{"AssetDatabaseUtils".Colorfy( TypeName )}{DOT}{"GetAll".Colorfy( Verbs )}{OP} {type.Name.Colorfy( Keyword )} {CP} found {collection.Length.ToString().Colorfy( Numbers )}:\n   -{string.Join( ",\n   -", collection.ToList().ConvertAll<string>( ( so ) => so.NameOrNull() ) )}" );
        return collection;
#else //UNITY_EDITOR
        Debug.Log( $"AssetDatabaseUtils.GetAll( {type} ) {ConsoleMessage.ONLY_EDITOR}" );
        return null;
#endif //UNITY_EDITOR
    }
}
