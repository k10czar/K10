using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public sealed class PermanentHashedSOImporter : AssetPostprocessor
{
	static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
	{
		Debug.Log($"<><> OnPostprocessAllAssets 1");
		var sw = new System.Diagnostics.Stopwatch();
		sw.Start();

		var collections = new List<IHashedSOCollection>();
		var elements = new List<IHashedSO>();

		for( int i = 0; i < importedAssets.Length; i++ )
		{
			var obj = AssetDatabase.LoadAssetAtPath( importedAssets[i], typeof( ScriptableObject ) );

			if( obj is IHashedSOCollection collection ) collections.Add( collection );
			if( obj is IHashedSO hso ) elements.Add( hso );
		}

		for( int i = 0; i < collections.Count; i++ )
		{
			var collection = collections[i];
			Debug.Log( $"OnPostprocessAllAssets Before EditorRemoveWrongElements( {AssetDatabase.GetAssetPath( collection as Object )} )\n{collection}" );
			collection.EditorRemoveWrongElements();
			Debug.Log( $"OnPostprocessAllAssets After EditorRemoveWrongElements( {AssetDatabase.GetAssetPath( collection as Object )} )\n{collection}" );
		}

		for( int i = 0; i < elements.Count; i++ )
		{
			var hso = elements[i];
			var col = hso.GetCollection();
			if( col != null )
			{
				var oHso = hso as Object;
				Debug.Log( $"OnPostprocessAllAssets Before of EditorRequestMember( {oHso.NameOrNull()}[{hso.HashID}] ) on Collection {col.ToStringOrNull()}" );
				col.EditorRequestMember( oHso );
				Debug.Log( $"OnPostprocessAllAssets After of EditorRequestMember( {oHso.NameOrNull()}[{hso.HashID}] ) on Collection {col.ToStringOrNull()}" );
			}
		}

		for( int i = 0; i < collections.Count; i++ )
		{
			var collection = collections[i];
			Debug.Log( $"OnPostprocessAllAssets Before EditorCheckConsistency( {AssetDatabase.GetAssetPath( collection as Object )} )\n{collection}" );
			collection.EditorCheckConsistency();
			Debug.Log( $"OnPostprocessAllAssets After EditorCheckConsistency( {AssetDatabase.GetAssetPath( collection as Object )} )\n{collection}" );
		}

		sw.Stop();
		Debug.Log( $"PermanentHashedSOImporter.OnPostprocessAllAssets took {sw.ElapsedMilliseconds}ms"
				+ $"\nimportedAssets({importedAssets.Length}):\n\t-{importedAssets.Length>0}{string.Join( ",\n\t-", importedAssets )}"
				+ $"\ndeletedAssets({deletedAssets.Length}):\n\t-{string.Join( ",\n\t-", deletedAssets )}"
				+ $"\nmovedAssets({movedAssets.Length}):\n\t-{string.Join( ",\n\t-", movedAssets )}"
				+ $"\nmovedFromAssetPaths({movedFromAssetPaths.Length}):\n\t-{string.Join( ",\n\t-", movedFromAssetPaths )}" );
		
		Debug.Log($"<><> OnPostprocessAllAssets 2");
	}
}
