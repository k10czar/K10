using UnityEngine;

public class IgnoreLastParameter<T> : ITriggerable<T>
{
    [SerializeReference,ExtendedDrawer] ITriggerable triggerable;

    public void Trigger( T interactor )
    {
        triggerable?.Trigger();
    }
}
