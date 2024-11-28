using UnityEngine;

public class HoldingObject : IValidator<IInteractor>
{
    [SerializeReference,ExtendedDrawer] IReferenceHolderRaw<IInteractionObject> holdObject;
    public bool Validate( IInteractor interactor ) => holdObject.CurrentReference.Equals( interactor.Object.CurrentReference );
}
