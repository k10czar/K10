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
}
