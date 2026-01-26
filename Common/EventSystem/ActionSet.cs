using System;
using System.Collections.Generic;
using System.Linq;
using K10;
using UnityEngine;

public class ActionSet
{
    private readonly List<IVoidable> registeredActions = ObjectPool<List<IVoidable>>.Request();

    #region Register Interface

    public IVoidable Register(IEventRegister target, Action action)
    {
        var capsule = new ActionCapsule(action, target);
        registeredActions.Add(capsule);
        return capsule;
    }

    public IVoidable Register<T>(IEventRegister<T> target, Action<T> action)
    {
        var capsule = new ActionCapsule<T>(action, target);
        registeredActions.Add(capsule);
        return capsule;
    }

    public IVoidable Register<T, K>(IEventRegister<T, K> target, Action<T, K> action)
    {
        var capsule = new ActionCapsule<T, K>(action, target);
        registeredActions.Add(capsule);
        return capsule;
    }

    public IVoidable Register<T, K, L>(IEventRegister<T, K, L> target, Action<T, K, L> action)
    {
        var capsule = new ActionCapsule<T, K, L>(action, target);
        registeredActions.Add(capsule);
        return capsule;
    }

    #endregion

    #region Void Interface

    public void Void(IVoidable voidable)
    {
        var removed = registeredActions.Remove(voidable);
        Debug.Assert(removed, "Trying to void non tracked IVoidable!");
        voidable.Void();
    }

    public void Void()
    {
        var entries = registeredActions.ToList();
        registeredActions.Clear();

        foreach (var capsule in entries)
            capsule.Void();
    }

    #endregion

    ~ActionSet() => ObjectPool<List<IVoidable>>.Return(registeredActions);
}