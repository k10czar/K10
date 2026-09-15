#if ADDRESSABLES
#if CODE_METRICS
#define DEBUG_NOTIFY
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public interface ILazyAddressableRef
{
	bool IsLoaded { get; }
	Awaitable PreloadAsync();
}

public class LazyAddressableRef<T> : ILazyAddressableRef where T : Object
{
	readonly string _address;
	AsyncOperationHandle<T> _handle;
	bool _isLoading;
	bool _loaded;
	T _asset;
#if UNITY_EDITOR
	T _safeEditorAsset;
#endif

	public LazyAddressableRef( string address ) => _address = address;

	public bool IsLoaded => _loaded;

	public void Preload()
	{
		if( _loaded || _isLoading ) return;
		_isLoading = true;
		_handle = Addressables.LoadAssetAsync<T>( _address );
		_handle.Completed += OnCompleted;
#if DEBUG_NOTIFY
		NotificationConsole.Notify( $"<color=#0080FF>LazyAddressableRef</color> Loading: \"{_address}\"" );
#endif //DEBUG_NOTIFY
	}

	public async Awaitable PreloadAsync()
	{
		var handle = LoadAsync();
		while (!handle.GetAwaiter().IsCompleted)
			await Awaitable.EndOfFrameAsync();
	}

	public async Awaitable<T> LoadAsync()
	{
		if (_loaded)
			return _asset;

		if (!_isLoading || _handle.Status == AsyncOperationStatus.None)
		{
			_isLoading = true;
			_handle = Addressables.LoadAssetAsync<T>(_address);
		}

		if (!_handle.IsDone)
			await _handle.Task;

		OnCompleted(_handle);
			
#if DEBUG_NOTIFY
		NotificationConsole.Notify( $"<color=#0080FF>LazyAddressableRef</color> Loading: \"{_address}\"" );
#endif //DEBUG_NOTIFY

		return _asset;
	}

	public T SafeInstance
	{
		get
		{
#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				if( _safeEditorAsset == null )
				{
					var guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T)}");
					_safeEditorAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
				}
				return _safeEditorAsset;
			}
#endif
			return Asset;
		}
	}

	public T Asset
	{
		get
		{
			if( _loaded ) return _asset;
			
			if( !_isLoading ) Preload();
			
#if CODE_METRICS
			var sw = StopwatchPool.RequestStarted();
#endif // CODE_METRICS

			if( _handle.IsValid() ) 
			{
				if( _handle.Status == AsyncOperationStatus.None ) _handle.WaitForCompletion();
				_asset = _handle.Result;
				_loaded = true;
			}
#if CODE_METRICS
			var message = $"😴<color=#0080FF>LazyAddressableRef</color> \"{_address}\" Request miss took: <color=#DAA520>{ValueToString(sw.ReturnToPoolAndGetElapsedMs())}ms</color>";
#if DEBUG_NOTIFY
			NotificationConsole.Notify( message );
#else
			Debug.Log( message );
#endif //DEBUG_NOTIFY
#endif //CODE_METRICS

			return _asset;
		}
	}
	
    static string ValueToString(double ms) => ms > 10 ? $"{ms:N0}" : ( ms > 1 ? $"{ms:N1}" : ( ms > .1 ? $"{ms:N2}" : ( ms > .01 ? $"{ms:N3}" : $"{ms:N6}" ) ) );

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