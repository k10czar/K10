using System.Collections.Generic;
using System;
public static class EnumToStringsCache<T> where T : Enum, IConvertible
{
    static Dictionary<T, string> _cachedToStrings = new();

    public static string Get( T enumValue )
    {
        if (!_cachedToStrings.TryGetValue(enumValue, out var str))
        {
            str = enumValue.ToString();
            _cachedToStrings[enumValue] = str;
        }
        return str;
    }
}
