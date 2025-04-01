using System;
using UnityEngine;

public class ToStringAttribute : PropertyAttribute { }
public class ExtendedDrawerAttribute : PropertyAttribute
{
	public readonly bool ShowName;

	public ExtendedDrawerAttribute( bool showName = false )
	{
		ShowName = showName;
	}
}
public class BoxedAttribute : PropertyAttribute
{
    public string ColorName { get; private set; }

	public BoxedAttribute()
	{
		ColorName = null;
	}

	public BoxedAttribute( string colorName )
	{
		ColorName = colorName;
	}
}

public class OnlyOnPlayAttribute : PropertyAttribute { }

public class RandomizeButtonAttribute : PropertyAttribute
{
	// public readonly object min = null;
	// public readonly object max = null;

	// public RandomizeButtonAttribute( object min = null, object max = null )
	// {
	// 	this.min = min;
	// 	this.max = max;
	// }
}

public class UnitAttribute : PropertyAttribute
{
	public readonly string UnitName;

	public UnitAttribute( string unitName )
	{
		UnitName = unitName;
	}
}

public class ListingPathAttribute : Attribute
{
	public string Path { get; }

	public ListingPathAttribute( string path )
	{
		Path = path;
	}
}

public class LocalPosition : PropertyAttribute {}