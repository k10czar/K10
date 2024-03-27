using System;
using System.Reflection;
using UnityEngine;


public class ToStringAttribute : PropertyAttribute { }
public class ExtendedDrawerAttribute : PropertyAttribute { }

public class ListingPathAttribute : Attribute
{
	public string Path { get; }

	public ListingPathAttribute( string path )
	{
		Path = path;
	}
}


public class OverridingColorAttribute : Attribute
{
	public UnityEngine.Color Color { get; }

	public OverridingColorAttribute( UnityEngine.Color color )
	{
		Color = color;
	}

	public static UnityEngine.Color TryGetColorFrom( object obj, UnityEngine.Color defaultColor )
	{
		return TryGetColorFrom( obj?.GetType(), defaultColor );
	}

	public static UnityEngine.Color TryGetColorFrom( System.Type type, UnityEngine.Color defaultColor )
	{
		var att = type.GetCustomAttribute<OverridingColorAttribute>();
		if( att == null ) return defaultColor;
		return att.Color;
	}
}