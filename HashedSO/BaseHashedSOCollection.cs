using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public interface IHashedSOCollectionEditor
{
	void EditorTryOptimize();
	void EditorCheckConsistency();
	bool TryResolveConflict( int i );
	bool EditorRequestMember( Object t, bool forceCorrectPosition = false );
	bool EditorCanChangeIDsToOptimizeSpace { get; }
	void Editor_HACK_Remove( int id );
	void Editor_HACK_EnforceHashIDs();
	void EditorRemoveWrongElements();
}
#endif

public interface IHashedSOCollection
#if UNITY_EDITOR
	: IHashedSOCollectionEditor
#endif
{
	bool ContainsHashID( int hashID );
	bool Contains( IHashedSO obj );
	int Count { get; }
	int PlaceholderCount { get; } //new check consistence - Placeholder
	IHashedSO GetElementBase( int hashID );
	IHashedSO PlaceholderGetElementBase( int hashID );//new check consistence - Placeholder
}

public abstract class BaseHashedSOCollection : ScriptableObject, IHashedSOCollection, System.Collections.IEnumerable
{

	public abstract int Count { get; }
	public abstract int PlaceholderCount { get; } //new check consistence - Placeholder
	public abstract IHashedSO GetElementBase( int index );
	public abstract IHashedSO PlaceholderGetElementBase( int index );//new check consistence - Placeholder
	public abstract bool Contains( IHashedSO element );
	public abstract bool ContainsHashID( int hashID );
	public abstract bool PlaceHolderContainsHashID( int hashID );//new check consistence - Placeholder

	IEnumerator IEnumerable.GetEnumerator()
	{
		var count = Count;
		for( int i = 0; i < count; i++ )
		{
			var element = GetElementBase( i );
			if( element != null ) yield return element;
		}
	}

	public abstract System.Type GetElementType();

#if UNITY_EDITOR
	public abstract void Editor_HACK_Remove( int id );

	void IHashedSOCollectionEditor.Editor_HACK_EnforceHashIDs()
	{
		HashSet<IHashedSO> _alreadyVisited = new HashSet<IHashedSO>();

		for( int i = 0; i < Count; i++ )
		{
			var element = GetElementBase( i );
			if( element == null ) continue;
			if( element.HashID != i ) continue;

			_alreadyVisited.Add( element );
		}

		for( int j = 0; j < Count; j++ )
		{
			var element = GetElementBase( j );
			if( element == null ) continue;

			if( _alreadyVisited.Contains( element ) )
			{

				if( element.HashID != j )
				{
					Editor_HACK_Remove( j );
					j--;
				}

				continue;
			}
			_alreadyVisited.Add( element );
			if( element.HashID == j ) continue;
			element.SetHashID( j );
			EditorUtility.SetDirty( element as ScriptableObject );
		}
	}

	void IHashedSOCollectionEditor.EditorRemoveWrongElements()
	{
		List<int> _elementsToRemove = new List<int>();
		for( int i = 0; i < Count; i++ )
		{
			var element = GetElementBase( i );
			if( element == null ) continue;
			if( element.HashID == i ) continue;
			_elementsToRemove.Add( i );
		}

		// if( _elementsToRemove.Count > 0 )
		// {
		// 	var strs = _elementsToRemove.ConvertAll<string>( ( id ) => {
		// 		var element = GetElementBase( id );
		// 		return $"[{id}] => {element.ToStringOrNull()}";
		// 	} );
		// 	Editor_Log.Add( $"Remove Wrong Elements:\n{string.Join( ",\n", strs )}" );
		// }

		for( int i = 0; i < _elementsToRemove.Count; i++ ) Editor_HACK_Remove( _elementsToRemove[i] );

		UnityEditor.EditorUtility.SetDirty( this );
	}

	void IHashedSOCollectionEditor.EditorCheckConsistency()
	{
//		Debug.Log($"<><>EditorCheckConsistency");
		( (IHashedSOCollectionEditor)this ).EditorRemoveWrongElements();

		var guids = AssetDatabase.FindAssets( $"t:{GetElementType().ToString()}" );

		for( int i = 0; i < guids.Length; i++ )
		{
			var path = AssetDatabase.GUIDToAssetPath( guids[i] );
			var asset = AssetDatabase.LoadAssetAtPath( path, GetElementType() );
			( (IHashedSOCollectionEditor)this ).EditorRequestMember( asset );
		}

		UnityEditor.EditorUtility.SetDirty( this );
	}

