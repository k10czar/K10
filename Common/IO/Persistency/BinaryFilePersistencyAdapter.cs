using UnityEngine;
using System.Collections;


public interface IPersistencyAdapter
{
	void Save<T>( string path, T obj );
	T Load<T>( string path );
	T LoadOrNew<T>( string path ) where T : new();
	bool Exists( string path );
	void Delete( string filePath );
	void DeleteDir( string path, bool recursive );

	void DeleteAll();
}

public class BinaryFilePersistencyAdapter : IPersistencyAdapter
{
	string _defaultPath;

	public BinaryFilePersistencyAdapter( string basicPath ) { _defaultPath = basicPath; }

	public void Save<T>( string path, T obj ) { FileAdapter.WriteAllBytes( _defaultPath + path, BinaryAdapter.Serialize( obj ) ); }
	public T Load<T>( string path ) { return BinaryAdapter.Deserialize<T>( FileAdapter.ReadAllBytes( _defaultPath + path ) ); }
	public T LoadOrNew<T>( string path ) where T : new() { var ret = Load<T>( path ); return ( ret != null ) ? ret : new T(); }
	public bool Exists( string path ) { return FileAdapter.Exists( _defaultPath + path ); }
	public void Delete( string filePath ) { FileAdapter.Delete( _defaultPath + filePath ); }
	public void DeleteDir( string path, bool recursive ) { FileAdapter.DeleteDir( _defaultPath + path, recursive ); }
	public void DeleteAll() { FileAdapter.DeleteDir( _defaultPath, true ); }
}
