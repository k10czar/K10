using UnityEngine;

public class SetEnable : ITriggerable<Behaviour>
{
    [SerializeField] bool enabled = true;

    public void Trigger(Behaviour mb)
    {
        mb.enabled = enabled;
    }
}