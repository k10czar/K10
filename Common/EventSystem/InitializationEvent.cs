using System;
using UnityEngine;

public class InitializationEvent : ICustomDisposableKill
{
    private readonly EventSlot initialized = new();
    private bool isInitialized;

    public void SetReady()
    {
        Debug.Assert(!isInitialized, "Already initialized!");

        isInitialized = true;
        initialized.Trigger();
        initialized.Kill();
    }

    public void CallWhenReady(Action callback)
    {
        if (isInitialized) callback.Invoke();
        else initialized.Register(callback);
    }
    
    public static implicit operator bool (InitializationEvent v) => v.isInitialized;

    public void Kill()
    {
        initialized.Kill();
    }
}