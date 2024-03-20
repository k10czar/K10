

using System;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PropDrawerCache
{
    static readonly System.Type PROP_TYPE = typeof(PropertyDrawer);
    static readonly FieldInfo TYPE_FIELD = PROP_TYPE.GetField( "m_Type" );
    static Dictionary<System.Type,PropertyDrawer> _drawersCache = null; //new Dictionary<string, Type>();

    /// <summary> Returns a already cached PropertyDrawer instance or create new one for for the specified type. </summary>
    /// <param name="type">The Type that you request a PropertyDrawer for</param>
    /// <returns>Best matching PropertyDrawer for a specified type</returns>
    public static PropertyDrawer From( System.Type type )
    {
        if( type == null ) return null;
        if( _drawersCache == null ) _drawersCache = new();

        if( _drawersCache.TryGetValue( type, out var drawerType ) ) return drawerType;

		var candidates = System.AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany( s => s.GetTypes() )
                        .Where( t =>  IsPropertyDrawerOf( t, type ) );

        Debug.Log( $"{"Found".Colorfy(Colors.Console.Verbs)} {candidates.Count().ToStringColored(Colors.Console.Numbers)} {"PropertyDrawer".Colorfy(Colors.Console.TypeName)} of {type.Name.Colorfy(Colors.Console.Keyword)}: {string.Join( ", ", candidates )}" );

        PropertyDrawer candidate = null;
        foreach( var pd in candidates )
        {
            candidate = pd.CreateInstance() as PropertyDrawer;
            if( candidate != null ) break;
        }

        _drawersCache.Add( type, candidate );

        return candidate;
    }

    /// <summary> Check if the input property drawer Type is a property drawer of another the given type. </summary>
    /// <param name="propertyDrawerType">Property drawer Type</param>
    /// <param name="type">Given Type</param>
    /// <returns>Returns if the input property drawer Type is a property drawer of another the given type</returns>
    private static bool IsPropertyDrawerOf(Type propertyDrawerType, Type type)
    {
        if( !propertyDrawerType.IsSubclassOf( PROP_TYPE ) ) return false;
        var att = propertyDrawerType.GetCustomAttribute<CustomPropertyDrawer>();
        if( att == null ) return false;
        var attType = TYPE_FIELD.GetValue( att ) as System.Type;
        if( attType == null ) return false;
        return propertyDrawerType.IsSubclassOf( attType );
    }
}