using UnityEngine;

public class MultipleInteractions : IInteractorTrigger
{
    [SerializeReference,ExtendedDrawer] IInteractorTrigger[] triggers;

    public void Trigger(IInteractor t)
    {
        if( triggers == null ) return;
        foreach( var trigger in triggers ) trigger?.Trigger(t);
    }
}
