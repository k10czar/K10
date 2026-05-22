using System;
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

    public WaitForAnyEvent(params IEventRegister[] events)
    {
        foreach (var targetEvent in events)
            listeners.Register(targetEvent, Triggered);
    }

    public WaitForAnyEvent(Func<bool> validator, params IEventRegister[] events)
        : this(events)
    {
        this.validator = validator;
    }
}