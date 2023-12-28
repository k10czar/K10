using System.Linq;
using UnityEditor;
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
                if( !AssetDatabase.IsValidFolder( path.Substring( 0, i ) ) )
                {
                    AssetDatabase.CreateFolder( root, folder );
                    Debug.Log( "CreateFolder " + folder + " at " + root );
                }
                last = i;
            }
        }
    }
    
    public static T[] GetAll<T>( bool debug = true ) where T : ScriptableObject
    {
        string[] assetNames = AssetDatabase.FindAssets( $"t:{typeof(T).Name}" );
        var collection = new T[assetNames.Length];
        for( int i = 0; i < assetNames.Length; i++ )
        {
            var SOpath = AssetDatabase.GUIDToAssetPath( assetNames[i] );
            var element = AssetDatabase.LoadAssetAtPath<T>( SOpath );
            collection[ i ] = element;
        }
		if( debug ) Debug.Log( $"AssetDatabaseUtils{DOT}{"GetAll".Colorfy( Colors.Console.Methods )}{OAB}{typeof(T).Name.Colorfy( Colors.Console.Types )}{CAB}:\n   -{string.Join( ",\n   -", collection.ToList().ConvertAll<string>( ( so ) => so.NameOrNull() ) )}" );
        return collection;
    }
    
    public static ScriptableObject[] GetAll( System.Type type, bool debug = true ) 
    {
        string[] assetNames = AssetDatabase.FindAssets( $"t:{type.Name}" );
        var collection = new ScriptableObject[assetNames.Length];
        for( int i = 0; i < assetNames.Length; i++ )
        {
            var SOpath = AssetDatabase.GUIDToAssetPath( assetNames[i] );
            var element = AssetDatabase.LoadAssetAtPath<ScriptableObject>( SOpath );
            collection[ i ] = element;
        }
		if( debug ) Debug.Log( $"AssetDatabaseUtils{DOT}<color=yellow>GetAll</color>{OP}{type.Name.Colorfy( Colors.Lime )}{CP}:\n   -{string.Join( ",\n   -", collection.ToList().ConvertAll<string>( ( so ) => so.NameOrNull() ) )}" );
        return collection;
    }
}
