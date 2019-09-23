using UnityEngine;
using UnityEditor;

public sealed class PermanentHashedSOImporter : AssetPostprocessor
{
	static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
	{
		for( int i = 0; i < importedAssets.Length; i++ )
		{
			var obj = AssetDatabase.LoadAssetAtPath( importedAssets[i], typeof( ScriptableObject ) );

			if( obj is IHashedSOCollection collection ) collection.EditorCheckConsistency();

			if( obj is IHashedSO hso )
			{
				var col = hso.GetCollection();
				if( col != null )
				{
					var oHso = hso as Object;
					Debug.Log( $"AssetPostprocessor of {oHso.NameOrNull()} on Collection {col.ToStringOrNull()}" );
					col.EditorRequestMember( oHso );
				}
			}
		}
	}
}
