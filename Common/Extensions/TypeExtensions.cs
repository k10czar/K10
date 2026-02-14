using System;
using System.Linq;
using UnityEngine;

public static class TypeExtensions
{
    const int MAX_DEPTH = 10;

    public static bool Implements( this System.Type type, System.Type interfaceType )
    {
        foreach( var typeInterface in type.GetInterfaces() ) if( typeInterface == interfaceType ) return true;
        return false;
    }

    public static string TypeNameOrNull( this object type, string nullString = ConstsK10.NULL_STRING )
    {
        if( type == null ) return nullString;
        return type.GetType().NameOrNull();
    }

    public static string NameOrNull( this Type type, string nullString = ConstsK10.NULL_STRING, int depth = 0 )
    {
        if( type == null ) return nullString;
        if( type.IsGenericType )
        {
            depth++;
            if( depth > MAX_DEPTH ) return "...";
            return $"{type.Name.Split('`')[0]}<{string.Join(",",Array.ConvertAll( type.GenericTypeArguments, ( t ) => t.NameOrNull(nullString,depth) ))}>";
        }
        return type.Name;
    }

    public static object CreateInstance(this System.Type type)
    {
        if( type == null ) return null;
        var instance = System.Activator.CreateInstance(type);
        return instance;
    }

    public static bool InheritsOrImplements(this object obj, Type baseType) => obj != null && obj.GetType().InheritsOrImplements( baseType );
    public static bool InheritsOrImplements(this Type type, Type baseType) {
        type = ResolveGenericType(type);
        baseType = ResolveGenericType(baseType);

        while (type != typeof(object)) {
            if (baseType == type || HasAnyInterfaces(type, baseType)) return true;
                
            type = ResolveGenericType(type.BaseType);
            if (type == null) return false;
        }
            
        return false;
    }

        
    static Type ResolveGenericType(Type type) {
        if (type is not { IsGenericType: true }) return type;

        var genericType = type.GetGenericTypeDefinition();
        return genericType != type ? genericType : type;
    }

    static bool HasAnyInterfaces(Type type, Type intefaceType) {
        return type.GetInterfaces().Any(i => ResolveGenericType(i) == intefaceType);
    }
    
	public static string TypeNameOrNullColored( this object obj, string nullString = ConstsK10.NULL_STRING ) => TypeNameOrNullColored( obj, Colors.Console.TypeName, Colors.Console.Negation, nullString );
	public static string TypeNameOrNullColored( this object obj, Color color, string nullString = ConstsK10.NULL_STRING ) => TypeNameOrNullColored( obj, color, Colors.Console.Negation, nullString );
    public static string TypeNameOrNullColored( this object obj, Color color, Color nullColor, string nullString = ConstsK10.NULL_STRING )
    {
        if( obj == null ) return nullString.Colorfy( nullColor );
        return obj.GetType().Name.Colorfy( color );
    }
}
