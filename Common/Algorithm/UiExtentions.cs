using UnityEngine.UI;

public static class UiExtentions
{
    public static void Sync( this Toggle t, IValueState<bool> value )
    {
        if( t == null ) return;
        t.isOn = value.Value;
        t.onValueChanged.AddListener( value.Setter );
    }

    public static void Sync( this Slider t, IValueState<float> value )
    {
        if( t == null ) return;
        t.value = value.Value;
        t.onValueChanged.AddListener( value.Setter );
    }
}