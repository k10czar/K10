using System.Collections.Generic;
using UnityEngine;

public interface IEditorAssetProcessingProcess
{
	bool EDITOR_ExecuteAssetValidationProcess();
}

public static class EditorAssetProcessingExtensions
{
	private static Object _currentModifiedAsset;
    public static Object CurrentModifiedAsset => _currentModifiedAsset;

    public static bool EDITOR_ExecuteAssetValidationProcessOnObject( this IEditorAssetProcessingProcess t )
	{
		var old = _currentModifiedAsset;
		_currentModifiedAsset = t as Object;
		var ret = t.EDITOR_ExecuteAssetValidationProcess();
		_currentModifiedAsset = old;
		return ret;
	}

    public static bool EDITOR_TransferFrom<T>( this IList<T> destinationCollection, ref T[] originalCollection )
	{
		if( originalCollection == null ) return false;
		if( originalCollection.Length == 0 ) return false;
		destinationCollection.Clear();
		for( int i = 0; i < originalCollection.Length; i++ )
		{
			var t = originalCollection[i];
			destinationCollection.Add( t );
		}
		originalCollection = null;
		return true;
	}

    public static bool EDITOR_TransferFrom<T>( ref T[] destinationCollection, ref T[] originalCollection )
	{
		if( originalCollection == null ) return false;
		if( originalCollection.Length == 0 ) return false;
		destinationCollection = new T[ originalCollection.Length ];
		for( int i = 0; i < originalCollection.Length; i++ ) destinationCollection[i] = originalCollection[i];
		originalCollection = null;
		return true;
	}

    public static bool EDITOR_RemoveEmpty<T>( this IList<T> destinationCollection )
	{
		var removed = false;
		for( int i = destinationCollection.Count - 1; i >= 0; i-- )
		{
			var t = destinationCollection[i];
			if( t != null ) continue;
			removed = true;
			destinationCollection.RemoveAt( i );
		}
		return removed;
	}

	public static bool EDITOR_TryExecuteAssetValidationProcess<T>( this IList<T> collection )
	{
		if( collection == null ) return false;
		var modded = false;
		for( int i = 0; i < collection.Count; i++ )
		{
			var t = collection[i];
			if( t == null ) continue;
			if( t is IEditorAssetProcessingProcess proc )
			{
				modded |= proc.EDITOR_ExecuteAssetValidationProcess();
				collection[i] = t;
			}
		}
		return modded;
	}

	public static bool EDITOR_ExecuteAssetValidationProcess<T>( this IList<T> collection ) where T : IEditorAssetProcessingProcess
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
}