using System;
using System.Collections.Generic;
using K10.EventSystem;
using UnityEngine;

public class WaitForAnyEvent : CustomYieldInstruction
{
    private readonly ActionSet listeners = new();
    private readonly Func<bool> validator;

    private bool eventTriggered = false;

    public override bool keepWaiting => !eventTriggered;

    private void Triggered()
    {
        if (validator != null && !validator.Invoke())
            return;

        eventTriggered = true;
        listeners.Void();
    }

    public WaitForAnyEvent(IEnumerable<IEventRegister> events)
    {
        foreach (var targetEvent in events)
            listeners.Register(targetEvent, Triggered);
    }

    public WaitForAnyEvent(params IEventRegister[] events)
    {
        foreach (var targetEvent in events)
            listeners.Register(targetEvent, Triggered);
    }

    public WaitForAnyEvent(IEventRegister targetEvent, params object[] keys)
    {
        var filterCapsule = targetEvent.RegisterFiltered(Triggered);
        filterCapsule.SetKeys(keys);
        listeners.Track((ActionCapsuleBase)filterCapsule);
    }

    public WaitForAnyEvent(Func<bool> validator, params IEventRegister[] events)
        : this(events)
    {
        this.validator = validator;
    }
}