using UnityEngine;

public class Multiple : ITriggerable
{
    [SerializeReference,ExtendedDrawer] ITriggerable[] executions;

    public void Trigger()
    {
        executions?.TriggerAll();
    }
}

public class MultipleOn<T> : ITriggerable<T>
{
    [SerializeReference,ExtendedDrawer] ITriggerable<T>[] executions;

    public void Trigger( T t )
    {
        executions?.TriggerAll( t );
    }
}

public class MultipleOnTransform : MultipleOn<Transform> { }
public class MultipleOnGameObject : MultipleOn<GameObject> { }
public class MultipleOnComponent : MultipleOn<Component> { }
public class MultipleOnMonoBehaviour : MultipleOn<MonoBehaviour> { }
