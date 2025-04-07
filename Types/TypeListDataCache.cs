using System.Collections.Generic;
using UnityEngine;

public class TypeListDataCache
{
    static Dictionary<System.Type,TypeListData> _data = null;
    static Dictionary<(System.Type,System.Type),TypeListData> _doubleData = null;

    public static TypeListData GetFrom( System.Type type )
    {
        if( _data == null ) _data = new();
        if( !_data.TryGetValue( type, out var typeListData ) )
        {
            typeListData = new TypeListData( type );
            _data.Add( type, typeListData );
        }

        return typeListData;
    }

    public static TypeListData GetFrom( System.Type type1, System.Type type2 )
    {
        if( _doubleData == null ) _doubleData = new();
        var types = ( type1, type2 );
        if( !_doubleData.TryGetValue( types, out var typeListData ) )
        {
            typeListData = new TypeListData( type1, type2 );
            _doubleData.Add( types, typeListData );
        }

        return typeListData;
    }
}



public class TypeListData<T>
{
    public static TypeListData Data => TypeListDataCache.GetFrom( typeof(T) );
    private TypeListData() {}
}

public class TypeListData<T,K>
{
    public static TypeListData Data => TypeListDataCache.GetFrom( typeof(T), typeof(K) );
    private TypeListData() {}
}
