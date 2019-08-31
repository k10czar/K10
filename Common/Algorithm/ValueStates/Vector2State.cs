using UnityEngine;

[System.Serializable]
public class Vector2State : IValueState<Vector2>
{
    [SerializeField] Vector2 _value;
    [System.NonSerialized] EventSlot<Vector2> _onChange = new EventSlot<Vector2>();

    public Vector2 Value { get { return _value; } set { Setter( value ); } }
    public Vector2 Get() { return _value; }

    public void Setter( Vector2 value )
    {
        if( Mathf.Approximately( _value.x, value.x ) && Mathf.Approximately( _value.y, value.y ) ) return;
        _value = value;
        _onChange.Trigger( value );
    }

    public IEventRegister<Vector2> OnChange { get { return _onChange; } }

    public Vector2State( Vector2 initialValue = default( Vector2 ) ) { _value = initialValue; }

    public override string ToString() { return string.Format( "V2S({1})", typeof( Vector2 ).ToString(), _value ); }
}