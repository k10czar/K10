using System;
using System.Collections.Generic;
using System.Linq;
using K10;

public class ActionSet
{
    private readonly List<IVoidable> registeredActions = ObjectPool<List<IVoidable>>.Request();

    #region Register Interface

    public void Register(IEventRegister target, Action action)
    {
        var capsule = new ActionCapsule(action, target);
        registeredActions.Add(capsule);
    }

    public void Register<T>(IEventRegister<T> target, Action<T> action)
    {
        var capsule = new ActionCapsule<T>(action, target);
        registeredActions.Add(capsule);
    }

    public void Register<T, K>(IEventRegister<T, K> target, Action<T, K> action)
    {
        var capsule = new ActionCapsule<T, K>(action, target);
        registeredActions.Add(capsule);
    }

    public void Register<T, K, L>(IEventRegister<T, K, L> target, Action<T, K, L> action)
    {
        var capsule = new ActionCapsule<T, K, L>(action, target);
        registeredActions.Add(capsule);
    }

    #endregion

    public void Void()
    {
        var entries = registeredActions.ToList();
        registeredActions.Clear();

        foreach (var capsule in entries)
            capsule.Void();
    }

    ~ActionSet() => ObjectPool<List<IVoidable>>.Return(registeredActions);
}