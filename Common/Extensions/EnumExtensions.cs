using System.Reflection;
using System;

public static class EnumExtensions
{
    public static string FlagsToStringWithAllCheck<T>( this T enumValue, string separator = ", " ) where T : Enum, IConvertible
    {
        if( enumValue.ToInt32( null ) == -1 ) return $"All {enumValue.TypeNameOrNull()}";
        return FlagsToString( enumValue, separator );
    }

    public static string FlagsToString( this Enum enumValue, string separator = ", " )
    {
        Type type = enumValue.GetType();
        if (!type.IsDefined(typeof(FlagsAttribute), false))
        {
            throw new ArgumentException("The specified enum does not have the [Flags] attribute.");
        }

        var SB = StringBuilderPool.RequestEmpty();

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (enumValue.HasFlag((Enum)field.GetValue(null)))
            {
                if( SB.Length != 0 ) SB.Append( separator );
                SB.Append( field.Name );
            }
        }

        return SB.ReturnToPoolAndCast();
    }
}
