using System.Collections.Generic;

public class RuntimeFileAdapter : IFileAdapter
{
	Dictionary<string,byte[]> files = new();

	public string GetPersistentDataPath() { return "Persistent::"; }
	public string GetDebugPersistentDataPath() { return "DEBUG_Persistent::"; }
	public bool Exists( string path ) { return files.ContainsKey( path.ToLower() ); }
	public byte[] ReadAllBytes( string path ) { return files[path.ToLower()]; }
	public void WriteAllBytes( string path, byte[] bytes ) { files[path.ToLower()] = bytes; }
	public void RequestDirectory( string dir ) {  }
	public void Delete( string path ) { files.Remove( path.ToLower() ); }
	public void DeleteDir( string path, bool recursive ) 
	{
		var lowerPath = path.ToLower();
		K10.ObjectPool.RequestList<string>( out var toDelete );
		foreach( var file in files ) if( file.Key.StartsWith( lowerPath ) ) toDelete.Add( file.Key );
		foreach( var filePath in toDelete ) files.Remove( filePath );
		K10.ObjectPool.Return( toDelete );
	}
	public void SavePlayerPrefs() { }
}
