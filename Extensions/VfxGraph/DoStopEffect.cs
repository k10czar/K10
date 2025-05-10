using UnityEngine;
using UnityEngine.VFX;

public class DoStopEffect : ITriggerable<VisualEffect>
{
    [SerializeField] private VFXEventAttribute eventAttribute;

    public void Trigger(VisualEffect element)
    {
        element.Stop( eventAttribute );
    }
}