using UnityEngine;

[CreateAssetMenu( fileName = "CommonTrigger", menuName = "K10/CommonTrigger", order = 0 )]
public class CommonTriggerDefinition : ScriptableObject, ITriggerable<GameObject>
{
    [SerializeReference,ExtendedDrawer] ITriggerable<GameObject>[] execute;

    public void Trigger( GameObject objectToTrigger )
    {
        execute.TriggerAll( objectToTrigger );
    }
}