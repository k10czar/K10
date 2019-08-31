using System;
using UnityEngine;

public interface IPermanentHashedScriptableObjectEditorView
{
	void UpdateGUID();
}

public abstract class PermanentHashedScriptableObject : ScriptableObject, IPermanentHashedScriptableObjectEditorView
{
	[HideInInspector, SerializeField] bool _conflictFlag = false;
	public bool ConflictFlag => _conflictFlag;
	[HideInInspector, SerializeField] int _conflictedID = -1;
	[HideInInspector, SerializeField] int _hashId = -1;
#if UNITY_EDITOR
	[HideInInspector, SerializeField] string _guid;
	public string GUID { get { return _guid; } }
#endif

	public int HashID { get { return _hashId; } }

	public void SetHashID( int newID )
	{
		_hashId = newID;
	}

	public abstract IPermanentHashedSOCollection GetCollection();

	void IPermanentHashedScriptableObjectEditorView.UpdateGUID()
	{
#if UNITY_EDITOR
		var path = UnityEditor.AssetDatabase.GetAssetPath( this );
		var guid = UnityEditor.AssetDatabase.AssetPathToGUID( path );

		if( guid != _guid )
		{
			_guid = guid;
		}
#endif
	}
}