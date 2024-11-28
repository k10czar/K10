using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;

public class DoPlayEffect : ITriggerable<VisualEffect>
{
    [SerializeField] private VFXEventAttribute eventAttribute;

    public void Trigger(VisualEffect element)
    {
        element.Play( eventAttribute );
    }
}
