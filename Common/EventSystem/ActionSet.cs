using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionSet
{
    private readonly Dictionary<Action, (IEventRegister, ActionCapsule)> events = new();

    public void Register(IEventRegister target, Action action)
    {
        var capsule = new ActionCapsule(action);

        events.Add(action, (target, capsule));
        target.Register(capsule);
    }

    public void Deregister(IEventRegister target, Action action)
    {
        if (events.TryGetValue(action, out var entry))
        {
            var (register, capsule) = entry;
            Debug.Assert(register == target, $"Found different event register! {register} != {target}");

            capsule.Void();
            register.Unregister(capsule);
        }
        else Debug.LogError($"Action was not being tracked by ActionSet! {action}");
    }

    public void Void()
    {
        var values = events.Values.ToList();
        events.Clear();

        foreach (var (register, capsule) in values)
        {
            capsule.Void();
            register.Unregister(capsule);
        }
    }
}