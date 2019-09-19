using System;
using UnityEngine;

public interface IHashedSO
{
	int HashID { get; }
}

public abstract class HashedScriptableObject : ScriptableObject, IHashedSO
{
	[HideInInspector, SerializeField] int _hashId = -1;

	#if UNITY_EDITOR
	[HideInInspector, SerializeField] string _guid;
	#endif

	public int HashID { get { return _hashId; } }

    public abstract IHashedSOCollection GetCollection();

	#if UNITY_EDITOR
	public void CheckIntegrity()
	{
		var path = UnityEditor.AssetDatabase.GetAssetPath( this );
		var guid = UnityEditor.AssetDatabase.AssetPathToGUID( path );

		var col = GetCollection();
		if( guid != _guid )
		{
			col.RequestMember( this );

			_hashId = col.GetUnusedHashID();
			Debug.LogWarningFormat( "Now {0} has new hashID: {1}", name, _hashId );
			_guid = guid;
		}

		if( col.HashHasConflict( this ) ) _hashId = col.GetUnusedHashID();
	}
	#endif
    
	// public virtual void OnValidate()
	// {
	// 	if( Application.isPlaying ) return;
	//	CheckIntegrity()
	// }
}