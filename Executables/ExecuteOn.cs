using System;
using System.Linq;
using UnityEngine;

public abstract class ExecuteOn<T> : ITriggerable, ISummarizable
{
    [SerializeField] T element;
    [SerializeReference,ExtendedDrawer] ITriggerable<T>[] executions;

    public void Trigger()
    {
        executions.TriggerAll( element );
    }

    public string Summarize() => $"{executions.TrySummarize( " and " )} on {element.TrySummarize()}";
}

public abstract class ExecuteOnArrayOf<T> : ITriggerable
{
    [SerializeField] T[] elements;
    [SerializeReference,ExtendedDrawer] ITriggerable<T>[] executions;

    public void Trigger()
    {
        executions.TriggerAll( elements );
    }

    public string Summarize() => $"{executions.TrySummarize( " and " )} on {elements.TrySummarize( " and " )}";
}