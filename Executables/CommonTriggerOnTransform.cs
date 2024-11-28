using UnityEngine;

public class CommonTriggerOnTransform : ITriggerable<Transform>
{
    [SerializeField,InlineProperties] CommonTriggerDefinition commonTrigger;

    public void Trigger( Transform transform )
    {
        if( transform == null ) return;
        if( commonTrigger == null ) return;
        commonTrigger.Trigger( transform.gameObject );
    }
}
