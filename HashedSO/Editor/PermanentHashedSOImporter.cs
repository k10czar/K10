using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public sealed class PermanentHashedSOImporter : AssetPostprocessor
{
	static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
	{
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

		var hasElements = importedAssets.Length > 0 || deletedAssets.Length > 0 || movedAssets.Length > 0 || movedFromAssetPaths.Length > 0;
		if( !hasElements && sw.ElapsedMilliseconds < 10 ) return;

		var afterLine = $"\n{Colorfy.OpenTag( Colors.Console.Names )}   -";
		var log = $"{"PermanentHashedSOImporter".Colorfy( Colors.Console.Types )}.{"OnPostprocessAllAssets".Colorfy( Colors.Console.Verbs )} took {$"{sw.ElapsedMilliseconds}ms".Colorfy( Colors.Console.Numbers )}";
		if( importedAssets.Length > 0 ) log += $"\n{$"{importedAssets.Length.ToString().Colorfy( Colors.Console.Numbers )} imported assets:".Colorfy( Colors.Console.Verbs )}{afterLine}{string.Join( ",\n   -", importedAssets ).Colorfy( Colors.Console.Names )}{Colorfy.CloseTag()}";
		if( deletedAssets.Length > 0 ) log += $"\n{$"{deletedAssets.Length.ToString().Colorfy( Colors.Console.Numbers )} deleted assets:".Colorfy( Colors.Console.Negation )}{afterLine}{string.Join( ",\n   -", deletedAssets ).Colorfy( Colors.Console.Names )}{Colorfy.CloseTag()}";
		if( movedAssets.Length > 0 ) log += $"\n{$"{movedAssets.Length.ToString().Colorfy( Colors.Console.Numbers )} moved assets:".Colorfy( Colors.Console.Types )}{afterLine}{string.Join( ",\n   -", movedAssets ).Colorfy( Colors.Console.Names )}{Colorfy.CloseTag()}";
		if( movedFromAssetPaths.Length > 0 ) log += $"\n{$"{movedFromAssetPaths.Length.ToString().Colorfy( Colors.Console.Numbers )} moved from asset paths:".Colorfy( Colors.Console.Interfaces )}{afterLine}{string.Join( ",\n   -", movedFromAssetPaths ).Colorfy( Colors.Console.Names )}{Colorfy.CloseTag()}";

		Debug.Log( log );
	}
}
