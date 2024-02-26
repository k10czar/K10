using UnityEngine;

public class TypeListDataCache<T>
{
    static float _maxWidth = -1;
    static TypeListData<T> _data = null;

    public static TypeListData<T> Data => _data ??= new TypeListData<T>();
    public static float MaxWidth
    {
        get
        {
            if( _maxWidth > -Mathf.Epsilon ) return _maxWidth;

            _maxWidth = 10;
            foreach( var lab in Data.GetGUIs() )
            {
                K10GuiStyles.basicStyle.CalcMinMaxWidth( lab, out var minW, out var maxW );
                maxW += 15;
                if( _maxWidth < maxW ) _maxWidth = maxW;
            }
            return _maxWidth;
        }
    }
}
