using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public interface IHashedSOCollectionEditor
{
	void EditorTryOptimize();
	void EditorCheckConsistency();
	bool EditorRequestMember( Object t );
	bool EditorCanChangeIDsToOptimizeSpace { get; }
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

public abstract class BaseHashedSOCollection : ScriptableObject, IHashedSOCollection
{
	public abstract int Count { get; }
	public abstract IHashedSO GetElementBase( int index );
	public abstract bool Contains( IHashedSO element );
	public abstract bool ContainsHashID( int hashID );

	public abstract System.Type GetElementType();

#if UNITY_EDITOR
	void IHashedSOCollectionEditor.EditorCheckConsistency()
	{
		Debug.Log( $"Checking consistency of Collection {this.name} / t:{GetElementType().ToString()}" );
		var guids = AssetDatabase.FindAssets( $"t:{GetElementType().ToString()}" );
		for( int i = 0; i < guids.Length; i++ )
		{
			var path = AssetDatabase.GUIDToAssetPath( guids[i] );
			var asset = AssetDatabase.LoadAssetAtPath( path, GetElementType() );
			( (IHashedSOCollectionEditor)this ).EditorRequestMember( asset );
		}
	}
	
	bool IHashedSOCollectionEditor.EditorRequestMember( Object obj )
	{
		var t = obj as IHashedSO;
		if( t == null ) return false;

		if( Contains( t ) ) return false;
		else
		{
			int hashID = t.HashID;
			if( hashID < 0 || hashID >= Count || GetElementBase( hashID ) != t )
			{
				var assetPath = AssetDatabase.GetAssetPath( (Object)t );
				var assetGuid = UnityEditor.AssetDatabase.AssetPathToGUID( assetPath );
				bool isDuplicateFromOtherFile = t.GUID != assetGuid;

				if( !isDuplicateFromOtherFile )
				{
					if( !ResolveConflictedFile( t, assetPath ) ) return false;
				}
			}

			AddElement( t );
			( (IHashedSOEditor)t ).SetHashID( Count - 1 );

			EditorUtility.SetDirty( (Object)t );
			EditorUtility.SetDirty( this );
		}
		return true;
	}

	void IHashedSOCollectionEditor.EditorTryOptimize()
	{
		if( !( (IHashedSOCollectionEditor)this ).EditorCanChangeIDsToOptimizeSpace ) return;

		Clear();
		((IHashedSOCollectionEditor )this).EditorCheckConsistency();
	}
	public abstract bool EditorCanChangeIDsToOptimizeSpace { get; }
	protected abstract void Clear();
	protected abstract bool AddElement( IHashedSO obj );
	protected abstract bool ResolveConflictedFile( IHashedSO t, string assetPath );
#endif
}
