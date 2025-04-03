using UnityEngine;

public class WaitForEventSlot : CustomYieldInstruction
{
    private readonly IEvent targetEvent;
    private bool eventTriggered = false;

    public override bool keepWaiting => !eventTriggered;

    private void Triggered()
    {
        eventTriggered = true;
        targetEvent.Unregister(Triggered);
    }

    public WaitForEventSlot(IEvent targetEvent)
    {
        this.targetEvent = targetEvent;
        targetEvent.Register(Triggered);
    }
}