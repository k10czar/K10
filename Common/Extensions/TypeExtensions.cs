using UnityEngine;

public static class TypeExtensions
{
    const string NULL_STRING = K10UnityExtensions.NULL_STRING;

    public static bool Implements( this System.Type type, System.Type interfaceType )
    {
        foreach( var typeInterface in type.GetInterfaces() ) if( typeInterface == interfaceType ) return true;
        return false;
    }

    public static string TypeNameOrNull( this object type, string nullString = NULL_STRING )
    {
        if( type == null ) return nullString;
        return type.GetType().Name;
    }

	public static string TypeNameOrNullColored( this object obj, string nullString = NULL_STRING ) => TypeNameOrNullColored( obj, Colors.Console.TypeName, Colors.Console.Negation, nullString );
	public static string TypeNameOrNullColored( this object obj, Color color, string nullString = NULL_STRING ) => TypeNameOrNullColored( obj, color, Colors.Console.Negation, nullString );
    public static string TypeNameOrNullColored( this object obj, Color color, Color nullColor, string nullString = NULL_STRING )
    {
        if( obj == null ) return nullString.Colorfy( nullColor );
        return obj.GetType().Name.Colorfy( color );
    }

    public static object CreateInstance( this System.Type type ) => ( type != null ) ? System.Activator.CreateInstance( type ) : null;
}
