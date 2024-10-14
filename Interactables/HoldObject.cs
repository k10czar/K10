using UnityEngine;

public class HoldObject : IInteractorTrigger
{
    [SerializeReference,ExtendedDrawer] IReferenceHolderRaw<IInteractionObject> interactableObjectReference;

    public void Trigger( IInteractor interactor )
    {
        interactor.Hold( interactableObjectReference.CurrentReference );
    }
}