using System;
using System.Collections.Generic;

public static class EnumCache
{
    public static IReadOnlyList<T> GetValues<T>() where T : Enum, IConvertible => EnumCache<T>.GetValues();
    public static IReadOnlyList<string> GetNames<T>() where T : Enum, IConvertible => EnumCache<T>.GetNames();
    public static int GetCount<T>() where T : Enum, IConvertible => EnumCache<T>.GetCount();
    public static string GetCachedNameFrom<T>(T t) where T : Enum, IConvertible => EnumCache<T>.GetCachedNameFrom( t );
    public static string GetCachedNameFrom<T>(string format, T t) where T : Enum, IConvertible => EnumCache<T>.GetCachedNameFrom( format, t );
}

public static class EnumCache<T> where T : Enum, IConvertible
{
    static IReadOnlyList<T> _cachedValues = null;
    static IReadOnlyList<string> _cachedNames = null;
    static Dictionary<T,string> _cachedNamesDict = null;
    static Dictionary<string,Dictionary<T,string>> _cachedFormatedNamesDict = null;
    static int _count = -1;

    public static IReadOnlyList<T> GetValues()
    {
        if (_cachedValues == null)
        {
            var reference = Enum.GetValues(typeof(T));
            var list = new List<T>(reference.Length);
            foreach (var e in reference) list.Add((T)e);
            _cachedValues = list;
        }
        return _cachedValues;
    }

    public static string GetCachedNameFrom( T t )
    {
        if( _cachedNamesDict == null )
        {
            _cachedNamesDict = new Dictionary<T,string>();
        }
        if( !_cachedNamesDict.TryGetValue(t, out var name ) )
        {
            name = t.ToString();
            _cachedNamesDict.Add(t, name);
        }
        return name;
    }

    public static string GetCachedNameFrom( string format, T t )
    {
        if( _cachedFormatedNamesDict == null )
        {
            _cachedFormatedNamesDict = new();
        }
        if( !_cachedFormatedNamesDict.TryGetValue(format, out var dict ) )
        {
            dict = new();
            _cachedFormatedNamesDict.Add(format, dict);
        }
        if( !dict.TryGetValue(t, out var name ) )
        {
            name = string.Format( format, t.ToString() );
            dict.Add(t, name);
        }
        return name;
    }

    public static IReadOnlyList<string> GetNames()
    {
        if (_cachedValues == null)
        {
            var reference = Enum.GetValues(typeof(T));
            var list = new List<T>(reference.Length);
            foreach (var e in reference) list.Add((T)e);
            _cachedValues = list;
        }
        if (_cachedNames == null)
        {
            var names = new List<string>();
            foreach (var e in _cachedValues) names.Add( e.ToString() );
            _cachedNames = names;
        }
        return _cachedNames;
    }

    public static string GetCachedNameFromId( int id )
    {
        return GetNames()[id];
    }

    public static int GetCount()
    {
        if (_count < 0)
        {
            if( _cachedValues != null ) _count = _cachedValues.Count;
            else _count = Enum.GetValues(typeof(T)).Length;
        }
        return _count;
    }

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