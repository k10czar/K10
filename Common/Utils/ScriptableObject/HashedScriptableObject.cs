using System;
using UnityEngine;

#if UNITY_EDITOR
public interface IHashedSOEditor
{
	string GUID { get; }
	void SetHashID( int newID );
}
#endif

public interface IExportIgnorable
{
	bool Ignore { get; }
}

public interface IHashedSO
#if UNITY_EDITOR
: IHashedSOEditor
#endif
{
	int HashID { get; }
	IHashedSOCollection GetCollection();
}

public abstract class HashedScriptableObject : ScriptableObject, IHashedSO, IExportIgnorable
{
	[SerializeField] private bool _ignoreFromExport = false;
	[HideInInspector, SerializeField] int _hashId = -1;

	#if UNITY_EDITOR
	[HideInInspector, SerializeField] string _guid;
	public string GUID { get { return _guid; } }
	#endif

	public int HashID { get { return _hashId; } }

    public abstract IHashedSOCollection GetCollection();

#if UNITY_EDITOR
	void IHashedSOEditor.SetHashID( int newID )
	{
		_hashId = newID;
		var path = UnityEditor.AssetDatabase.GetAssetPath( this );
		_guid = UnityEditor.AssetDatabase.AssetPathToGUID( path );
		UnityEditor.EditorUtility.SetDirty( this );
	}
#endif

	public override string ToString() => $"{name}[{HashID}]";
	public bool Ignore => _ignoreFromExport;
}

public static class HashedSOExtentions
{
	public static int HashIdOrNull( this IHashedSO hso ) => ( hso != null ) ? hso.HashID : -1;
}