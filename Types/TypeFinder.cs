using System;
using System.Collections.Generic;
using System.Linq;

public static class TypeFinder
{
    static Dictionary<string,System.Type> _nameToTypeCache = null; //new Dictionary<string, Type>();

    public static System.Type WithName( string typename, StringComparison comparison = StringComparison.Ordinal )
    {
        if( _nameToTypeCache == null ) _nameToTypeCache = new();

        if( _nameToTypeCache.TryGetValue( typename, out var type ) ) return type;

		type = System.Type.GetType( typename ) ?? System.AppDomain.CurrentDomain.GetAssemblies()
                                                    .SelectMany( s => s.GetTypes() )
                                                    .FirstOrDefault( t => string.Equals( typename, t.Name, comparison ) );

        _nameToTypeCache.Add( typename, type );

        return type;
    }

    public static System.Type WithNameFromAssembly( string typename, string assemblyName, StringComparison comparison = StringComparison.Ordinal )
    {
        if( _nameToTypeCache == null ) _nameToTypeCache = new Dictionary<string, Type>();

        if( _nameToTypeCache.TryGetValue( typename, out var type ) ) return type;

		type = System.Type.GetType( typename ) ?? System.AppDomain.CurrentDomain.GetAssemblies()
                                                    .FirstOrDefault( a => string.Equals( assemblyName, a.GetName().Name, comparison ) )?
                                                    .GetTypes().FirstOrDefault( t => string.Equals( typename, t.FullName, comparison ) );

        _nameToTypeCache.Add( typename, type );

        return type;
    }
}
