using System.Collections.Generic;
using UnityEngine;

public class TypeListDataCache
{
    static Dictionary<System.Type,TypeListData> _data = null;

    public static TypeListData GetFrom( System.Type type )
    {
        if( _data == null ) _data = new Dictionary<System.Type, TypeListData>();
        if( !_data.TryGetValue( type, out var typeListData ) )
        {
            typeListData = new TypeListData( type );
            _data.Add( type, typeListData );
        }

        return typeListData;
    }
}

public class TypeListData<T>
{
    public static TypeListData Data => TypeListDataCache.GetFrom( typeof(T) );
}
