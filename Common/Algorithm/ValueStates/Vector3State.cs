using UnityEngine;

[System.Serializable]
public class Vector3State : IValueState<Vector3>
{
    [SerializeField] Vector3 _value;
    [System.NonSerialized] EventSlot<Vector3> _onChange = new EventSlot<Vector3>();

    public Vector3 Value { get { return _value; } set { Setter( value ); } }
    public Vector3 Get() { return _value; }

    public void Setter( Vector3 value )
    {
        if( Mathf.Approximately( _value.x, value.x ) && Mathf.Approximately( _value.y, value.y ) && Mathf.Approximately( _value.z, value.z ) ) return;
        _value = value;
        _onChange.Trigger( value );
    }

    public IEventRegister<Vector3> OnChange { get { return _onChange; } }

    public Vector3State( Vector3 initialValue = default( Vector3 ) ) { _value = initialValue; }

    public override string ToString() { return string.Format( "V3S({1})", typeof( Vector3 ).ToString(), _value ); }
}