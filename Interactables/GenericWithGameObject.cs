using UnityEngine;

public class GenericWithGameObject : IInteractorTrigger
{
    [SerializeReference,ExtendedDrawer] ITriggerable<GameObject> triggerable;

    public void Trigger( IInteractor interactor )
    {
        triggerable.Trigger( ( interactor != null ) ? interactor.TheInteractor : null );
    }
}
