using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

public static class HsoUtils
{
	public static bool TryTransferRef<T,K>( ref K softRef, ref T directRef ) where T : UnityEngine.ScriptableObject, IHashedSO where K : CollectionElementSoftReference<T>, new()
	{
		if( directRef == default(T) ) 
		{
			if( softRef != default(K) && softRef.GetReference() == default(T) )
			{
				softRef = default(K);
				return true;
				// return false;
			} else
			{
				return softRef?.RefreshUsedRef() ?? false;
			}
		}
		if( softRef == default(K) ) softRef = new K();
		softRef.SetReference( directRef );
		directRef = default(T);
		return true;
	}
}

public static class CollectionElementSoftReferenceExtensions
{
	public static bool Contains<T>( this List<CollectionElementSoftReference<T>> collection, T element ) where T : UnityEngine.ScriptableObject, IHashedSO
	{
		if( element == null ) return false;
		return Contains( collection, element.HashID );
	}

	public static bool Contains<T>( this List<CollectionElementSoftReference<T>> collection, int hashId ) where T : UnityEngine.ScriptableObject, IHashedSO
	{
		if( collection == null ) return false;
		for( int i = 0; i < collection.Count; i++ )
		{
			var softRef = collection[i];
			if( softRef.HashID == hashId ) return true;
		}
		return false;
	}

	public static bool TransferFrom<T>( ref CollectionElementSoftReference<T>[] to, ref T[] from ) where T : UnityEngine.ScriptableObject, IHashedSO
	{
		if( from == null ) return false;
		var totalCount = 0;
		var set = new HashSet<int>();
		for( int i = 0; i < to.Length; i++ )
		{
			var element = to[i];
			if( element == null ) continue;
			if( set.Contains( element.HashID ) ) continue;
			totalCount++;
		}
		var modded = false;
		for( int i = 0; i < from.Length; i++ )
		{
			var element = from[i];
			if( element == null ) continue;
			if( set.Contains( element.HashID ) ) continue;
			totalCount++;
			modded = true;
		}
		if( !modded ) return false;
		var collection = new CollectionElementSoftReference<T>[ totalCount ];
		set.Clear();
		totalCount = 0;
		for( int i = 0; i < to.Length; i++ )
		{
			var element = to[i];
			if( element == null ) continue;
			if( set.Contains( element.HashID ) ) continue;
			collection[totalCount++] = element;
		}
		for( int i = 0; i < from.Length; i++ )
		{
			var element = from[i];
			if( element == null ) continue;
			if( set.Contains( element.HashID ) ) continue;
			collection[totalCount++] = new CollectionElementSoftReference<T>( element );
		}
		from = null;
		return modded;
	}

	public static bool TransferFrom<T>( this List<CollectionElementSoftReference<T>> to, ref List<T> from ) where T : UnityEngine.ScriptableObject, IHashedSO
	{
		if( from == null ) return false;
		var modded = false;
		for( int i = 0; i < from.Count; i++ )
		{
			var hardRef = from[i];
			if( !Contains( to, hardRef ) ) 
			{
				var softRef = new CollectionElementSoftReference<T>( hardRef );
				to.Add( softRef );
				modded = true;
			}
			from.RemoveAt( i );
			i--;
		}
		from = null;
		return modded;
	}

	public static bool TransferFrom<T>( this List<CollectionElementSoftReference<T>> to, ref T[] from ) where T : UnityEngine.ScriptableObject, IHashedSO
	{
		if( from == null ) return false;
		var modified = false;
		for( int i = 0; i < from.Length; i++ )
		{
			var hardRef = from[i];
			if( !Contains( to, hardRef ) ) 
			{
				var softRef = new CollectionElementSoftReference<T>( hardRef );
				to.Add( softRef );
				modified = true;
			}
		}
		from = null;
		return modified;
	}
	
	public static bool TransferFrom<T>( this CollectionElementSoftReference<T> to, ref T from ) where T : UnityEngine.ScriptableObject, IHashedSO
	{
		if( from == default(T) ) return false;
		to.SetReference( from );
		from = default(T);
		return true;
	}

