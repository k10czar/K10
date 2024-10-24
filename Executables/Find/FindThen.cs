using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FindThen<T> : ITriggerable<GameObject> where T : Component
{
    [SerializeReference,ExtendedDrawer] ITriggerable<T>[] execute;

    public void Trigger(GameObject obj)
    {
        if (obj == null) return;
        var component = obj.GetComponent<T>();
        if (component == null) return;
        execute.TriggerAll(component);
    }
}
