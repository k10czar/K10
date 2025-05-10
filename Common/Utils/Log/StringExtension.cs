using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum ELogType
{
    Basic,
    Warning,
    Error
}

public static class StringExtension
{
    static readonly HashSet<char> INVALID_FILENAME_CHARS = new HashSet<char>( System.IO.Path.GetInvalidFileNameChars() );
    static readonly HashSet<char> INVALID_PATH_CHARS = new HashSet<char>( System.IO.Path.GetInvalidPathChars() );
    
    public static void Log( this string[] log, ELogType logType = ELogType.Basic )
    {
        if( log == null ) return;
        for( int i = 0; i < log.Length; i++ ) log[i].Log( logType );
    }

    public static void Log( this string log, ELogType logType = ELogType.Basic )
    {
        switch( logType )
        {
            case ELogType.Basic: UnityEngine.Debug.Log( log ); break;
            case ELogType.Warning: UnityEngine.Debug.LogWarning( log ); break;
            case ELogType.Error: UnityEngine.Debug.LogError( log ); break;
        }
    }
    
    public static string SanitizeFileName( this string fileName ) => RemoveChars( fileName, INVALID_FILENAME_CHARS );
    public static string SanitizePathName( this string fileName ) => RemoveChars( fileName, INVALID_PATH_CHARS );

    public static string[] Numerated<T>( this IEnumerable<T> objs )
    {
        var count = objs.Count();
        var strs = new string[count];
        var i = 0;
        foreach( var obj in objs )
        {
            strs[i] = $"{i}) {obj.ToStringOrNull()}";
            i++;
        }
        return strs;
    }

    public static string RemoveChars( this string fileName, HashSet<char> charsToRemove )
    {
        // return new string( fileName.Where( c => !charsToRemove.Contains(c) ) );
        var invalidChars = 0;
        for( int i = 0; i < fileName.Length; i++ ) if( charsToRemove.Contains( fileName[i] ) ) invalidChars++;
        if( invalidChars == 0 ) return fileName;
        var newName = new char[ fileName.Length - invalidChars ];
        var it = 0;
        for( int i = 0; i < fileName.Length; i++ ) if( !charsToRemove.Contains( fileName[i] ) ) newName[it++] = fileName[i];
        return new string( newName );
    }
    
    public static bool IsCommand( this string str, out string commandParameter, params string[] commands )
    {
        if( commands != null )
        {
            for( int i = 0; i < commands.Length; i++ )
            {
                var cmd = commands[i];
                if( str.StartsWith( cmd, System.StringComparison.OrdinalIgnoreCase ) )
                {
                    var toRemove = cmd.Length;
					var nextChar = str[toRemove];
                    if( nextChar == ':' || nextChar == '=' || nextChar == '-' || nextChar == '|' ) toRemove++;
                    commandParameter = str.Substring( toRemove, str.Length - toRemove );
                    return true;
                }
            }
        }
        commandParameter = null;
        return false;
    }

    public static bool AsPathIsFilename( this string path, string name, System.StringComparison comparison = System.StringComparison.OrdinalIgnoreCase )
    {
        var dx = 0;
		for( int i = path.Length - 1; i >= 0; i-- )
		{
			if( path[i] == '.' ) 
			{
				dx = path.Length - i;
				break;
			}
			if( path[i] == '/' ) break;
			if( path[i] == '\\' ) break;
		}
        var pathSize = path.Length;
        var minPathSize = ( ( name?.Length ?? 0 ) + dx );
        if( pathSize < minPathSize ) return false;
        var pIt = pathSize - minPathSize;
        for( int pi = 0; pi < name.Length; pi++ )
        {
            if( string.Compare( path, pIt, name, 0, name.Length, comparison ) != 0 ) return false;
        }
        var barPos = pIt - 1;
        if( barPos > 0 && path[barPos] != '/' && path[barPos] != '\\' ) return false;
        return true;
    }
}
