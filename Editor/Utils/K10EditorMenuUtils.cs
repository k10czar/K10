//Assets/Editor/K10EditorUtils.cs
using UnityEngine;
using UnityEditor;
using System;

public static class K10EditorMenuUtils
{
    const string FOLDER_RELATIVE_PATH = "Screenshots";
    const string FILE_EXTENSION = "png";

    static string GetCurrentFileName( int mag ) => $"{Application.productName}_{mag}x_{DateTime.Now:yyMMdd_HH-mm-ss-ffff}.{FILE_EXTENSION}";
    static string GetCurrentFilePath( int mag ) => $"{FOLDER_RELATIVE_PATH}/{GetCurrentFileName(mag)}";
    static void CaptureScreenshot( int mag ) 
    {
        FileAdapter.RequestDirectory( FOLDER_RELATIVE_PATH );
        var filePath = GetCurrentFilePath( mag );
        ScreenCapture.CaptureScreenshot( filePath, mag );
        Debug.Log( $"<color=cyan>CaptureScreenshot</color>( <color=#D0FF14>{mag}x</color> ) @ <color=magenta>{filePath}</color>" );
    }

    [MenuItem("K10/Utils/Capture Screenshot/1x #&1")] static void CaptureScreenshot1x() => CaptureScreenshot(1);
    [MenuItem("K10/Utils/Capture Screenshot/2x #&2")] static void CaptureScreenshot2x() => CaptureScreenshot(2);
    [MenuItem("K10/Utils/Capture Screenshot/3x #&3")] static void CaptureScreenshot3x() => CaptureScreenshot(3);
    [MenuItem("K10/Utils/Capture Screenshot/4x #&4")] static void CaptureScreenshot4x() => CaptureScreenshot(4);
    [MenuItem("K10/Utils/Capture Screenshot/Open Folder #&p")] static void OpenScreenshotsFolder() => OpenFolderOnExplorer(FOLDER_RELATIVE_PATH);
    [MenuItem("K10/Open Project Folder #&e",priority =-1)] static void OpenProjectFolder() => OpenFolderOnExplorer( Application.dataPath );
    
    public static void OpenFolderOnExplorer( string path )
    {
        Debug.Log( $"<color=cyan>OpenFolderOnExplorer</color>( <color=magenta>{path}</color> )" );
        #if UNITY_EDITOR_WIN
        System.Diagnostics.Process.Start("explorer.exe", path.Replace('/', '\\'));
        #elif UNITY_EDITOR_OSX
        System.Diagnostics.Process.Start("open", path.Replace('/', '\\'));
        #elif UNITY_EDITOR_LINUX
        System.Diagnostics.Process.Start("xdg-open", path.Replace('/', '\\'));
        #endif
    }
}
