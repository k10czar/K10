using System.Collections.Generic;
using UnityEngine;

public abstract class HashedSOCollection<T> : BaseHashedSOCollection, IHashedSOCollection where T : HashedScriptableObject
{
	[SerializeField] List<T> _list = new List<T>();

	public T this[int index] => _list[index];
	public override int Count => _list.Count;

	public T GetElement( int hashId ) => this[hashId];

	public override System.Type GetElementType() => typeof( T );
	public override IHashedSO GetElementBase( int hashId ) => this[hashId];

	public override bool ContainsHashID( int hashID ) => ( hashID < _list.Count || _list[hashID] != null );
	public override bool Contains( IHashedSO obj ) => _list.Contains( obj as T );

#if UNITY_EDITOR
	protected override void Clear() { _list.Clear(); }
	public override bool EditorCanChangeIDsToOptimizeSpace => true;
	protected override bool AddElement( IHashedSO obj ) { if( obj is T t ) _list.Add( t ); return ( obj is T ); }
	protected override bool ResolveConflictedFile( IHashedSO t, string assetPath ) => true;
#endif
}