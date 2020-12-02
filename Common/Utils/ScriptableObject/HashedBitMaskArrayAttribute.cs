using System;
using UnityEngine;

[AttributeUsage( AttributeTargets.Field, AllowMultiple = true )]
public class HashedBitMaskArrayAttribute : PropertyAttribute
{
	public readonly System.Type propType;

	public HashedBitMaskArrayAttribute( System.Type aType ) { propType = aType; }
}

[AttributeUsage( AttributeTargets.Field, AllowMultiple = true )]
public class HashedSOBitMaskAttribute : PropertyAttribute
{
	public readonly System.Type propType;

	public HashedSOBitMaskAttribute( System.Type aType ) { propType = aType; }
}
