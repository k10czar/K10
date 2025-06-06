using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TypeExtensions
{

    public static bool Implements( this System.Type type, System.Type interfaceType )
    {
        foreach( var typeInterface in type.GetInterfaces() ) if( typeInterface == interfaceType ) return true;
        return false;
    }

    public static string TypeNameOrNull( this object type, string nullString = ConstsK10.NULL_STRING )
    {
        if( type == null ) return nullString;
        return type.GetType().Name;
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
