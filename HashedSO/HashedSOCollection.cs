using System;
using System.Collections.Generic;
using GitHub.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


[Serializable]
public struct Placeholder_struct<T>
{
	public int ID;
	public T ObjectInList;
}


public abstract class HashedSOCollection<T> : BaseHashedSOCollection, IEnumerable<T> where T : HashedScriptableObject
{
	//[Serializable] public class StringStringDictionary : SerializableDictionary<int, T> { }

	
	[FormerlySerializedAs("_list")][SerializeField] List<T> _listOld = new List<T>();
	[SerializeField] List<Placeholder_struct<T>> _list_placeholder = new List<Placeholder_struct<T>>();//new  - Placeholder
	
	public SerializableDictionary<int, T> objDic;	
	
	public T this[int index] => objDic[index];
	
	//public T GetPlaceholderObjectByIndex(int index) => _list_placeholder[index].ObjectInList;//new  - Placeholder
	public override int Count => objDic.Count;
	public override int PlaceholderCount => _list_placeholder.Count; //new  - Placeholder

	public T GetElement(int hashId) => this[hashId];
	public T PlaceholderGetElement(int hashId) => _list_placeholder.Find((x) => x.ID == hashId).ObjectInList; //new  - PlaceholderTODO: Change .Find because it is too heavy


	public T GetElementOrDefault(int hashId)
	{
		if (hashId >= 0 && hashId < objDic.Count) return this[hashId];
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

	public override IHashedSO GetElementBase(int hashId)
	{
		Debug.Log("<><>< hashId: "+hashId);
		return	objDic.GetValuesList()[hashId]; //   this[hashId];
	}
	
	public override IHashedSO PlaceholderGetElementBase( int hashId ) => _list_placeholder.Find((x) => x.ID == hashId).ObjectInList; //new  - Placeholder

	public override bool ContainsHashID(int hashID) => (hashID < objDic.Count || objDic[hashID] != null);
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


	public override bool Contains( IHashedSO obj ) => objDic.ContainsKey( (obj as T).HashID );

	public IEnumerator<T> GetEnumerator() => objDic.Values.GetEnumerator();  //TODO: VALIDATE FOR REAL

#if UNITY_EDITOR

	protected override void Clear() 
	{ 
		objDic.Clear();
		UnityEditor.EditorUtility.SetDirty( this );
	}
	protected override void ClearPlaceholderList() //new  - Placeholder
	{ 
		_list_placeholder.Clear();
		UnityEditor.EditorUtility.SetDirty( this );
	}
	public override bool EditorCanChangeIDsToOptimizeSpace => true;

	public override void Editor_HACK_Remove(int id){ objDic[id] = null; }

	protected override bool AddElement(IHashedSO obj)
	{
		if( obj is T t )
			objDic.Add( t.HashID, t );
		
		foreach(KeyValuePair<int,T> i in objDic)
		{
			//Now you can access the key and value both separately from this attachStat as:
			Debug.Log("<><><><>Key: "+i.Key+ " value: "+i.Value);
			Debug.Log(i.Value);
		}
		return ( obj is T );
	}
	
	protected override bool PlaceholderAddElement(IHashedSO obj) //new  - Placeholder
	{
		if (obj is T t)
		{
			Debug.Log($"<><>ADD ELEMENT: {obj}, HASHid: {obj.HashID}");
			var newStruct = new Placeholder_struct<T>();
			newStruct.ObjectInList = t;
			newStruct.ID = t.HashID;
			
			int newid = t.HashID +10;
			( (IHashedSOEditor)t ).SetHashID( newid);
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
		var element = objDic[i];
		var realId = element.HashID;
		if( realId == i ) return false;
		if( objDic[realId] != null ) return false;
		objDic[realId] = element;
		objDic[i] = null;
		UnityEditor.EditorUtility.SetDirty( this );
		return true;
	}
	protected override bool SetRealPosition( IHashedSO obj ) //TODO: why this change id
	{
		var t = obj as T;
		if( t == null ) return false;
		var id = obj.HashID;
		if( id < 0 ) return false;
		//while( id >= objDic.Count ) objDic.Add( null );
		objDic[id] = t;
		UnityEditor.EditorUtility.SetDirty( this );
		return true;
	}
	
#endif

	public override string ToString()// TODO: NEED FIX
	{
		string s = "";
		for( int i = 0; i < objDic.Count; i++ )
		{
			Debug.Log($"<><>PlaceholderCount Before : {PlaceholderCount}");
			var element = objDic[i];  // THIS SHOULD BE ID AND NOT POS
			if( element == null ) s += $"[{i}] => NULL\n";
			else s += $"[{i}] => {objDic[i].NameOrNull()}[{objDic[i].HashID}]\n";
		}
		return $"{this.GetType()}:\n{s}";

	}

	private int SetRandomID()
	{
		Debug.Log($"<><>PlaceholderCount Before : {PlaceholderCount}");
		return Random.Range(PlaceholderCount, int.MaxValue);
	
	}
}