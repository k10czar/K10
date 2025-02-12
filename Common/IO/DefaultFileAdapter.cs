#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WP_8 || UNITY_WP_8_1
using System;
using UnityEngine;

#if UNITY_WINRT
using UnityEngine.Windows;
#else
using System.IO;
#endif

public class DefaultFileAdapter : IFileAdapter
{
	public string GetPersistentDataPath() { return Application.persistentDataPath; }
	public string GetDebugPersistentDataPath() { return Application.persistentDataPath; }
	public bool Exists( string path ) { return File.Exists( path ); }
	public byte[] ReadAllBytes( string path ) { return Exists( path ) ? File.ReadAllBytes( path ) : Array.Empty<byte>(); }
	public void WriteAllBytes( string path, byte[] bytes ) { File.WriteAllBytes( path, bytes ); }
	public void RequestDirectory( string dir ) { if( !Directory.Exists(dir) ) Directory.CreateDirectory( dir ); }
	public void Delete( string path ) { if( Exists(path) ) File.Delete( path ); }
	public void DeleteDir( string path, bool recursive ) { if( Directory.Exists(path) ) Directory.Delete( path, recursive ); }
	public void Copy( string source, string destination ) { File.Copy( source, destination ); }
	public void SavePlayerPrefs() { PlayerPrefsAdapter.Save(); }
}

#endif
