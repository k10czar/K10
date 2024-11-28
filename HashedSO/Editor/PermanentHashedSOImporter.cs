using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using static Colors.Console;

public sealed class PermanentHashedSOImporter : AssetPostprocessor
{
	// static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
	static void Deactivated(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		var sw = new System.Diagnostics.Stopwatch();
		sw.Start();

		bool containsTags = false;

		var collections = new List<IHashedSOCollection>();
		var elements = new List<IHashedSO>();

		for (int i = 0; i < importedAssets.Length; i++)
		{
			var obj = AssetDatabase.LoadAssetAtPath(importedAssets[i], typeof(ScriptableObject));

			if( obj is IHashedSOCollection collection ) collections.Add( collection );
			if( obj is IHashedSO hso ) elements.Add( hso );
			if( obj is TagSO tag ) containsTags = true;
		}

		for (int i = 0; i < collections.Count; i++)
		{
			var collection = collections[i];
			//Debug.Log( $"{"OnPostprocessAllAssets".Colorfy( TypeName )} {"Before".Colorfy( Keyword )} {"EditorRemoveWrongElements".Colorfy( Verbs )}( {AssetDatabase.GetAssetPath( collection as Object )} )\n{collection}" );
			collection.EditorRemoveWrongElements();
			//Debug.Log( $"{"OnPostprocessAllAssets".Colorfy( TypeName )} {"After".Colorfy( Warning )} {"EditorRemoveWrongElements".Colorfy( Verbs )}( {AssetDatabase.GetAssetPath( collection as Object )} )\n{collection}" );
		}

		for (int i = 0; i < elements.Count; i++)
		{
			var hso = elements[i];
			var col = hso.GetCollection();
			if (col != null)
			{
				var oHso = hso as Object;
				//Debug.Log( $"{"OnPostprocessAllAssets".Colorfy( TypeName )} {"Before".Colorfy( Keyword )} of {"EditorRequestMember".Colorfy( Verbs )}( {oHso.NameOrNull()}[{hso.HashID}] ) on Collection {col.ToStringOrNull()}" );
				col.EditorRequestMember( oHso );
				//Debug.Log( $"{"OnPostprocessAllAssets".Colorfy( TypeName )} {"After".Colorfy( Warning )} of {"EditorRequestMember".Colorfy( Verbs )}( {oHso.NameOrNull()}[{hso.HashID}] ) on Collection {col.ToStringOrNull()}" );
			}
		}

		for (int i = 0; i < collections.Count; i++)
		{
			var collection = collections[i];
			//Debug.Log( $"{"OnPostprocessAllAssets".Colorfy( TypeName )} {"Before".Colorfy( Keyword )} {"EditorCheckConsistency".Colorfy( Verbs )}( {AssetDatabase.GetAssetPath( collection as Object )} )\n{collection}" );
			collection.EditorCheckConsistency();
			//Debug.Log( $"{"OnPostprocessAllAssets".Colorfy( TypeName )} {"After".Colorfy( Warning )} {"EditorCheckConsistency".Colorfy( Verbs )}( {AssetDatabase.GetAssetPath( collection as Object )} )\n{collection}" );
		}

		sw.Stop();

		var hasElements = importedAssets.Length > 0 || deletedAssets.Length > 0 || movedAssets.Length > 0 || movedFromAssetPaths.Length > 0;
		if( !hasElements && sw.ElapsedMilliseconds < 10 ) return;

		var afterLine = $"\n{Colorfy.OpenTag( Names )}   -";
		var log = $"{"PermanentHashedSOImporter".Colorfy( TypeName )}.{"OnPostprocessAllAssets".Colorfy( Verbs )} took {$"{sw.ElapsedMilliseconds}ms".Colorfy( Numbers )}";
		if( importedAssets.Length > 0 ) log += $"\n{$"{importedAssets.Length.ToString().Colorfy( Numbers )} imported assets:".Colorfy( Verbs )}{afterLine}{string.Join( ",\n   -", importedAssets ).Colorfy( Names )}{Colorfy.CloseTag()}";
		if( deletedAssets.Length > 0 ) log += $"\n{$"{deletedAssets.Length.ToString().Colorfy( Numbers )} deleted assets:".Colorfy( Negation )}{afterLine}{string.Join( ",\n   -", deletedAssets ).Colorfy( Names )}{Colorfy.CloseTag()}";
		if( movedAssets.Length > 0 ) log += $"\n{$"{movedAssets.Length.ToString().Colorfy( Numbers )} moved assets:".Colorfy( TypeName )}{afterLine}{string.Join( ",\n   -", movedAssets ).Colorfy( Names )}{Colorfy.CloseTag()}";
		if( movedFromAssetPaths.Length > 0 ) log += $"\n{$"{movedFromAssetPaths.Length.ToString().Colorfy( Numbers )} moved from asset paths:".Colorfy( Interfaces )}{afterLine}{string.Join( ",\n   -", movedFromAssetPaths ).Colorfy( Names )}{Colorfy.CloseTag()}";

		//Debug.Log( log );
		
		if( containsTags ) TagsDebug.Instance.Rebuild();
	}
}
