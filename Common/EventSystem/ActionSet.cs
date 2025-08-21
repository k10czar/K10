using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionSet
{
    private readonly Dictionary<(IEventRegister, Action), ActionCapsule> events = new();

    public void Register(IEventRegister target, Action action)
    {
        var capsule = new ActionCapsule(action);

        events.Add((target, action), capsule);
        target.Register(capsule);
    }

    public void Deregister(IEventRegister target, Action action)
    {
        if (events.TryGetValue((target, action), out var capsule))
        {
            capsule.Void();
            target.Unregister(capsule);
        }
        else Debug.LogError($"Action was not being tracked by ActionSet! {action}");
    }

    public void Void()
    {
        var entries = events.ToList();
        events.Clear();

        foreach (var ((register, _), capsule) in entries)
        {
            capsule.Void();
            register.Unregister(capsule);
        }
    }
}