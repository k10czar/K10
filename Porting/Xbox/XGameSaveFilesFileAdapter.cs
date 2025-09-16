#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using System;
using UnityEngine;
using System.IO;
using Unity.XGamingRuntime;
using UnityEngine.Android;
using System.Linq;

public class XGameSaveFilesFileAdapter : IFileAdapter
{
	BoolState _isInitilized = new();
	public IBoolStateObserver IsInitilized => _isInitilized;

	private string _folderPath;
	private XUserHandle _userHandle;
	private string _scid;
	ConditionalEventsCollection _validator = new ();

    public void Initialize(XUserHandle userHandle, string scid)
    {
		_userHandle = userHandle;
		_scid = scid;

		RetrieveSaveFolder();
		
		_validator.Void();
		ApplicationEventsRelay.IsSuspended.OnFalseState.Register(_validator.Validated(RetrieveSaveFolder));
    }

	private void RetrieveSaveFolder()
	{
        Debug.Log($"RetrieveSaveFolder");
		_isInitilized.SetFalse();
        SDK.XGameSaveFilesGetFolderWithUiAsync(_userHandle, _scid, 
            (int hresult, string folderResult) => 
            {
				if (HR.FAILED(hresult))
				{
					Debug.LogError($"Couldn't get XGameSaveFiles folder. HResult {hresult} ({HR.NameOf(hresult)})");
// #if UNITY_EDITOR
// 					_isInitilized.SetTrue();
// 					// TODO-Porting Remove this when is really working on PC 
// #endif
					return;
				}

				_folderPath = folderResult + "\\SV";
				RequestDirectory( _folderPath );

				_isInitilized.SetTrue();
                Debug.Log($"Successfully initialized XGameSaveFiles folder: {folderResult}");

				DebugFiles();
            }
        );
    }

    private void DebugFiles()
    {
		var sb = StringBuilderPool.RequestEmpty();

		var filesCount = 0;
		var foldersCount = 0;

		var path = GetPersistentDataPath();
		ListFoldersAndFiles( sb, 0, path, ref filesCount, ref foldersCount );

		Debug.Log( $"{path} has Files:{filesCount} Folders:{foldersCount} {{Creation}} (LastWrite) [LastAccess] \n{sb.ReturnToPoolAndCast()}" );
    }

	void ListFoldersAndFiles( System.Text.StringBuilder sb, int level, string path, ref int filesCount, ref int foldersCount, string indentation = "    " )
	{
		for( int i = 0; i < level; i++ ) sb.Append( indentation );
		var pathCreation = File.GetCreationTimeUtc( path );
		var pathLastAccess = File.GetLastAccessTimeUtc( path );
		var pathLastWrite = File.GetLastWriteTimeUtc( path );

		var lastBar = path.Length - 2;
		for( ; lastBar > 0; lastBar-- ) if( path[lastBar] == '/' || path[lastBar] == '\\' ) break;
		sb.AppendLine( $"{path.Substring(lastBar)} {{{pathCreation:dd MMM yy HH:mm:ss}}} ({pathLastAccess:dd MMM yy HH:mm:ss}) [{pathLastWrite:dd MMM yy HH:mm:ss}]" );

		level++;

		var files = Directory.GetFiles( path );
		filesCount += files.Length;
		foreach ( var file in files )
		{
			for( int i = 0; i < level; i++ ) sb.Append( indentation );
			var creation = File.GetCreationTimeUtc( file );
			var lastAccess = File.GetLastAccessTimeUtc( file );
			var lastWrite = File.GetLastWriteTimeUtc( file );
			sb.AppendLine( $"{file.Substring( path.Length )} {{{creation:dd MMM yy HH:mm:ss}}} ({lastWrite:dd MMM yy HH:mm:ss}) [{lastAccess:dd MMM yy HH:mm:ss}]" );
		}

		var folders = Directory.GetDirectories( path );
		foldersCount += folders.Length;
		foreach ( var folder in folders )
		{
			ListFoldersAndFiles( sb, level, folder, ref filesCount, ref foldersCount );
		}
	}

    public string GetPersistentDataPath() 
	{ 
		if ( _folderPath == null)
		{
			Debug.LogError($"XGameSaveFiles folder not initialized");
			_folderPath = Application.persistentDataPath;
		}
	
		return _folderPath; 
	}
	
	public string GetDebugPersistentDataPath() { return GetPersistentDataPath() +"/Debug"; }
	public bool Exists( string path ) { return File.Exists( path ); }
	public byte[] ReadAllBytes( string path ) { return Exists( path ) ? File.ReadAllBytes( path ) : Array.Empty<byte>(); } public void WriteAllBytes( string path, byte[] bytes ) { File.WriteAllBytes( path, bytes ); }
	public void RequestDirectory( string dir ) { if( !Directory.Exists(dir) ) Directory.CreateDirectory( dir ); }
	public void Delete( string path ) { if( Exists(path) ) File.Delete( path ); }
	public void DeleteDir( string path, bool recursive ) { if( Directory.Exists(path) ) Directory.Delete( path, recursive ); }
	public void Copy( string source, string destination ) { File.Copy( source, destination ); }
	public void SavePlayerPrefs() { PlayerPrefsAdapter.Save(); }
}
#endif