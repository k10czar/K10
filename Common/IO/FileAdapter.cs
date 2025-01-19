using System.Text;

public interface IFileAdapter
{
	string GetPersistentDataPath();
	string GetDebugPersistentDataPath();

	bool Exists( string path );
	byte[] ReadAllBytes( string path );
	void WriteAllBytes( string path, byte[] bytes );
	void RequestDirectory( string dir );
	void Delete( string path );
	void DeleteDir( string path, bool recursive );
	void Copy( string source, string destination );
	void SavePlayerPrefs();
}

public static class FileAdapter
{
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WP_8 || UNITY_WP_8_1
	private static IFileAdapter _implementation = new DefaultFileAdapter();
#else
	private static IFileAdapter _implementation = new NullFileAdapter();
#endif
	
	public static void SetImplementation(IFileAdapter implementation) { _implementation = implementation; }
	
	public static string persistentDataPath => _implementation.GetPersistentDataPath();
	public static string debugPersistentDataPath => _implementation.GetDebugPersistentDataPath();

	public static bool Exists( string path ) { return _implementation.Exists( path ); }
	
	public static byte[] ReadAllBytes( string path ) { return _implementation.ReadAllBytes( path ); }
	public static string ReadHasUTF8( byte[] bytes ) { return System.Text.Encoding.UTF8.GetString( bytes, 0, bytes.Length ); }
	public static string ReadHasUTF8( string path ) { var bytes = ReadAllBytes( path ); return ReadHasUTF8( bytes ); }

	public static void WriteAllBytes( string path, byte[] bytes ) { RequestFilePath( path ); _implementation.WriteAllBytes( path, bytes ); }

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

	public static void RequestDirectory( string dir ) { _implementation.RequestDirectory( dir ); }

	public static void SaveHasUTF8( string path, string data ) { WriteAllBytes( path, System.Text.Encoding.UTF8.GetBytes( data ) ); }

	public static void Delete( string path ) { _implementation.Delete( path ); }
	public static void DeleteDir( string path, bool recursive ) { _implementation.DeleteDir( path, recursive ); }
	public static void Copy( string source, string destination ) { _implementation.Copy( source, destination ); }

	public static void SavePlayerPrefs() { _implementation.SavePlayerPrefs(); }
}
