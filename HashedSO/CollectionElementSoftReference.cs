using UnityEngine;


[System.Serializable]
public abstract class BaseCollectionElementSoftReference
{
    public abstract void DisposeAsset();
    public abstract IHashedSO GetBaseReference();
#if UNITY_EDITOR
    public abstract void EDITOR_UpdateDataFromRef();
    public abstract System.Type EDITOR_GetAssetType();
#endif //UNITY_EDITOR
}

[System.Serializable]
public class CollectionElementSoftReference<T> : BaseCollectionElementSoftReference where T : UnityEngine.ScriptableObject, IHashedSO
{
#if UNITY_EDITOR
    [SerializeField] string _editorAssetRefGuid;
    [SerializeField] T _assetHardReference;
    [SerializeField] EAssetReferenceState _referenceState = EAssetReferenceState.Empty;
#endif //UNITY_EDITOR
    T _assetRuntimeReference;
    [SerializeField] int _id;

	private static T _dummyInstance = null;
	private static T Dummy => _dummyInstance ?? ( _dummyInstance = ScriptableObject.CreateInstance<T>() );

	public override void DisposeAsset()
	{
		_assetRuntimeReference = null;
#if UNITY_EDITOR
		_referenceState = EAssetReferenceState.Empty;
#endif //UNITY_EDITOR
	}

#if UNITY_EDITOR
    public override System.Type EDITOR_GetAssetType() => typeof(T);

    public void UpdateOldRef()
    {
        if( _assetHardReference == null ) return;
        var path = UnityEditor.AssetDatabase.GetAssetPath( _assetHardReference );
        _editorAssetRefGuid = UnityEditor.AssetDatabase.AssetPathToGUID( path );
        _assetHardReference = null;
    }

	public override void EDITOR_UpdateDataFromRef()
	{
		_id = _assetHardReference?.HashID ?? -1;
	}
#endif //UNITY_EDITOR

	public override IHashedSO GetBaseReference() => GetReference();

	public T GetReference()
	{
		_assetRuntimeReference = (T)Dummy.GetCollection().GetElementBase( Mathf.Max( _id, 0 ) );
#if UNITY_EDITOR
		_referenceState = _assetRuntimeReference != null ? EAssetReferenceState.Loaded : EAssetReferenceState.LoadedNull;
#endif //UNITY_EDITOR
		return _assetRuntimeReference;
	}

#if UNITY_EDITOR
	public void SetReference( T t )
	{
		_assetHardReference = t;
		UpdateOldRef();
		GetReference();
		EDITOR_UpdateDataFromRef();
	}
#endif //UNITY_EDITOR
}
