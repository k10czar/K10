using UnityEngine;
using UnityEditor;

public sealed class PermanentHashedSOImporter : AssetPostprocessor
{
	static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
	{
		for( int i = 0; i < importedAssets.Length; i++ )
		{
			var obj = AssetDatabase.LoadAssetAtPath( importedAssets[i], typeof( ScriptableObject ) );

			if( obj is IHashedSOCollection collection )
			{
				Debug.Log( $"OnPostprocessAllAssets Before {AssetDatabase.GetAssetPath( collection as Object )}\n{collection}" );
				collection.EditorCheckConsistency();
				Debug.Log( $"OnPostprocessAllAssets After {AssetDatabase.GetAssetPath( collection as Object )}\n{collection}" );
			}

			if( obj is IHashedSO hso )
			{
				var col = hso.GetCollection();
				if( col != null )
				{
					var oHso = hso as Object;
					Debug.Log( $"OnPostprocessAllAssets Before of {oHso.NameOrNull()}[{hso.HashID}] on Collection {col.ToStringOrNull()}" );
					col.EditorRequestMember( oHso );
					Debug.Log( $"OnPostprocessAllAssets After of {oHso.NameOrNull()}[{hso.HashID}] on Collection {col.ToStringOrNull()}" );
				}
			}
		}
	}
}
