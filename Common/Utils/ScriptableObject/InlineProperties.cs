using System;
using UnityEngine;

[AttributeUsage( AttributeTargets.Field, AllowMultiple = true )]
public sealed class InlinePropertiesAttribute : PropertyAttribute
{
	public readonly bool boxed;

	public InlinePropertiesAttribute( bool boxed = false ) 
	{
		this.boxed = boxed;
	}
}