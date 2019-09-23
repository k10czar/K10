using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PermanentHashedSOCollection<T> : HashedSOCollection<T> where T : HashedScriptableObject
{
#if UNITY_EDITOR
	protected override bool CanChangeIDsToOptimizeSpace => false;
	protected override bool ResolveConflictedFile( IHashedSO t, string assetPath )
	{
		if( !UnityEditor.EditorUtility.DisplayDialog( "Conflict on PermanentHashedSOCollection",
				$"{( (Object)t ).NameOrNull()} already has an ID but it's not on the appropriate list ({this.name}). Assign a new ID for it?",
				"Yes",
				"No, delete it" ) )
		{
			AssetDatabase.DeleteAsset( assetPath );
			return false;
		}
		return true;
	}
#endif
}