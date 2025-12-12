using System;
using UnityEngine;

public class WaitForEventSlot : CustomYieldInstruction
{
    private readonly IEvent targetEvent;
    private readonly Func<bool> validator;

    private bool eventTriggered = false;

    public override bool keepWaiting => !eventTriggered;

    private void Triggered()
    {
        if (validator != null && !validator.Invoke())
            return;

        eventTriggered = true;
        targetEvent.Unregister(Triggered);
    }

    public WaitForEventSlot(IEvent targetEvent)
    {
        this.targetEvent = targetEvent;
        targetEvent.Register(Triggered);
    }

    public WaitForEventSlot(IEvent targetEvent, Func<bool> validator)
    {
        this.targetEvent = targetEvent;
        this.validator = validator;

        targetEvent.Register(Triggered);
    }
}