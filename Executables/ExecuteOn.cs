using UnityEngine;

public abstract class ExecuteOn<T> : ITriggerable
{
    [SerializeField] T element;
    [SerializeReference,ExtendedDrawer] ITriggerable<T>[] executions;

    public void Trigger()
    {
        executions.TriggerAll( element );
    }
}

public abstract class ExecuteOnArrayOf<T> : ITriggerable
{
    [SerializeField] T[] elements;
    [SerializeReference,ExtendedDrawer] ITriggerable<T>[] executions;

    public void Trigger()
    {
        executions.TriggerAll( elements );
    }
}