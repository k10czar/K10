using UnityEngine;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

public enum EAssetLoadType
{
    Resources,

#if USE_ADDRESSABLES
    Addressables,
#endif
}

[System.Serializable]
public class AssetSoftReference<T> where T : UnityEngine.Object
{
#if UNITY_EDITOR
    [SerializeField] T _assetHardReference;
#endif //UNITY_EDITOR
    T _assetRuntimeReference;
    [SerializeField] string _assetDataRefCode;
    [SerializeField] EAssetLoadType _load;

    T Load()
    {
        if( _assetRuntimeReference == null )
        {
            switch( _load )
            {
                case EAssetLoadType.Resources:
                    _assetRuntimeReference = Resources.Load<T>( _assetDataRefCode );
                    break;
#if USE_ADDRESSABLES
                case EAssetLoadType.Addressables:
                    // _assetRuntimeReference = Addressables.Load<T>( _assetDataRefCode );
                    break;
#endif
            }
        }
        return _assetRuntimeReference;
    }
}