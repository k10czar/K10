using System;
using System.Collections.Generic;

public static class EnumCache<T> where T : Enum, IConvertible
{
    static IReadOnlyList<T> _cachedValues = null;
    static IReadOnlyList<string> _cachedNames = null;
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
}