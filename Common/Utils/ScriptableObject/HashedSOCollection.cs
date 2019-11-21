using System.Collections.Generic;
using UnityEngine;

public abstract class HashedSOCollection<T> : BaseHashedSOCollection where T : HashedScriptableObject
{
	[SerializeField] List<T> _list = new List<T>();

	public T this[int index] => _list[index];
	public override int Count => _list.Count;

	public T GetElement( int hashId ) => this[hashId];
	public T GetElementOrDefault( int hashId ) { if( hashId >= 0 && hashId < _list.Count ) return this[hashId]; return default( T ); }

	public override System.Type GetElementType() => typeof( T );
	public override IHashedSO GetElementBase( int hashId ) => this[hashId];

	public override bool ContainsHashID( int hashID ) => ( hashID < _list.Count || _list[hashID] != null );
	public override bool Contains( IHashedSO obj ) => _list.Contains( obj as T );

#if UNITY_EDITOR
	protected override void Clear() { _list.Clear(); }
	public override bool EditorCanChangeIDsToOptimizeSpace => true;
	protected override bool AddElement( IHashedSO obj ) { if( obj is T t ) _list.Add( t ); return ( obj is T ); }
	protected override bool ResolveConflictedFile( IHashedSO t, string assetPath ) => true;
	public override bool TryResolveConflict( int i )
	{
		if( i >= Count ) return false;
		var element = _list[i];
		var realId = element.HashID;
		if( realId == i ) return false;
		if( _list[realId] != null ) return false;
		_list[realId] = element;
		_list[i] = null;
		UnityEditor.EditorUtility.SetDirty( this );
		return true;
	}
	protected override bool SetRealPosition( IHashedSO obj )
	{
		var t = obj as T;
		if( t == null ) return false;
		var id = obj.HashID;
		while( _list.Count <= id ) _list.Add( null );
		_list[id] = t;
		UnityEditor.EditorUtility.SetDirty( this );
		return true;
	}
#endif
}