using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public sealed class PermanentHashedSOImporter : AssetPostprocessor
{
	static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
	{
	//	Debug.Log($"<><> OnPostprocessAllAssets 1");
		var sw = new System.Diagnostics.Stopwatch();
		sw.Start();

		//UnityEditor.EditorUtility.RequestScriptReload();
		var collections = new List<IHashedSOCollection>();
		var elements = new List<IHashedSO>();

		if (deletedAssets.Length > 0) //FORCE RECOMPILE ASSETS AFTER DELETING ONE SO WE CAN SEE CONFLICTS ON COLLECTIONS (FOR EXAMPLE THE ASSET ALL MISSION WILL SHOW CONFLICTS IF A MISSION IS DELETED). THIS WILL ONLY OCCUR IF WE FORCE RECOMPILE ASSETS
		{
			//foreach (var deletedAsset in deletedAssets)
			//{
			//	var obj = AssetDatabase.LoadAssetAtPath( deletedAsset, typeof( ScriptableObject ) );
			////	var obj = deletedAsset;
			//	Debug.Log("!<> ");
			//	Debug.Log($"!<> {obj}");
			//}
		//	UnityEditor.EditorUtility.RequestScriptReload();
			//Debug.Log("!<> 1");
			//for( int i = 0; i < deletedAssets.Length; i++ )
			//{
			//Debug.Log("!<> 2");
			//Debug.Log($"!<> {deletedAssets[i].ToString()}");
			//
			//	var obj = AssetDatabase.LoadAssetAtPath( deletedAssets[i], typeof( ScriptableObject ) );
			//
			//Debug.Log($"!<> {obj.ToString()}");
			//	if (obj is IHashedSOCollection collection || obj is IHashedSO hso)
			//	{
			//		Debug.Log("!<> 3");
			//		
			//		UnityEditor.EditorUtility.RequestScriptReload();
			//	}
			//}
			
		}
		
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
		
//		Debug.Log($"<><> OnPostprocessAllAssets 2");
	}
}

/// <CALLBACK BEFORE DELETING ASSET ON UNITY>
/// https://docs.unity3d.com/ScriptReference/AssetModificationProcessor.OnWillDeleteAsset.html
/// https://docs.unity3d.com/ScriptReference/AssetDeleteResult.html
/// </summary>
public class CAssetModificationProcessor : AssetModificationProcessor 
{

	public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions option)
	{
		var obj = AssetDatabase.LoadAssetAtPath( assetPath, typeof( ScriptableObject ) );
			

		if (obj is IHashedSO hso)
		{
			var col = hso.GetCollection();
			col.Editor_HACK_Remove(hso.HashID);
		}
		
		Debug.Log("OnWillDeleteAsset " + assetPath);
		return AssetDeleteResult.DidNotDelete; //Tells the internal implementation that the callback did not delete the asset. The asset will be delete by the internal implementation.
	}
}