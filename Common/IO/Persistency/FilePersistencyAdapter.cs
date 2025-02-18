using UnityEngine;

public class FilePersistencyAdapter : IPersistencyAdapter<string>
{
    string _filePath;

    public string FilePath => $"{FileAdapter.persistentDataPath}/{_filePath}";

    public FilePersistencyAdapter( string filePath )
    {
        _filePath = filePath;
    }

    public string Load()
    {
        var path = FilePath;
        var read = FileAdapter.Exists( path ) ? FileAdapter.ReadAsUTF8( path ) : string.Empty;
        Debug.Log( $"FilePersistencyAdapter.Load( @{path} ) => {read}" );
        return read;
    }

    public void Persists( string fileData )
    {
        var path = FilePath;
        Debug.Log( $"FilePersistencyAdapter.Persists( @{path}, {fileData} )" );
        FileAdapter.SaveAsUTF8( path, fileData );
    }
}
