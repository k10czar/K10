using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System;

public static class PropertyDrawerUtility
{
    private static MethodInfo getDrawerTypeForTypeMethod;
    private static LimitedCache<Type,PropertyDrawer> _cache = new( TimeSpan.FromMinutes( 5 ) );

    static PropertyDrawerUtility()
    {
        // Get the internal ScriptAttributeUtility type using its assembly-qualified name
        Type scriptAttributeUtilityType = Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor");
        
        if (scriptAttributeUtilityType != null)
        {
            // Retrieve the non-public, static method "GetDrawerTypeForType"
            getDrawerTypeForTypeMethod = scriptAttributeUtilityType.GetMethod(
                "GetDrawerTypeForType", 
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
            );
			Debug.Log( $"Found GetDrawerTypeForType: {getDrawerTypeForTypeMethod}( {getDrawerTypeForTypeMethod?.GetParameters().ToStringOrNull() ?? "NULL"} )" );
        }
		else
		{
			Debug.Log( $"Does not found UnityEditor.ScriptAttributeUtility" );
		}
    }

    /// <summary>
    /// Finds the custom PropertyDrawer type for a given System.Type.
    /// Returns null if no custom drawer is found (meaning the default drawer will be used).
    /// </summary>
    public static PropertyDrawer GetPropertyDrawerTypeForType(Type type, params Type[] ignoreTypes)
    {
		if( _cache.TryGetValue( type, out PropertyDrawer propertyDrawer) ) return propertyDrawer;

        if (getDrawerTypeForTypeMethod == null)
        {
            return null;
        }

        // Invoke the internal method to get the drawer type
        object[] parameters = new object[] { type, ignoreTypes, true };
        Type drawerType = (Type)getDrawerTypeForTypeMethod.Invoke(null, parameters);
        
        // Ensure the returned type is actually a PropertyDrawer and not just the default one (null or System.Object's default)
        if (drawerType != null && typeof(PropertyDrawer).IsAssignableFrom(drawerType))
        {
			var drawer = (PropertyDrawer)drawerType.CreateInstance();
			_cache.Add( type, drawer );
			Debug.Log( $"Found {drawerType} as a PropertyDrawer for {type}" );
            return drawer;
        }

		_cache.Add( type, null );
        return null;
    }
}
