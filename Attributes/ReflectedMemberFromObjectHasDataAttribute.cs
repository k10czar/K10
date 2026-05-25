using System.Reflection;


public abstract class ReflectedMemberFromObjectHasDataAttribute : BaseFieldValidationAttribute
{
	public readonly string MemberName;
	public readonly BindingFlags BindingFlags;

    public ReflectedMemberFromObjectHasDataAttribute( string memberName, BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, string errorMessage = null, string validMessage = null ) : base( errorMessage, validMessage )
	{
		this.MemberName = memberName;
		this.BindingFlags = bindingFlags;
	}
}

public class FieldFromObjectHasDataAttribute : ReflectedMemberFromObjectHasDataAttribute
{
    public FieldFromObjectHasDataAttribute( string fieldName, BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, string errorMessage = null, string validMessage = null ) : base( fieldName, bindingFlags, errorMessage, validMessage ) { }
}

public class PropertyFromObjectHasDataAttribute : ReflectedMemberFromObjectHasDataAttribute
{
    public PropertyFromObjectHasDataAttribute( string propName, BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, string errorMessage = null, string validMessage = null ) : base( propName, bindingFlags, errorMessage, validMessage ) { }
}
