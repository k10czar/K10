using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputActionReferenceEventBinder : IEventBinderReference, ISummarizable
{
    [SerializeField] InputActionReference _actionRef;

    EventSlot _eventSlot;
    private Action _removeAction;

    private void RequestEventBind()
    {
        Lazy.Request( ref _eventSlot );
        if( _removeAction == null ) _removeAction = _actionRef.action.RegisterTrigger( _eventSlot );
    }

    public void Register(IEventTrigger listener)
    {
        RequestEventBind();
        _eventSlot.Register( listener );
    }

    public bool Unregister(IEventTrigger listener)
    {
        var removed = _eventSlot.Unregister( listener );
        if( _eventSlot.EventsCount > 0 ) return removed;
        _removeAction();
        _removeAction = null;
        return removed;
    }

    public string Summarize()
    {
        if( _actionRef == null ) return "UNSETTED"; 
        if( _actionRef.action == null ) return "NULL ACTION"; 
        return $"{_actionRef.action.actionMap.name}/{_actionRef.action.name}";
    }
}
