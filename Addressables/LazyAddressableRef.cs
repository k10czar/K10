#if ADDRESSABLES
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LazyAddressableRef<T> where T : Object
{
	readonly string _address;
	AsyncOperationHandle<T> _handle;
	bool _isLoading;
	bool _loaded;
	T _asset;

	public LazyAddressableRef( string address ) => _address = address;

	public bool IsLoaded => _loaded;

	public void Preload()
	{
		if( _loaded || _isLoading ) return;
		_isLoading = true;
		_handle = Addressables.LoadAssetAsync<T>( _address );
		_handle.Completed += OnCompleted;
	}

	public T Asset
	{
		get
		{
			if( _loaded ) return _asset;
			
			if( !_isLoading )
			{
				_isLoading = true;
				_handle = Addressables.LoadAssetAsync<T>( _address );
				_handle.Completed += OnCompleted;
			}
			_handle.WaitForCompletion();
			return _asset;
		}
	}

	public void Release()
	{
		if( !_handle.IsValid() ) return;
		Addressables.Release( _handle );
		_asset = null;
		_loaded = false;
		_isLoading = false;
	}

	private void OnCompleted( AsyncOperationHandle<T> handle )
	{
		_isLoading = false;
		if( handle.Status == AsyncOperationStatus.Succeeded )
		{
			_asset = handle.Result;
			_loaded = true;
		}
		else
		{
			Debug.LogError( $"[LazyAddressableRef] Failed to load '{_address}': {handle.OperationException}" );
		}
	}
}
#endif