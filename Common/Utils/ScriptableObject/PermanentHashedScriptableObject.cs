using UnityEngine;

#if UNITY_EDITOR
public interface IPermanentHashedScriptableObjectEditorView
{
	void UpdateGUID();
}
#endif

public abstract class PermanentHashedScriptableObject : ScriptableObject, IHashedSO
#if UNITY_EDITOR
, IPermanentHashedScriptableObjectEditorView
#endif
{
	[HideInInspector, SerializeField] int _hashId = -1;

#if UNITY_EDITOR
	[HideInInspector, SerializeField] string _guid;
	public string GUID { get { return _guid; } }
#endif

	public int HashID { get { return _hashId; } }

	public void SetHashID( int newID )
	{
		_hashId = newID;
#if UNITY_EDITOR
		if( !Application.isPlaying ) UnityEditor.EditorUtility.SetDirty( this );
#endif
	}

	public abstract IPermanentHashedSOCollection GetCollection();

#if UNITY_EDITOR
	void IPermanentHashedScriptableObjectEditorView.UpdateGUID()
	{
		var path = UnityEditor.AssetDatabase.GetAssetPath( this );
		var guid = UnityEditor.AssetDatabase.AssetPathToGUID( path );
		if( guid != _guid ) _guid = guid;
	}
#endif
}