#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using System;
using UnityEngine;
using System.IO;
using Unity.XGamingRuntime;

public class XGameSaveFilesFileAdapter : IFileAdapter
{
	public bool Initialized { get; private set; }
	private string _folderPath;

    public void Initialize(XUserHandle userHandle, string scid)
    {
        SDK.XGameSaveFilesGetFolderWithUiAsync(userHandle, scid, 
            (int hresult, string folderResult) => 
            {
				if (HR.FAILED(hresult))
				{
					Debug.LogError($"Couldn't get XGameSaveFiles folder. HResult {hresult} ({HR.NameOf(hresult)})");
					return;
				}

				_folderPath = folderResult;
                Debug.Log($"Successfully initialized XGameSaveFiles folder: {folderResult}");
            }
        );
    }

	public string GetPersistentDataPath() { return _folderPath; }
	public string GetDebugPersistentDataPath() { return _folderPath + "/Debug"; }
	public bool Exists( string path ) { return File.Exists( path ); }
	public byte[] ReadAllBytes( string path ) { 
		return Exists( path ) ? File.ReadAllBytes( path ) : Array.Empty<byte>(); 
	}
	public void WriteAllBytes( string path, byte[] bytes ) { 
		File.WriteAllBytes( path, bytes ); 
	}
	public void RequestDirectory( string dir ) { if( !Directory.Exists(dir) ) Directory.CreateDirectory( dir ); }
	public void Delete( string path ) { if( Exists(path) ) File.Delete( path ); }
	public void DeleteDir( string path, bool recursive ) { if( Directory.Exists(path) ) Directory.Delete( path, recursive ); }
	public void Copy( string source, string destination ) { File.Copy( source, destination ); }
	public void SavePlayerPrefs() { PlayerPrefs.Save(); }
}
#endif