using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IPermanentHashedSOCollection
{
	bool ContainsHashID( int hashID );
	bool Contains( PermanentHashedScriptableObject obj );
	int Count { get; }
	PermanentHashedScriptableObject GetElementBase( int index );

	bool EditorRequestMember( object t, ref int id );
	void EditorCheckConsistency();
}

public abstract class PermanentHashedSOBaseCollection : ScriptableObject, IPermanentHashedSOCollection
{
	[SerializeField] protected int _lastIDGiven;

	public abstract int Count { get; }
	public abstract PermanentHashedScriptableObject GetElementBase( int index );
	public abstract bool Contains( PermanentHashedScriptableObject element );
	public abstract bool ContainsHashID( int hashID );

	public abstract void EditorCheckConsistency();
	public abstract bool EditorRequestMember( object obj, ref int id );
}

public class PermanentHashedSOCollection<T> : PermanentHashedSOBaseCollection where T : PermanentHashedScriptableObject
{
	//TO DO: cache HashID to ID relations if needed for performance
	[SerializeField] List<T> _list = new List<T>();

	public T this[int index] => _list[index];
	public override int Count => _list.Count;

	public override PermanentHashedScriptableObject GetElementBase( int index ) => this[index];

	public override bool ContainsHashID( int hashID ) => ( hashID < _list.Count || _list[hashID] != null );
	public override bool Contains( PermanentHashedScriptableObject obj ) => _list.Contains( obj as T );

	public override void EditorCheckConsistency()
	{
#if UNITY_EDITOR
		Debug.Log( "Checking consistency of Collection " + this.name + " / " + $"t:{typeof( T ).ToString()}" );
		var guids = AssetDatabase.FindAssets( $"t:{typeof( T ).ToString()}" );
		for( int i = 0; i < guids.Length; i++ )
		{
			var path = AssetDatabase.GUIDToAssetPath( guids[i] );
			var asset = AssetDatabase.LoadAssetAtPath<T>( path );
			int id = 0;
			EditorRequestMember( asset, ref id );
		}
#endif
	}

	public override bool EditorRequestMember( object obj, ref int id )
	{
#if UNITY_EDITOR
		var t = obj as T;
		if( t == null ) return false;

		if( _list.Contains( t ) )
		{
			Debug.Log( "Trying to add " + t.name + " but it is already on the PermanentHashedSOCollection. [" + name + "]" );
			return false;
		}
		else
		{
			int hashID = t.HashID;
			if( hashID < 0 || hashID >= _list.Count || _list[hashID] != t )
			{
				var assetPath = AssetDatabase.GetAssetPath( t );
				var assetGuid = UnityEditor.AssetDatabase.AssetPathToGUID( assetPath );
				bool isDuplicate = t.GUID != assetGuid;

				if( !isDuplicate )
				{
					if( !UnityEditor.EditorUtility.DisplayDialog( "Conflict on PermanentHashedSOCollection", string.Format( "{0} already has an ID but it's not on the appropriate list ({1}). Assign a new ID for it?", t.name, this.name ), "Yes", "No, delete it" ) )
					{
						AssetDatabase.DeleteAsset( assetPath );
						return false;
					}
				}
			}

			_list.Add( t );
			_lastIDGiven = _list.Count - 1;
			id = _lastIDGiven;

			t.SetHashID( id );
			( (IPermanentHashedScriptableObjectEditorView)t ).UpdateGUID();

			EditorUtility.SetDirty( t );
			EditorUtility.SetDirty( this );
		}
#endif
		return true;
	}
}