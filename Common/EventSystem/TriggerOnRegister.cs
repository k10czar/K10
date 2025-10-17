public struct TriggerOnRegister : IEventRegister
{
    public void Register(IEventTrigger listener) => listener.Trigger();
    public bool Unregister(IEventTrigger listener) => true;
}

public struct TriggerOnRegister<T> : IEventRegister<T>, IEventRegister
{
    T _valueToTrigger;

    public TriggerOnRegister( T valueToTrigger )
    {
        _valueToTrigger = valueToTrigger;
    }

    public void Register(IEventTrigger listener) => listener.Trigger();
    public void Register(IEventTrigger<T> listener) => listener.Trigger(_valueToTrigger);

    public bool Unregister(IEventTrigger listener) => true;
    public bool Unregister(IEventTrigger<T> listener) => true;
}

public struct TriggerOnRegister<T,K> : IEventRegister<T,K>, IEventRegister<T>, IEventRegister
{
    T _t;
    K _k;

    public TriggerOnRegister( T tToTrigger, K kToTrigger )
    {
        _t = tToTrigger;
        _k = kToTrigger;
    }

    public void Register(IEventTrigger listener) => listener.Trigger();
    public void Register(IEventTrigger<T> listener) => listener.Trigger(_t);
    public void Register(IEventTrigger<T, K> listener) => listener.Trigger( _t, _k );

    public bool Unregister(IEventTrigger listener) => true;
    public bool Unregister(IEventTrigger<T> listener) => true;
    public bool Unregister(IEventTrigger<T, K> listener) => true;
}