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
	void EditorCheckConsistency();
	bool TryResolveConflict( int i );
	bool EditorRequestMember( Object t, bool forceCorrectPosition = false );
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
	IHashedSO GetElementBase( int hashID );
}

public abstract class BaseHashedSOCollection : ScriptableObject, IHashedSOCollection, System.Collections.IEnumerable
{
	public abstract int Count { get; }
	public abstract IHashedSO GetElementBase( int index );
	public abstract bool Contains( IHashedSO element );
	public abstract bool ContainsHashID( int hashID );
	protected abstract IEnumerable<int> Editor_HACK_GetKeysCollection();

	protected abstract IEnumerator GetNonGenericEnumerator();
	IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetNonGenericEnumerator();

	public abstract System.Type GetElementType();

#if UNITY_EDITOR
	public abstract void Editor_HACK_Remove( int id );

	void IHashedSOCollectionEditor.Editor_HACK_EnforceHashIDs() //TODO CHECK IF IT WILL WORK
	{
		HashSet<int> alreadyTakenId = new HashSet<int>();
		HashSet<IHashedSO> alreadyVisited = new HashSet<IHashedSO>();
		List<IHashedSO> allElements = new List<IHashedSO>();
		List<IHashedSO> conflictedElements = new List<IHashedSO>();
		
		foreach( var obj in this )
		{
			var iterated = obj as IHashedSO;
			if( iterated == null ) continue;
			var id = iterated.HashID;
			if( alreadyVisited.Contains( iterated ) ) continue;
			allElements.Add( iterated );
			if( alreadyTakenId.Contains( id ) ) 
			{
				conflictedElements.Add( iterated );
			}
			alreadyTakenId.Add( id );
		}
		
		foreach( var iterated in conflictedElements )
		{
			var newId = -1;
			var baseValue = UnityEngine.Random.Range(0, int.MaxValue);
			for( int i = baseValue; i < int.MaxValue && newId == -1; i++ )
			{
				if( alreadyTakenId.Contains( i ) ) continue;
				newId = i;
			}
			if( newId == -1 )
			{
				for( int i = baseValue; i > int.MinValue && newId == -1; i-- )
				{
					if( alreadyTakenId.Contains( i ) ) continue;
					newId = i;
				}
				if( newId == -1 )
				{
					Debug.LogError( "Cannot find new ID, all id already taken" );
				}
			}
			iterated.SetHashID( newId );
			UnityEditor.EditorUtility.SetDirty( (Object)iterated );
		}

		Clear();

		foreach( var iterated in allElements ) AddElement( iterated );

		UnityEditor.EditorUtility.SetDirty( this );
	}
	

	protected int GetNewHashId()
	{
		var newId = -1;
		var baseValue = UnityEngine.Random.Range( 0, int.MaxValue );
		for( int i = baseValue; i < int.MaxValue && newId == -1; i++ )
		{
			if( ContainsHashID( i ) ) continue;
			newId = i;
		}
		if( newId == -1 )
		{
			for( int i = baseValue; i > int.MinValue && newId == -1; i-- )
			{
				if( ContainsHashID( i ) ) continue;
				newId = i;
			}
			if( newId == -1 )
			{
				Debug.LogError( "Cannot find new ID, all id already taken" );
			}
		}
		return newId;
	}

	void IHashedSOCollectionEditor.EditorRemoveWrongElements()//TODO: understand this
	{
		List<int> elementsToRemove = new List<int>();
		
		foreach( var key in Editor_HACK_GetKeysCollection() )
		{
			var element = GetElementBase( key );
			if( key == element.HashID ) continue;
			elementsToRemove.Add( key );
		}
		Debug.Log($"<><>< OBJETOS PARA REMOVER {elementsToRemove.Count}");
		
		for( int i = 0; i < elementsToRemove.Count; i++ ) Editor_HACK_Remove( elementsToRemove[i] );		

		UnityEditor.EditorUtility.SetDirty( this );
	}

	void IHashedSOCollectionEditor.EditorCheckConsistency()
	{
		
		( (IHashedSOCollectionEditor)this ).EditorRemoveWrongElements();

		var guids = AssetDatabase.FindAssets( $"t:{GetElementType().ToString()}" );
		
		for( int i = 0; i < guids.Length; i++ )
		{
		//	Debug.Log($"!!!!!<><<><>< inicioLoop: " );
			var path = AssetDatabase.GUIDToAssetPath( guids[i] );
			var asset = AssetDatabase.LoadAssetAtPath( path, GetElementType() );
			( (IHashedSOCollectionEditor)this ).EditorRequestMember( asset );
		}
		//	Debug.Log($"!!!!!<><<><>< tchau: " );

		UnityEditor.EditorUtility.SetDirty( this );
	}

	bool IHashedSOCollectionEditor.EditorRequestMember( Object obj, bool forceCorrectPosition = false )
	{
		var element = obj as IHashedSO;
		if( element == null ) 
		{
			Debug.LogError($"Cannot add element to the collection {name} there is not IHashedSO");
			return false;
		}
		
		int hashID = element.HashID;
		var query = GetElementBase( hashID );

		if( query == element ) return true;

		if( forceCorrectPosition )
		{
			Editor_HACK_Remove( hashID );
			AddElement( element );

			var newHashId = GetNewHashId();
			query.SetHashID( newHashId );
			AddElement( query );
		}
		else
		{
			var newHashId = GetNewHashId();
			element.SetHashID( newHashId );
			AddElement( element );
		}
		return true;
	}

	protected abstract void Clear();
	protected abstract bool AddElement( IHashedSO obj );
	public abstract int GetDicIDbyElement(IHashedSO element);
	protected abstract bool ResolveConflictedFile( IHashedSO t, string assetPath );
	public abstract bool TryResolveConflict( int i );
	protected abstract bool SetRealPosition( IHashedSO obj );
#endif
}