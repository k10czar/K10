#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public interface IEditorAssetValidationProcess
{
	bool EDITOR_ExecuteAssetValidationProcess();
}

public static class EditorAssetValidationProcess
{
	private static Object _currentModifiedAsset;
    public static Object CurrentModifiedAsset => _currentModifiedAsset;

    public static bool EDITOR_ExecuteAssetValidationProcessOnObject( this IEditorAssetValidationProcess t )
	{
		var old = _currentModifiedAsset;
		_currentModifiedAsset = t as Object;
		var ret = t.EDITOR_ExecuteAssetValidationProcess();
		_currentModifiedAsset = old;
		return ret;
	}

	public static bool EDITOR_ExecuteAssetValidationProcess<T>( this IList<T> collection ) where T : IEditorAssetValidationProcess
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
#endif