using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AssetDatabaseUtils
{
    public static void RequestPath( string path )
    {
        Debug.Log( "RequestPath " + path );
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
		if( debug ) Debug.Log( $"AssetDatabaseUtils<color=red>.</color><color=yellow>GetAll</color><color=red><</color><color=lime>{typeof(T).Name}</color><color=red>></color>:\n\t-{string.Join( ",\n\t-", collection.ToList().ConvertAll<string>( ( so ) => so.NameOrNull() ) )}" );
        return collection;
    }
}
