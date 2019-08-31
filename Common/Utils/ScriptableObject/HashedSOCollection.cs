

using System.Collections.Generic;
using UnityEngine;

public interface IHashedSOCollection
{
    void RequestMember( object obj );
    bool Contains( object obj );
    int GetRealIDFromHashID( int hashID );

	#if UNITY_EDITOR
    int GetUnusedHashID();
    bool HashHasConflict( HashedScriptableObject obj );
    #endif
}

public class HashedSOCollection<T> : SOCollection<T>, IHashedSOCollection where T : HashedScriptableObject
{
    //TO DO: cache HashID to ID relations if needed for performance
    Dictionary<int,int> _hashRelationCache;

    public void RequestMember( object obj )
    {
        var t = obj as T;
        if( t == null ) return;
        base.RequestMember( t );
    }

    public bool ContainsHashID( int hashID )
    {
        var count = Count;
        for( int i = 0 ; i < count; i++ )
        {
            var element = base[i];
            if( element.HashID == hashID ) return true;
        }
        return false;
    }

    public bool Contains( object obj )
    {
        var t = obj as T;
        if( t == null ) return false;
        return base.Contains( t );
    }

    void CheckHashRelation()
    {
        if( _hashRelationCache != null ) return;

        _hashRelationCache = new Dictionary<int,int>();

        var count = Count;
        for( int i = 0 ; i < count; i++ )
        {
            var element = base[i];
            if( element == null ) continue;
            var hashID = element.HashID;
            if( _hashRelationCache.ContainsKey( hashID ) ) Debug.LogWarningFormat( "HashID conflict {0} {1}", element.name, base[_hashRelationCache[hashID]].name );
            _hashRelationCache[ hashID ] = i;
        }
    }
    
    public int GetRealIDFromHashID( int hashID )
    {
        CheckHashRelation();
        int id;
        if( _hashRelationCache.TryGetValue( hashID, out id ) ) return id;
        // var count = Count;
        // for( int i = 0 ; i < count; i++ )
        // {
        //     var element = base[i];
        //     if( element.HashID == hashID ) return i;
        // }
        return -1;
    }

    public T GetItemFromHashID( int hashID )
    {
        var id = GetRealIDFromHashID( hashID );
        if( id < 0 ) return null;
        return base[id];
    }

	#if UNITY_EDITOR
    public bool HashHasConflict( HashedScriptableObject obj )
    {
        var count = Count;
        var hashID = obj.HashID;
        for( int i = 0 ; i < count; i++ )
        {
            var element = base[i];
            if( element != null && element != obj && element.HashID == hashID ) return true;
        }
        return false;
    }
	#endif
    
	#if UNITY_EDITOR
    public int GetUnusedHashID()
    {
        var count = Count;

        HashSet<int> set = new HashSet<int>();
        for( int i = 0 ; i < count; i++ )
        {
            var element = base[i];
            if (element == null) continue;
            set.Add( element.HashID );
        }
        
        //Brute Force to find some unused hash
        for( int i = 1; i < int.MaxValue; i++ ) { if( !set.Contains( i ) ) return i; }
        for( int i = -1; i > int.MinValue; i-- ) { if( !set.Contains( i ) ) return i; }

        return 0;
    }
	#endif
}