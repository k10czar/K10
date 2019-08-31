using UnityEngine;
using UnityEditor;

public sealed class PermanentHashedSOImporter : AssetPostprocessor
{
	static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
	{
		for( int i = 0; i < importedAssets.Length; i++ )
		{
			var obj = AssetDatabase.LoadAssetAtPath( importedAssets[i], typeof( ScriptableObject ) );
			if( obj is IPermanentHashedSOCollection collection )
			{
				collection.EditorCheckConsistency();
			}

			if( obj is PermanentHashedScriptableObject so )
			{
				var c = so.GetCollection();
				int id = 0;
				if( c != null )
				{
					Debug.Log( $"AssetPostprocessor of {so.NameOrNull()} on Collection {c.ToStringOrNull()}" );
					c.EditorRequestMember( so, ref id );
				}
			}
		}
	}
}
