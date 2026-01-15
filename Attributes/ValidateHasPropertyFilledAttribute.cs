using System;
using UnityEngine;

[AttributeUsage( AttributeTargets.Field, AllowMultiple = true )]
public abstract class BaseFieldValidationAttribute : PropertyAttribute
{
	public readonly string ErrorMessage;
	public readonly string ValidMessage;

    public BaseFieldValidationAttribute( string errorMessage = null, string validMessage = null ) 
	{
		this.ErrorMessage = errorMessage;
		this.ValidMessage = validMessage;
	}
}

[AttributeUsage( AttributeTargets.Field, AllowMultiple = true )]
public sealed class FieldHasDataAttribute : BaseFieldValidationAttribute
{
	public readonly string PropName;

    public FieldHasDataAttribute( string propName, string errorMessage = null, string validMessage = null ) : base( errorMessage, validMessage )
	{
		this.PropName = propName;
	}
}