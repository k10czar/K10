using UnityEngine;
using System.Collections;

#if UNITY_WINRT
using UnityEngine.Windows;
#else
using System.IO;
#endif

public static class FileAdapter
{
	public static bool Exists( string path ) { return File.Exists( path ); }
	public static byte[] ReadAllBytes( string path ) { return Exists( path ) ? File.ReadAllBytes( path ) : new byte[0]; }
	public static string ReadHasUTF8( byte[] bytes ) { return System.Text.Encoding.UTF8.GetString( bytes, 0, bytes.Length ); }
	public static string ReadHasUTF8( string path ) { var bytes = ReadAllBytes( path ); return ReadHasUTF8( bytes ); }
	public static void WriteAllBytes( string path, byte[] bytes ) { RequestFilePath( path ); File.WriteAllBytes( path, bytes ); }

	const string DEBUG_PATH = "Temp/K10/Debug/";
	static string GenerateDebugPath() { return DEBUG_PATH + System.DateTime.Now.ToString( "yyyy_mm_dd_HH_mm_ss_fff_tt " ); }
	public static void Debug( string name, string data ) { SaveHasUTF8( GenerateDebugPath() + name, data ); }
	public static void Debug( string name ) { WriteAllBytes( GenerateDebugPath() + name, new byte[0] ); }

	public static void RequestFilePath( string filePath )
	{
		int pathFileDivisor = filePath.Length - 1;
		while( pathFileDivisor > 0 && filePath[pathFileDivisor] != '/' && filePath[pathFileDivisor] != '\\' )
			pathFileDivisor--;

		RequestDirectory( filePath.Substring( 0, pathFileDivisor ) );
	}

	public static void RequestDirectory( string dir )
	{
		if( !Directory.Exists( dir ) )
		{
			//Debug.Log( "Creating directory " + dir );
			Directory.CreateDirectory( dir );
		}
	}

	public static void SaveHasUTF8( string path, string data ) { WriteAllBytes( path, System.Text.Encoding.UTF8.GetBytes( data ) ); }
	public static void Delete( string path ) { if( Exists( path ) ) File.Delete( path ); }
	public static void DeleteDir( string path, bool recursive ) { if( Directory.Exists( path ) ) Directory.Delete( path, recursive ); }
}