	public static IEnumerable<T> DirectRefsEnumerator<T>( this List<CollectionElementSoftReference<T>> collection ) where T : UnityEngine.ScriptableObject, IHashedSO
	{
		var count = collection.Count;
		for( int i = 0; i < count; i++ )
		{
			yield return collection[i].GetReference();
		}
	}
	
#if UNITY_EDITOR
	public static bool EDITOR_TransferToSoftReference<T>( this IList<T> collection ) where T : IEditorAssetValidationProcess
	{
		if( collection == null ) return false;
		var modded = false;
		for( int i = 0; i < collection.Count; i++ )
		{
			var t = collection[i];
			if( t == null ) continue;
			modded |= t.EDITOR_ExecuteAssetValidationProcess();
			collection[i] = t;
		}
		return modded;
	}
#endif
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
    [SerializeField] int _id = -1;

	public int HashID => _id;

#if UNITY_EDITOR
    public bool HasReference => _id >= 0 && !string.IsNullOrEmpty( _editorAssetRefGuid );
#else
    public bool HasReference => _id >= 0;
#endif //UNITY_EDITOR

    public virtual IHashedSOCollection GetCollection() => CachedDummySO<T>.Instance.GetCollection();

	public CollectionElementSoftReference() { }

	public CollectionElementSoftReference( T t )
	{
		SetReference( t );
	}

	public override void DisposeAsset()
	{
		_assetRuntimeReference = null;
#if UNITY_EDITOR
		_referenceState = EAssetReferenceState.Empty;
#endif //UNITY_EDITOR
	}

#if UNITY_EDITOR
	public T EDITOR_GetAsset()
	{
		if( _assetHardReference != null ) return _assetHardReference;
		if( !string.IsNullOrEmpty( _editorAssetRefGuid ) ) 
		{
			var path = AssetDatabase.GUIDToAssetPath( _editorAssetRefGuid );
			if( !string.IsNullOrEmpty( path ) )
			{
				var asset = AssetDatabase.LoadAssetAtPath<T>( path );
				if( asset != null ) return asset;
			}
		}
		return GetReference();
	}
#endif //UNITY_EDITOR
	
    public bool RefreshUsedRef()
    {
        return SetReference( GetReference() );
    }

#if UNITY_EDITOR
    public override System.Type EDITOR_GetAssetType() => typeof(T);

    public void UpdateOldRef()
    {
		if (_assetHardReference == null)
		{
			_id = -1;
			return;
		}
        var path = UnityEditor.AssetDatabase.GetAssetPath( _assetHardReference );
        _editorAssetRefGuid = UnityEditor.AssetDatabase.AssetPathToGUID( path );
		EDITOR_UpdateDataFromRef();
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
		if( _id >= 0 && _assetRuntimeReference == null ) 
		{
			var collection = GetCollection();
            if( collection == null ) return null;
			var id = Mathf.Max( _id, 0 );
			_assetRuntimeReference = (T)collection.GetElementBase( id );
		}
#if UNITY_EDITOR
		_referenceState = _assetRuntimeReference != null ? EAssetReferenceState.Loaded : EAssetReferenceState.LoadedNull;
#endif //UNITY_EDITOR
		return _assetRuntimeReference;
	}

    public void ClearReference()
    {
        SetReference( null );
    }

	public virtual bool SetReference( T t )
	{
		var changed = false;
		var initialId = _id;
		var initialAssetRuntimeReference = _assetRuntimeReference;

		var changedRuntimeRef = _assetRuntimeReference != t;
		// changed |= changedRuntimeRef;
		_assetRuntimeReference = t;

		var id = _assetRuntimeReference?.HashID ?? -1;
		var changedId = id != _id;
		changed |= changedId;
		_id = id;

#if UNITY_EDITOR

		var changedHardRef = _assetHardReference != t;
		// changed |= changedHardRef;
		_assetHardReference = t;

		UpdateOldRef();
		GetReference();

		if( changed && !Application.isPlaying )
		{
			Debug.Log( $"Changed {t.ToStringOrNull()} {changedRuntimeRef} {changedId}({initialId}->{id}) {changedHardRef}" );
		}
#endif //UNITY_EDITOR
		return changed;
	}

    public override string ToString() => $"{typeof(T).FullName}[_id]";
}
