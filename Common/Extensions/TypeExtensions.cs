public static class TypeExtensions
{
    const string NULL_STRING = "NULL";

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

    public static object CreateInstance( this System.Type type ) => ( type != null ) ? System.Activator.CreateInstance( type ) : null;
}
