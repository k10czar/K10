using System;
using UnityEngine;

public class NullFileAdapter : IFileAdapter
{
	private static bool _warningLogged = false;
	private static void LogWarningOnce()
	{
		if( _warningLogged ) return;
		Debug.LogWarning( "NullFileAdapter currently in use, file reads and writes will not work. " +
						 "Use FileAdapter.SetImplementation() to provide a IFileAdapter for this platform." );
		_warningLogged = true;
	}

	public string GetPersistentDataPath() { LogWarningOnce(); return ""; }
	public string GetDebugPersistentDataPath() { LogWarningOnce(); return ""; }
	public bool Exists( string path ) { LogWarningOnce(); return false; }
	public byte[] ReadAllBytes( string path ) { LogWarningOnce(); return Array.Empty<byte>(); }
	public void WriteAllBytes( string path, byte[] bytes ) { LogWarningOnce(); }
	public void RequestDirectory( string dir ) { LogWarningOnce(); }
	public void Delete( string path ) { LogWarningOnce(); }
	public void DeleteDir( string path, bool recursive ) { LogWarningOnce(); }
	public void SavePlayerPrefs() { LogWarningOnce(); }
}