	bool IHashedSOCollectionEditor.EditorRequestMember( Object obj, bool forceCorrectPosition = false )
	{
		var t = obj as IHashedSO;
		if( t == null ) return false;

		int hashID = t.HashID;
		
		IHashedSO element = ( hashID < Count && hashID >= 0 ) ? GetElementBase( hashID ) : null;

			Debug.Log($"<><> element: {element}");
		if( element == null && hashID >= 0 )
		{
			Debug.Log($"<><> Callset 1");
			SetRealPosition( t );
			EditorRemoveDuplication( t );
			// Editor_Log.Add( $"Request Member:\nOn [{hashID}] set {t.ToStringOrNull()}, was NULL before" );
			return false;
		}

		var sameRef = ScriptableObject.ReferenceEquals( t, element );


		if( sameRef )
		{
			if( element.HashID != t.HashID || forceCorrectPosition )
			{
				// Editor_Log.Add( $"Request Member:\nOn [{hashID}] removed {element.ToStringOrNull()} and replace with {t.ToStringOrNull()}" );
			Debug.Log($"<><> Callset 2");
				//SetRealPosition( t );
			}
			EditorRemoveDuplication( t );
			return false;
		}

		var where = EditorWhereIs( t );
		if( where != -1 )
		{
			t.SetHashID( where );
			EditorRemoveDuplication( t );
			return false;
		}

		bool fromDialog = false;
		if( hashID < 0 || hashID >= Count || !sameRef )
		{
			var assetPath = AssetDatabase.GetAssetPath( (Object)t );
			var assetGuid = UnityEditor.AssetDatabase.AssetPathToGUID( assetPath );
			bool isDuplicateFromOtherFile = t.GUID != assetGuid;

			if( !isDuplicateFromOtherFile )
			{
				if( !ResolveConflictedFile( t, assetPath ) ) return false;
				fromDialog = !EditorCanChangeIDsToOptimizeSpace;
			}
		}

		var newID = Count;
		
		Debug.Log($"<><>Count: {Count}, newID: {newID}");

		// Editor_Log.Add( $"Request Member:\nOn [{newID}] setted {t.ToStringOrNull()} with new hashID{(fromDialog ? " with dialog permission" : "")}" );
		( (IHashedSOEditor)t ).SetHashID( newID );
		//AddElement( t );  //new  - Placeholder   -- Will not add to the old list anymore
		PlaceholderAddElement(t); //new  - Placeholder

		UnityEditor.EditorUtility.SetDirty( (Object)t );
		UnityEditor.EditorUtility.SetDirty( this );

		return true;
	}

	int EditorWhereIs( IHashedSO t )
	{
		var count = Count;
		for( int i = 0; i < count; i++ )
		{
			var e = GetElementBase( i );
			if( e == null ) continue;
			if( ScriptableObject.ReferenceEquals( t, e ) ) return i;
		}
		return -1;
	}

	void EditorRemoveDuplication( IHashedSO t ) //TODO check if need this
	{
		if( t == null ) return;
		var count = Count;
		for( int i = 0; i < Count; i++ )
		{
			var e = GetElementBase( i );
			if( e == null ) continue;
			if( i == t.HashID ) continue;
			if( !ScriptableObject.ReferenceEquals( t, e ) ) continue;
			Editor_HACK_Remove( i );
		}
	}

	void IHashedSOCollectionEditor.EditorTryOptimize()
	{
		Debug.Log("<><><><><  EditorTryOptimize: ");
		var editor = ( (IHashedSOCollectionEditor)this );
		if( !editor.EditorCanChangeIDsToOptimizeSpace ) return;

		var before = new List<string>();
		var after = new List<string>();

		for( int i = 0; i < Count; i++ ) before.Add( GetElementBase( i ).ToStringOrNull() );

		Clear();

		var guids = AssetDatabase.FindAssets( $"t:{GetElementType().ToString()}" );

		for( int i = 0; i < guids.Length; i++ )
		{
			var path = AssetDatabase.GUIDToAssetPath( guids[i] );
			var asset = AssetDatabase.LoadAssetAtPath( path, GetElementType() );
			if( asset is IHashedSO t )
			{
				( (IHashedSOEditor)t ).SetHashID( Count - 1 );
				//AddElement( t ); //new  - Placeholder   -- Will not add to the old list anymore
				PlaceholderAddElement(t);//new  - Placeholder
			}
		}

		for( int i = 0; i < Count; i++ ) after.Add( GetElementBase( i ).ToStringOrNull() );
		var logs = new List<string>();
		var count = Mathf.Min( before.Count, after.Count );
		for( int i = 0; i < count; i++ ) logs.Add( $"[{i}]\t\t=>\t\t{before[i]}\t\t=>\t\t{after[i]}" );
		for( int i = count; i < before.Count; i++ ) logs.Add( $"[{i}]\t\t=>\t\t{before[i]}\t\t=>\t\t-" );
		for( int i = count; i < after.Count; i++ ) logs.Add( $"[{i}]\t\t=>\t\t-\t\t=>\t\t{after[i]}" );

		// Editor_Log.Add( $"Collection Optimized:\n{string.Join( ",\n", logs )}" );
	}

	public abstract bool EditorCanChangeIDsToOptimizeSpace { get; }
	protected abstract void Clear();
	protected abstract void ClearPlaceholderList(); //new  - Placeholder
	protected abstract bool AddElement( IHashedSO obj );
	protected abstract bool PlaceholderAddElement( IHashedSO obj ); //new  - Placeholder
	protected abstract bool ResolveConflictedFile( IHashedSO t, string assetPath );
	public abstract bool TryResolveConflict( int i );
	protected abstract bool SetRealPosition( IHashedSO obj );
#endif
}