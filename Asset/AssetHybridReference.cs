using UnityEngine;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

public enum EAssetReferenceType
{
    DirectReference,
    Resources,

#if USE_ADDRESSABLES
    Addressables,
#endif
}

public enum EAssetReferenceState
{
    Empty,
    Requested,
    Loaded,
    LoadedNull,
}

[System.Serializable]
public abstract class BaseAssetHybridReference
{
    public abstract void PreLoad();
    public abstract void DisposeAsset();
    public abstract UnityEngine.Object GetReference();
}

[System.Serializable]
public class AssetHybridReference<T> : BaseAssetHybridReference where T : UnityEngine.Object
{
#if UNITY_EDITOR
    [SerializeField] T _assetHardReference;
#endif //UNITY_EDITOR
    [SerializeField] T _serializedDirectReference;
    T _assetRuntimeReference;
    [SerializeField] string _guid;
    [SerializeField] string _resourcesPath;
    [SerializeField] EAssetReferenceType _referenceType;
    [SerializeField] EAssetReferenceState _referenceState = EAssetReferenceState.Empty;
    
    
    bool _loaded = false;
#if USE_ADDRESSABLES
	private AsyncOperationHandle<T> _addressableOp;
#endif
	private ResourceRequest _resourcesLoadOp = null;

	public override void PreLoad()
    {
        if( _loaded || _assetRuntimeReference != null ) return;
        _referenceState = EAssetReferenceState.Requested;
        switch( _referenceType )
        {
            case EAssetReferenceType.DirectReference:
                _assetRuntimeReference = _serializedDirectReference;
                _referenceState = EAssetReferenceState.Loaded;
                _loaded = true;
                break;

            case EAssetReferenceType.Resources:
                _resourcesLoadOp = Resources.LoadAsync<T>( _resourcesPath );
                break;

#if USE_ADDRESSABLES
            case EAssetReferenceType.Addressables:
                _addressableOp = Addressables.LoadAssetAsync<T>( _guid );
                break;
#endif
        }
    }

    public override void DisposeAsset()
    {
#if USE_ADDRESSABLES
        if( _addressableOp.IsValid() && _addressableOp.IsDone && _assetRuntimeReference == null ) _assetRuntimeReference = _addressableOp.Result;
        _addressableOp = default(AsyncOperationHandle<T>);
        if( _referenceType == EAssetReferenceType.Addressables && _assetRuntimeReference != null ) Addressables.Release<T>( _assetRuntimeReference );
#endif
        _assetRuntimeReference = null;
        _loaded = false;
        _referenceState = EAssetReferenceState.Empty;
    }

    public override UnityEngine.Object GetReference() => Load();

	public T Load()
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
#endif
            }
            _referenceState = ( _assetRuntimeReference != null ) ? EAssetReferenceState.Loaded : EAssetReferenceState.LoadedNull;
        }
        return _assetRuntimeReference;
    }
}