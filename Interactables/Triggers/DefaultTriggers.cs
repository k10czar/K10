using UnityEngine;

public class DefaultTriggers : IInteractorTrigger
{
    [SerializeReference,ExtendedDrawer] ITriggerable[] triggers;

    public void Trigger(IInteractor t)
    {
        if( triggers == null ) return;
        foreach( var trigger in triggers ) trigger?.Trigger();
    }
}