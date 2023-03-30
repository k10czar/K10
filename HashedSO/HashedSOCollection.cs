using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


[Serializable]
public struct Placeholder_struct<T>
{
	public int ID;
	public T ObjectInList;
}

public abstract class HashedSOCollection<T> : BaseHashedSOCollection, IEnumerable<T> where T : HashedScriptableObject
{
	[SerializeField] List<T> _list = new List<T>();
	[SerializeField] List<Placeholder_struct<T>> _list_placeholder = new List<Placeholder_struct<T>>();//new  - Placeholder

	public T this[int index] => _list[index];
	
	//public T GetPlaceholderObjectByIndex(int index) => _list_placeholder[index].ObjectInList;//new  - Placeholder
	public override int Count => _list.Count;
	public override int PlaceholderCount => _list_placeholder.Count; //new  - Placeholder

	public T GetElement(int hashId) => this[hashId];
	public T PlaceholderGetElement(int hashId) => _list_placeholder.Find((x) => x.ID == hashId).ObjectInList; //new  - PlaceholderTODO: Change .Find because it is too heavy


	public T GetElementOrDefault(int hashId)
	{
		if (hashId >= 0 && hashId < _list.Count) return this[hashId];
		return default(T);
	}
	public T PlaceholderGetElementOrDefault(int hashId) //new  - Placeholder
	{
		if (hashId >= 0)
		{
			return PlaceholderGetElement(hashId);
		}
		return default(T);
	}
	

	public override System.Type GetElementType() => typeof(T);
	public override IHashedSO GetElementBase(int hashId) => this[hashId];
	public override IHashedSO PlaceholderGetElementBase( int hashId ) => _list_placeholder.Find((x) => x.ID == hashId).ObjectInList; //new  - Placeholder

	public override bool ContainsHashID(int hashID) => (hashID < _list.Count || _list[hashID] != null);
	public override bool PlaceHolderContainsHashID(int hashID)//new  - Placeholder
	{
		
		var result = _list_placeholder.Find((x) => x.ID == hashID);
		if (result.ObjectInList != null)
		{
			Debug.Log("<><><> Found an item with the given ID");
			return true;
		}
		else
		{
			Debug.Log("<><><> No item with the given ID was found");
			return false;
		}
	
	}  


	public override bool Contains( IHashedSO obj ) => _list.Contains( obj as T );

	public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

#if UNITY_EDITOR
	protected override void Clear() 
	{ 
		_list.Clear();
		UnityEditor.EditorUtility.SetDirty( this );
	}
	protected override void ClearPlaceholderList() //new  - Placeholder
	{ 
		_list_placeholder.Clear();
		UnityEditor.EditorUtility.SetDirty( this );
	}
	public override bool EditorCanChangeIDsToOptimizeSpace => true;

	public override void Editor_HACK_Remove(int id){ _list[id] = null; }
	
	protected override bool AddElement( IHashedSO obj ) { if( obj is T t ) _list.Add( t ); return ( obj is T ); }
	protected override bool PlaceholderAddElement(IHashedSO obj) //new  - Placeholder
	{
		if (obj is T t)
		{
			Debug.Log($"<><>ADD ELEMENT: {obj}, HASHid: {obj.HashID}");
			var newStruct = new Placeholder_struct<T>();
			newStruct.ObjectInList = t;
			newStruct.ID = t.HashID;
			
			//( (IHashedSOEditor)t ).SetHashID( SetRandomID());
			//Debug.Log($"<><>COUNT AFTER: {PlaceholderCount}");

		
			newStruct.ID = t.HashID;
			
			
			_list_placeholder.Add(newStruct);
		}

		return ( obj is T );
	}

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
		if( id < 0 ) return false;
		while( id >= _list.Count ) _list.Add( null );
		_list[id] = t;
		UnityEditor.EditorUtility.SetDirty( this );
		return true;
	}
	
#endif

	public override string ToString()
	{
		string s = "";
		for( int i = 0; i < _list.Count; i++ )
		{
			var element = _list[i];
			if( element == null ) s += $"[{i}] => NULL\n";
			else s += $"[{i}] => {_list[i].NameOrNull()}[{_list[i].HashID}]\n";
		}
		return $"{this.GetType()}:\n{s}";
	}

	private int SetRandomID()
	{
		Debug.Log($"<><>PlaceholderCount Before : {PlaceholderCount}");
		return Random.Range(PlaceholderCount, int.MaxValue);
	
	}
}