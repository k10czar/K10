using System;
using System.Reflection;
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

public class EnumEntry : PropertyAttribute
{
	public readonly Type enumType;
	public readonly EConsoleColor color;

	public EnumEntry(Type enumType, EConsoleColor color = EConsoleColor.Primary)
	{
		this.enumType = enumType;
		this.color = color;
	}

	public EnumEntry(EConsoleColor color = EConsoleColor.Primary)
	{
		this.enumType = null;
		this.color = color;
	}
}

public class EnumAndFieldEntry : PropertyAttribute
{
	public readonly Type firstType;
	public readonly Type secondType;

	public readonly EConsoleColor firstColor;
	public readonly EConsoleColor secondColor;

	public readonly string firstHint;
	public readonly string secondHint;

	public EnumAndFieldEntry(Type firstType, Type secondType, EConsoleColor firstColor = EConsoleColor.Primary, EConsoleColor secondColor = EConsoleColor.Support, string firstHint = null, string secondHint = null)
	{
		this.firstType = firstType;
		this.secondType = secondType;
		this.firstColor = firstColor;
		this.secondColor = secondColor;
		this.firstHint = firstHint;
		this.secondHint = secondHint;
	}
}

public class EnumAndMaskEntry : PropertyAttribute
{
	public readonly Type firstType;
	public readonly Type secondType;

	public readonly EConsoleColor firstColor;
	public readonly EConsoleColor secondColor;

	public static EnumAndFieldEntry From<T,U>(EConsoleColor firstColor = EConsoleColor.Primary, EConsoleColor secondColor = EConsoleColor.Support)
		=> new(typeof(T), typeof(U), firstColor, secondColor);

	public EnumAndMaskEntry(Type firstType, Type secondType, EConsoleColor firstColor = EConsoleColor.Primary, EConsoleColor secondColor = EConsoleColor.Support)
	{
		this.firstType = firstType;
		this.secondType = secondType;
		this.firstColor = firstColor;
		this.secondColor = secondColor;
	}
}

public class LocalPosition : PropertyAttribute {}