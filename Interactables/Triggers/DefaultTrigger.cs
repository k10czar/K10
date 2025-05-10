using UnityEngine;

public class DefaultTrigger : IInteractorTrigger
{
    [SerializeReference,ExtendedDrawer] ITriggerable trigger;

    public void Trigger(IInteractor t)
    {
        if( trigger == null ) return;
        trigger?.Trigger();
    }
}
