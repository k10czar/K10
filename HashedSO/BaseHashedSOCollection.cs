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
	IHashedSO GetElementBase( int hashID );
}

public abstract class BaseHashedSOCollection : ScriptableObject, IHashedSOCollection, System.Collections.IEnumerable
{

	public abstract int Count { get; }
	public abstract IHashedSO GetElementBase( int index );
	public abstract bool Contains( IHashedSO element );
	public abstract bool ContainsHashID( int hashID );

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

	void IHashedSOCollectionEditor.Editor_HACK_EnforceHashIDs() //TODO CHECK IF IT WILL WORK
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

				//if( element.HashID != j )
				//{
				//	Editor_HACK_Remove( j );
				//	j--;
				//}
				
				if(element.HashID != GetElementBase(j).HashID )
				{
				//	Editor_HACK_Remove( j ); 
					Editor_HACK_Remove( element.HashID  ); //TODO VERIFY IF THIS MAKE SENSE
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

	void IHashedSOCollectionEditor.EditorRemoveWrongElements()//TODO: understand this
	{
		List<IHashedSO> elementsToRemove = new List<IHashedSO>();
		
		
		for( int i = 0; i < Count; i++ )
		{
			var element = GetElementBase( i ); // this should be the key and not position ???
			if (element == null) continue;

			if( element.HashID == GetElementBase(i).HashID ) continue;
			elementsToRemove.Add( element );
		}
		Debug.Log($"<><>< OBJETOS PARA REMOVER {elementsToRemove.Count}");
		
		for( int i = 0; i < elementsToRemove.Count; i++ ) Editor_HACK_Remove(GetDicIDbyElement(elementsToRemove[i])  );
	//	for( int i = 0; i < _elementsToRemove.Count; i++ ) Editor_HACK_Remove( _elementsToRemove[i].HashID );
		

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
		
		
//		Debug.Log($"!!!!!<><<><>< sera?: " );
		var t = obj as IHashedSO;
		if (t == null) return false;


//			Debug.Log($"!!!!!<><<><><  foi: " );
		
		int hashID = t.HashID;

		IHashedSO element = null;

		if (!DicHasIDKey(hashID))
		{
		//	Debug.Log($"???<><<><>< NAO TEM ID {hashID}  name: {t.ToString()} ");
			AddElement(t);
			UnityEditor.EditorUtility.SetDirty( (Object)t );
			UnityEditor.EditorUtility.SetDirty( this );
			return true;
		}
		else
		{
			//Debug.Log($"???<><<><>< JÁ TEM ID {hashID}  name: {t.ToString()} Count: {Count} " );
			
			element = GetElementeByKey(hashID);
			var isSame = ScriptableObject.ReferenceEquals( t, element );
			if(isSame)	return false;
			
			var randID = hashID;

			while (DicHasIDKey(randID))
			{
				randID = Random.Range(Count+1, int.MaxValue-1); //Create unique ID based on in.maxvalue
			}
		
			( (IHashedSOEditor)t ).SetHashID( randID );
	
			Debug.Log($"???<><<><>< JÁ TROCOU ID {hashID}  name: {t.ToString()} ");
			AddElement( t );
			UnityEditor.EditorUtility.SetDirty( (Object)t );
			UnityEditor.EditorUtility.SetDirty( this );
			return true;

		}
		
		if (hashID < Count && hashID >= 0)
		{
			//Debug.Log($"!!!!!<><<><>< PEGOU ELEMENTO " );
			//element = GetElementBase(hashID);
			//Debug.Log($"!!!!!<><<><>< PEGOU ELEMENTO {element} ID: {element.HashID}" );
		}
		
		else
		{
		//	Debug.Log($"!!!!!<><<><>< SETOU ELEMENTO NULL " );
		//	element = null;
		}
		
		Debug.Log("!!!!!<><<><><  element: "+element + " -hashID: "+hashID);
	
	
		if( element == null && hashID >= 0 )
		{
///												//Debug.Log($"!!!!!<><<><>< VAI SETAR POS " );
		
			//SetRealPosition( t );
			//Debug.Log($"!!!!!<><<><>< SETOU POS " );
		    //
			//EditorRemoveDuplication( t );
		    //
			//// Editor_Log.Add( $"Request Member:\nOn [{hashID}] set {t.ToStringOrNull()}, was NULL before" );
			//return false;
		}


		var sameRef = ScriptableObject.ReferenceEquals( t, element );
		
		if( sameRef )
		{
		
			//if( element.HashID != t.HashID || forceCorrectPosition )
			//{
			////	Debug.Log("GGGGGG");
			//	// Editor_Log.Add( $"Request Member:\nOn [{hashID}] removed {element.ToStringOrNull()} and replace with {t.ToStringOrNull()}" );
			//
			//	SetRealPosition( t );
			//}
			//EditorRemoveDuplication( t );
			//return false;
		}

	//	var where = EditorWhereIs( t );
	//	if( where != -1 )
	//	{
	//			Debug.Log("HHHHHHH");
			//t.SetHashID( where ); //TODO: check all flow that send to SET HASH-ID
			//EditorRemoveDuplication( t );
			//return false;
	//	}
		//		Debug.Log("IIIIIII");

		bool fromDialog = false;
		if( hashID < 0 || hashID >= Count || !sameRef )
		{
		//		Debug.Log("JJJJJ");
			var assetPath = AssetDatabase.GetAssetPath( (Object)t ); //todo ENTENDER O PORQUE DISSO USANDO DIALOG
			var assetGuid = UnityEditor.AssetDatabase.AssetPathToGUID( assetPath );
			bool isDuplicateFromOtherFile = t.GUID != assetGuid;

			if( !isDuplicateFromOtherFile )
			{
			//	Debug.Log("KKKKK");
				if( !ResolveConflictedFile( t, assetPath ) ) return false;
			//	Debug.Log("LLLLLL");
				fromDialog = !EditorCanChangeIDsToOptimizeSpace;
			}
		}
				

	//	var newID = Count;
	//	
	//	Debug.Log($"<><>Count: {Count}, newID: {newID}");
	  //
	//	// Editor_Log.Add( $"Request Member:\nOn [{newID}] setted {t.ToStringOrNull()} with new hashID{(fromDialog ? " with dialog permission" : "")}" );
	//	( (IHashedSOEditor)t ).SetHashID( newID );
	//	Debug.Log("<><><><>Entrou2");
	//	AddElement( t ); 

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
		Debug.Log($"!!!!!<><<><>< t {t.ToString()}.HashID {t.HashID} " );
		
//		Debug.Log("BATATA");
		if( t == null ) return;
		Debug.Log($"!!!!!<><<><><  t {t.ToString()} NAO É NULL " );
		var count = Count;
		Debug.Log($"!!!!!<><<><><  count {Count}  " );
		for( int i = 0; i < Count; i++ )
		{
			
			var e = GetElementBase( i );
		
			if( e == null ) continue;
		
			//if( i == t.HashID ) continue;  //TODO: this check should not check with i
			if( e.HashID == t.HashID ) continue;  

			if( !ScriptableObject.ReferenceEquals( t, e ) ) continue;
		
			//Editor_HACK_Remove( i );
			Editor_HACK_Remove( e.HashID );
		
		}
	}

	void IHashedSOCollectionEditor.EditorTryOptimize()
	{
		//Debug.Log("<><><><><  EditorTryOptimize: ");
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
				Debug.Log("<><><><> VOU ADICIONAR ELEMENTO COM ID BASEADO NO COUNT");
				( (IHashedSOEditor)t ).SetHashID( Count - 1 );
				AddElement( t ); 
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
	protected abstract bool AddElement( IHashedSO obj );
	protected abstract IHashedSO GetElementeByKey(int hashID);
	public abstract int GetDicIDbyElement(IHashedSO element);
	public abstract List<IHashedSO> CheckNullInDic();
	protected abstract bool DicHasIDKey( int hasID );
	protected abstract bool ResolveConflictedFile( IHashedSO t, string assetPath );
	public abstract bool TryResolveConflict( int i );
	protected abstract bool SetRealPosition( IHashedSO obj );
#endif
}