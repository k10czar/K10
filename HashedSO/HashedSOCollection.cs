using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;





public abstract class HashedSOCollection<T> : BaseHashedSOCollection, IEnumerable<T> where T : HashedScriptableObject
{
	//[Serializable] public class StringStringDictionary : SerializableDictionary<int, T> { }

	
	[FormerlySerializedAs("_list")][SerializeField] List<T> _listOld = new List<T>();
	
	[SerializeField]
	private SerializableDictionary<int, T> objDic;	
	
	
	public T this[int hashId] => GetElementOrDefault( hashId );
	public override int Count => objDic.Count;

	public T GetElement(int hashId) => this[hashId];  //TODO: SEE IF NEED TO CREATE A METHOD TO GET BY ID

	public T GetRandomElement() { return objDic.GetRandomValue(); }

	//public T GetElement(int hashId) => objDic[hashId];

	public override int GetDicIDbyElement(IHashedSO element)
	{
		return objDic.FirstOrDefault(x => x.Value == element).Key;
	}

	public T GetElementOrDefault(int hashId)
	{
		if( objDic.TryGetValue( hashId, out var t ) ) return t;
		return default(T);
	}

	public override System.Type GetElementType() => typeof(T);

	public override IHashedSO GetElementBase(int hashId)
	{
		//Debug.Log("<><>< hashId: "+hashId + " Count: "+objDic.GetValuesList().Count);

		//if (hashId >= objDic.GetValuesList().Count) return null;
		//if (!ContainsHashID(hashId)) return null;
		//return	objDic.GetValuesList()[hashId]; //   this[hashId];

		if (!ContainsHashID(hashId))
		{
			Debug.LogError($" HASH ID DO NOT CONTAINS ON DICTIONARY, WILL RETURN NULL - Check why it is asking for this ID");
			return null;
		}
		
		return objDic[hashId];
	}

	//public override bool ContainsHashID(int hashID) => (hashID < objDic.Count || objDic[hashID] != null);

	public override bool ContainsHashID(int hashID) => TryGetElement( hashID, out var element ) && element != null;
	public bool TryGetElement(int hashID, out T element) => objDic.TryGetValue(hashID, out element);
	public override bool Contains( IHashedSO obj ) => obj != null && TryGetElement( obj.HashID, out var element ) && element == obj;


	protected override IEnumerable<int> Editor_HACK_GetKeysCollection() => objDic.Keys;
	protected override System.Collections.IEnumerator GetNonGenericEnumerator() => objDic.Values.GetEnumerator();
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => objDic.Values.GetEnumerator();
	

#if UNITY_EDITOR

	protected override void Clear() 
	{ 
		objDic.Clear();
		UnityEditor.EditorUtility.SetDirty( this );
	}

	public override void Editor_HACK_Remove(int id)
	{
	//	objDic[id] = null;
		Debug.Log($"<><><><> HACK REMOVE ID: {id}");// {obj.ToString()} id: {obj.HashID}");

		objDic.Remove(id);
	}

	protected override bool AddElement(IHashedSO obj)
	{
		if( obj is T t )
			objDic.Add( t.HashID, t );

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
//		Debug.Log("!!!!!TRYING ADDING NULL" );
		var t = obj as T;
		if( t == null ) return false;
		var id = obj.HashID;
		if( id < 0 ) return false;
		Debug.Log($"!!!!!ADICIONAR SetRealPosition obj: {obj.ToString()} id: {id}, objDic.Count: {objDic.Count} " );
		while (id >= objDic.Count)
		{
	//	Debug.Log($"!!!!!<>< while id: {id}, >= objDic.Count: {objDic.Count} " );
			objDic.Add( Random.Range(-10000000,-1),null);
		}
		objDic[id] = t;
		UnityEditor.EditorUtility.SetDirty( this );
		
		return true;
	}
	
#endif

	public override string ToString()// TODO: NEED FIX
	{
		//string s = "";
		//for( int i = 0; i < objDic.Count; i++ )
		//{
		//	
		//	//var element = objDic[i];  // THIS SHOULD BE ID AND NOT POS
		//	if( element == null ) s += $"[{i}] => NULL\n";
		//	else s += $"[{i}] => {objDic[i].NameOrNull()}[{objDic[i].HashID}]\n";
		//}
		//return $"{this.GetType()}:\n{s}";

		
		//string s = "";
		//for( int i = 0; i < objDic.Count; i++ )
		//{
		//	Debug.Log($"<><>Dicionario count : {objDic.GetValuesList().Count}  objdic count: {objDic.Count}");
		//	var element = GetElementBase(i);// objDic.GetValuesList()[i];  // THIS SHOULD BE ID AND NOT POS
		//	if( element == null ) s += $"[{i}] => NULL\n";
		//	else s += $"[{i}] => {objDic.GetValuesList()[i].NameOrNull()}[{objDic.GetValuesList()[i].HashID}]\n";
		//}
		//return $"{this.GetType()}:\n{s}";

		string s = "";
		int currentElementIteration = 0;
		foreach (KeyValuePair<int,T> entry in objDic)
		{
//			Debug.Log($"<><>Quantidade de values count : {objDic.GetValuesList().Count}  objdic count: {objDic.Count}");
			var element = entry.Value;
			if (element == null)

			{
				Debug.Log("ACHOU NULL.");
				s += $"[{currentElementIteration}] => NULL\n";
			}
			else s += $"[{currentElementIteration}] => {element.NameOrNull()}[{element.HashID}]\n";

			currentElementIteration++;
		}
		//string s = "";
		//s += $"NULL\n";
		return $"{this.GetType()}:\n{s}";
	}
}