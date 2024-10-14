using System;

[Serializable]
public class UnityEventTrigger : ITriggerable
{
    [UnityEngine.SerializeField] public UnityEngine.Events.UnityEvent action;

    public void Trigger()
    {
        if( action == null ) return;
        action.Invoke();
    }
}
