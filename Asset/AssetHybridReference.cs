using UnityEngine;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

public enum EAssetReferenceType
{
    DirectReference = 0,
    Resources = 1,

#if USE_ADDRESSABLES
    Addressables = 2,
#endif
}

[System.Serializable]
public abstract class BaseAssetHybridReference
{
    public abstract void PreLoad();
    public abstract void DisposeAsset();
    public abstract UnityEngine.Object GetBaseReference();
#if UNITY_EDITOR
    public abstract System.Type EDITOR_GetAssetType();
    public abstract void EDITOR_UpdateDataFromRef( System.Func<UnityEngine.Object,bool> CheckIsAddressableFunc );
#endif //UNITY_EDITOR
}

[System.Serializable]
public class AssetHybridReference<T> : BaseAssetHybridReference where T : UnityEngine.Object
{
#if UNITY_EDITOR
    [SerializeField] string _editorAssetRefGuid;
    [SerializeField] T _assetHardReference;
    [SerializeField] EAssetReferenceState _referenceState = EAssetReferenceState.Empty;
#endif //UNITY_EDITOR
    [SerializeField] T _serializedDirectReference;
    T _assetRuntimeReference;
    [SerializeField] string _guid;
    [SerializeField] string _resourcesPath;
    [SerializeField] EAssetReferenceType _referenceType;
    public bool IsRefNull => _referenceType == EAssetReferenceType.Addressables ? string.IsNullOrEmpty( _guid ) : _referenceType == EAssetReferenceType.Resources ? string.IsNullOrEmpty( _resourcesPath ) : _serializedDirectReference == null;



	bool _loaded = false;
#if USE_ADDRESSABLES
	private AsyncOperationHandle<T> _addressableOp;
#endif //USE_ADDRESSABLES
	private ResourceRequest _resourcesLoadOp = null;

	public override void PreLoad()
    {
        if( _loaded || _assetRuntimeReference != null ) return;
#if UNITY_EDITOR
        _referenceState = EAssetReferenceState.Requested;
#endif //UNITY_EDITOR
        switch( _referenceType )
        {
            case EAssetReferenceType.DirectReference:
                _assetRuntimeReference = _serializedDirectReference;
#if UNITY_EDITOR
                _referenceState = EAssetReferenceState.Loaded;
#endif //UNITY_EDITOR
                _loaded = true;
                break;

            case EAssetReferenceType.Resources:
                _resourcesLoadOp = Resources.LoadAsync<T>( _resourcesPath );
                break;

#if USE_ADDRESSABLES
            case EAssetReferenceType.Addressables:
                _addressableOp = Addressables.LoadAssetAsync<T>( _guid );
                break;
#endif //USE_ADDRESSABLES
        }
    }
    
#if UNITY_EDITOR
    public override System.Type EDITOR_GetAssetType() => typeof(T);

    private void ResetData()
    {
            _referenceType = EAssetReferenceType.DirectReference;
            _assetHardReference = null;
            _serializedDirectReference = null;
#if USE_ADDRESSABLES
            _guid = string.Empty;
#endif //USE_ADDRESSABLES
            _resourcesPath = string.Empty;
            _editorAssetRefGuid = string.Empty;
    }

    public void UpdateOldRef()
    {
        if( _assetHardReference == null ) return;
        var path = UnityEditor.AssetDatabase.GetAssetPath( _assetHardReference );
        _editorAssetRefGuid = UnityEditor.AssetDatabase.AssetPathToGUID( path );
        _assetHardReference = null;
    }

    const string RESOURCES_PATH = "/Resources/";
    public override void EDITOR_UpdateDataFromRef( System.Func<UnityEngine.Object,bool> CheckIsAddressableFunc )
    {
        UpdateOldRef();

        if( string.IsNullOrWhiteSpace( _editorAssetRefGuid ) )
        {
            ResetData();
            return;
        }

        var path = UnityEditor.AssetDatabase.GUIDToAssetPath( _editorAssetRefGuid );
        var assetRef = UnityEditor.AssetDatabase.LoadAssetAtPath<T>( path );
#if USE_ADDRESSABLES
        _guid = string.Empty;
#endif

        var resourcesIndex = path.IndexOf( RESOURCES_PATH, System.StringComparison.OrdinalIgnoreCase );
        
        if( resourcesIndex != -1 )
        {
            var lastDot = path.Length - 1;
            for( ; lastDot >= 0 && path[lastDot] != '.'; lastDot-- ) { }
            var startId = resourcesIndex + RESOURCES_PATH.Length;
            var resourcePath = path.Substring( startId, lastDot - startId );

            _resourcesPath = resourcePath;
            _referenceType = EAssetReferenceType.Resources;
            _serializedDirectReference = null;
        }
#if USE_ADDRESSABLES
        else if( CheckIsAddressableFunc( assetRef ) )
        {
            _referenceType = EAssetReferenceType.Addressables;
            _serializedDirectReference = null;
            _resourcesPath = string.Empty;
            _guid = _editorAssetRefGuid;
        }
#endif
        else
        {
            _referenceType = EAssetReferenceType.DirectReference;
            _serializedDirectReference = assetRef;
            _resourcesPath = string.Empty;
        }
    }
#endif //UNITY_EDITOR

    public override void DisposeAsset()
    {
#if USE_ADDRESSABLES
        if( _addressableOp.IsValid() && _addressableOp.IsDone && _assetRuntimeReference == null ) _assetRuntimeReference = _addressableOp.Result;
        _addressableOp = default(AsyncOperationHandle<T>);
        if( _referenceType == EAssetReferenceType.Addressables && _assetRuntimeReference != null ) Addressables.Release<T>( _assetRuntimeReference );
#endif //USE_ADDRESSABLES
        _assetRuntimeReference = null;
        _loaded = false;
#if UNITY_EDITOR
        _referenceState = EAssetReferenceState.Empty;
#endif //UNITY_EDITOR
    }

    public override UnityEngine.Object GetBaseReference() => GetReference();

	public T GetReference()
    {
        if( !_loaded && _assetRuntimeReference == null )
        {
            switch( _referenceType )
            {
                case EAssetReferenceType.DirectReference:
                    _assetRuntimeReference = _serializedDirectReference;
                    break;

                case EAssetReferenceType.Resources:
                    if( _resourcesLoadOp == null || !_resourcesLoadOp.isDone )
                    {
                        _assetRuntimeReference = Resources.Load<T>( _resourcesPath );
                        if( _resourcesLoadOp != null ) Debug.Log( $"Ignored LoadAssetAsync operation from Resources because Load is call and operation is not ready for: {_resourcesPath} {_resourcesLoadOp}" );
                    }
                    else _assetRuntimeReference = (T)_resourcesLoadOp.asset;
                    break;
#if USE_ADDRESSABLES
                case EAssetReferenceType.Addressables:
                    if( !_addressableOp.IsValid() ) _addressableOp = Addressables.LoadAssetAsync<T>( _guid );
                    _addressableOp.WaitForCompletion();
                    _assetRuntimeReference = _addressableOp.Result;
                    break;
#endif //USE_ADDRESSABLES
            }
#if UNITY_EDITOR
            _referenceState = ( _assetRuntimeReference != null ) ? EAssetReferenceState.Loaded : EAssetReferenceState.LoadedNull;
#endif //UNITY_EDITOR
        }
        return _assetRuntimeReference;
    }
}