using System;
using System.Collections.Generic;
using System.Linq;

public static class TypeFinder
{
    const StringComparison COMPARISON = StringComparison.Ordinal;
    static Dictionary<string,Type> _nameToTypeCache = null; //new Dictionary<string, Type>();

    public static Type GetGenericType( this IEnumerable<System.Reflection.Assembly> assemblies, string typename )
    {
        foreach( var assembly in assemblies )
        {
            var type = assembly.GetGenericType( typename );
            if( type != null ) return null;
        }
        return null;
    }

    public static Type GetGenericType( this System.Reflection.Assembly assembly, string typename )
    {
        foreach( var t in assembly.GetTypes() )
        {
            var iterName = t.FullName;
            if( typename.StartsWith( iterName, COMPARISON ) ) 
            {
                var tLen = typename.Length;
                var itLen = iterName.Length;
                if( tLen == itLen ) return t;
                if( tLen > itLen ) 
                {
                    var typesCount = 0;
                    var it = itLen - 1;
                    var power = 1;
                    while( it >= 0 && char.IsNumber( iterName[it] ) ) 
                    {
                        var digit = iterName[it] - '0';
                        typesCount += digit * power;
                        power *= 10;
                        it--;
                    }
                    var subName = typename.Substring( itLen + 1, typename.Length - ( itLen + 2 ) );
                    var types = GetTypesArray( subName, typesCount );
                    // UnityEngine.Debug.Log( $"{typename} {"starts".Colorfy( Colors.Console.Verbs )} with {iterName} trying {string.Join<Type>(",",types)}" );
                    try
                    {
                        var gt = t.MakeGenericType( types );
                        return gt;
                    }
                    catch( Exception e )
                    {
                        UnityEngine.Debug.LogError( $"{typename} GetTypesArray( {subName}, {typesCount} ) {"starts".Colorfy( Colors.Console.Verbs )} with {iterName} trying {string.Join<Type>(",",types)} throws {e}" );
                        UnityEngine.Debug.LogException( e );
                        continue;
                    }
                }
            }
        }
        return null;
    }
    
    public static Type[] GetTypesArray( string typename, int count )
    {
        UnityEngine.Debug.Log( $"{"GetTypesArray".Colorfy( Colors.Console.Verbs )}( {typename}, {count.ToStringColored( Colors.Console.Numbers)} )" );
        var it = 0;
        var itA = 0;
        var len = typename.Length;
        var level = 0;
        var itB = itA + 1;
        var types = new Type[count];
        while( itB < len && itA < len && it < count )
        {
            while( level == 0 && itA < len && typename[itA] != '[' ) itA++;
            level++;
            itB = itA + 1;
            while( level > 0 && itB < len ) 
            {
                if( typename[itB] == '[' ) level++;
                if( typename[itB] == ']' ) level--;
                itB++;
            }
            var typeStr = typename.Substring( itA + 1, itB - ( itA + 2 ) );
            // UnityEngine.Debug.Log( $"{"GetTypesArray".Colorfy( Colors.Console.Verbs )}[ {it} ] => {typeStr}" );
            types[it] = WithName( typeStr );
            // UnityEngine.Debug.Log( $"{"GetTypesArray".Colorfy( Colors.Console.Verbs )}[ {it} ] => {typeStr} => {types[it].ToStringOrNull()}" );
            itA = itB + 1;
            it++;
        }
        return types;
    }

    public static Type WithName( string typename )
    {
        if( _nameToTypeCache == null ) _nameToTypeCache = new();

        if( _nameToTypeCache.TryGetValue( typename, out var type ) ) return type;

		type = Type.GetType( typename );
        if( type == null ) type = AppDomain.CurrentDomain.GetAssemblies()
                                    .GetGenericType( typename );;

        _nameToTypeCache.Add( typename, type );

        return type;
    }

    public static Type WithNameFromAssembly( string typename, string assemblyName )
    {
        if( _nameToTypeCache == null ) _nameToTypeCache = new Dictionary<string, Type>();

        if( _nameToTypeCache.TryGetValue( typename, out var type ) ) return type;

		type = Type.GetType( typename );
        if( type == null ) type = AppDomain.CurrentDomain.GetAssemblies()
                                    .FirstOrDefault( a => string.Equals( assemblyName, a.GetName().Name, COMPARISON ) )?
                                    .GetGenericType( typename );

        _nameToTypeCache.Add( typename, type );

        return type;
    }
}
