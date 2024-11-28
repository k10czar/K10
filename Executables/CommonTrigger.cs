using UnityEngine;

public class CommonTrigger : ITriggerable
{
    [SerializeField] GameObject objectToTrigger;
    [SerializeField,InlineProperties] CommonTriggerDefinition commonTrigger;

    public void Trigger()
    {
        if( commonTrigger == null ) return;
        commonTrigger.Trigger( objectToTrigger );
    }
}