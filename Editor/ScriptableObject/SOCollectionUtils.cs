using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class SOCollectionUtils
{
	public static List<T> FindAllAssetsOfType<T>() where T : UnityEngine.Object
	{
		var list = new List<T>();
		FindAllAssetsOfTypeNonAlloc<T>( list );
		return list;
	}

	public static void FindAllAssetsOfTypeNonAlloc<T>( IList<T> list ) where T : UnityEngine.Object
	{
		list.Clear();
		var assets = AssetDatabase.FindAssets( $"t:{ typeof(T) }" );
		for( int i = 0; i < assets.Length; i++ )
		{
			var guid = assets[i];
			var path = AssetDatabase.GUIDToAssetPath( guid );
			var exp = AssetDatabase.LoadAssetAtPath<T>( path );
			list.Add( exp );
		}
	}

    public static void AddAllNew<T>( SerializedObject serializedObject, bool removeNulls = false ) where T : ScriptableObject
    {
        // var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        // var results = from type in assemblies.GetTypes()
        //       where typeof(IFoo).IsAssignableFrom(type)
        //       select type;

        string[] guids = AssetDatabase.FindAssets( string.Format( "t:{0}", typeof( T ) ) );

        HashSet<object> _set = new HashSet<object>();
        var prop = serializedObject.FindProperty( "_objects" );

        for( int i = prop.arraySize - 1; i >= 0; i-- )
        {
            var element = prop.GetArrayElementAtIndex( i );
            if( element.objectReferenceValue != null ) _set.Add( element.objectReferenceValue );
            else if( removeNulls ) prop.DeleteArrayElementAtIndex( i );
        }

        for( int i = 0; i < guids.Length; i++ )
        {
            var assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
            var t = AssetDatabase.LoadAssetAtPath<T>( assetPath );

            if( _set.Contains( t ) ) continue;

            var id = prop.arraySize;
            prop.InsertArrayElementAtIndex( id );
            var element = prop.GetArrayElementAtIndex( id );
            element.objectReferenceValue = t;
        }
    }

    public static void AddAllNew<T>( SOCollection<T> collection, bool removeNulls = false ) where T : ScriptableObject { AddAllNew( (List<T>)collection.Objects, removeNulls ); }
    public static void AddAllNew<T>( List<T> list, bool removeNulls = false ) where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets( string.Format( "t:{0}", typeof( T ) ) );

        HashSet<object> set = new HashSet<object>();

        for( int i = list.Count - 1; i >= 0; i-- )
        {
            var element = list[i];
            if( element != null ) set.Add( element );
            else if( removeNulls ) list.RemoveAt( i );
        }

        for( int i = 0; i < guids.Length; i++ )
        {
            var assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
            var t = AssetDatabase.LoadAssetAtPath<T>( assetPath );
            if( !set.Contains( t ) ) list.Add( t );
        }
    }
}
