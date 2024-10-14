using UnityEngine;

public class Multiple : ITriggerable
{
    [SerializeReference,ExtendedDrawer] ITriggerable[] executions;

    public void Trigger()
    {
        executions?.TriggerAll();
    }
}
