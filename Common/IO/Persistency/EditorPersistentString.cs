using System.Collections.Generic;
using System.IO;
using System.Text;

public class EditorPersistentString
{
	string _relativePath;
	string _cachedValue;
	bool _loaded;

	public string PathToUse => FullPath( _relativePath );
	static string FullPath( string relativePath ) => "Temp/Editor/" + relativePath;

	static readonly Dictionary<string, EditorPersistentString> _dict = new();

	public static bool Exists( string relativePath ) => _dict.ContainsKey( relativePath ) || File.Exists( FullPath( relativePath ) );

	public static EditorPersistentString At( string relativePath, string startValue )
	{
		var has = Exists( relativePath );
		var ret = At( relativePath );
		if( !has ) ret.Set = startValue;
		return ret;
	}

	public static EditorPersistentString At( string relativePath )
	{
		if( !_dict.TryGetValue( relativePath, out var val ) )
		{
			val = new EditorPersistentString( relativePath );
			_dict[relativePath] = val;
		}
		return val;
	}

	EditorPersistentString( string relativePath )
	{
		_relativePath = relativePath;
	}

	public string Get
	{
		get
		{
			if( !_loaded )
			{
				_loaded = true;
				var fullPath = PathToUse;
				_cachedValue = File.Exists( fullPath ) ? File.ReadAllText( fullPath, Encoding.UTF8 ) : string.Empty;
			}
			return _cachedValue;
		}
	}

	public string Set
	{
		set
		{
			if( _loaded && _cachedValue == value ) return;
			_cachedValue = value;
			_loaded = true;
			var filePath = PathToUse;
			int div = filePath.Length - 1;
			while( div > 0 && filePath[div] != '/' && filePath[div] != '\\' ) div--;
			var dir = filePath[..div];
			if( !Directory.Exists( dir ) ) Directory.CreateDirectory( dir );
			File.WriteAllText( filePath, value ?? string.Empty, Encoding.UTF8 );
		}
	}
}
