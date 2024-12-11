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

    public void CallWhenReady(InitializationEvent otherInitialization, Action callback)
    {
        if (isInitialized) otherInitialization.CallWhenReady(callback);
        else initialized.Register(() => otherInitialization.CallWhenReady(callback));
    }

    public static implicit operator bool (InitializationEvent v) => v.isInitialized;

    public InitializationEvent(bool startReady = false)
    {
        if (startReady) SetReady();
    }

    public void Kill() => initialized.Kill();

    public void Clear()
    {
        isInitialized = false;
        initialized.Clear();
    }
}

public class InitializationEvent<T> : ICustomDisposableKill
{
    private readonly T source;

    private readonly EventSlot<T> initialized = new();
    private bool isInitialized;

    public void SetReady()
    {
        Debug.Assert(!isInitialized, "Already initialized!");

        isInitialized = true;
        initialized.Trigger(source);
        initialized.Kill();
    }

    public void CallWhenReady(Action callback)
    {
        if (isInitialized) callback.Invoke();
        else initialized.Register(callback);
    }

    public void CallWhenReady(Action<T> callback)
    {
        if (isInitialized) callback.Invoke(source);
        else initialized.Register(callback);
    }

    public void CallWhenReady<U>(InitializationEvent<U> otherInitialization, Action callback)
    {
        if (isInitialized) otherInitialization.CallWhenReady(callback);
        else initialized.Register(() => otherInitialization.CallWhenReady(callback));
    }

    public static implicit operator bool (InitializationEvent<T> v) => v.isInitialized;

    public InitializationEvent(T source, bool startReady = false)
    {
        this.source = source;
        if (startReady) SetReady();
    }

    public void Kill() => initialized.Kill();

    public void Clear()
    {
        isInitialized = false;
        initialized.Clear();
    }
}