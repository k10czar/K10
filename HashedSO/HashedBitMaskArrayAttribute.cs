using System;
using UnityEngine;

[AttributeUsage( AttributeTargets.Field, AllowMultiple = true )]
public class HashedElementFilterBitsAttribute : PropertyAttribute
{
	public readonly System.Type propType;

	public HashedElementFilterBitsAttribute( System.Type aType ) { propType = aType; }
}
