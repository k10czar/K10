using UnityEngine;

public class OnInputFieldValueChangedText : IEventBinderReference
{
    [SerializeField] TMPro.TMP_InputField _inputField;

    EventSlot<string> _eventSlot;

    public void Register(IEventTrigger listener)
    {
        if( _eventSlot == null )
        {
            _eventSlot = new();
            _inputField.onValueChanged.AddListener( _eventSlot.Trigger );
        }
    }

    public bool Unregister(IEventTrigger listener)
    {
        if( _eventSlot == null ) return false;
        var unregistered = _eventSlot.Unregister( listener );
        if( _eventSlot.EventsCount == 0 )
        {
            _inputField.onValueChanged.RemoveListener( _eventSlot.Trigger );
            _eventSlot = null;
        }
        return unregistered;
    }
}
