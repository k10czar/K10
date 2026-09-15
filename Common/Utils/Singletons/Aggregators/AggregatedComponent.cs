using UnityEngine;

/// <summary> Component that is placed in ComponentsAggregators </summary>
public static class AggregatedSingletonComponent<T> where T : Component
{
    static T _instance;

    // Generic classes can't use RuntimeInitializeOnLoadMethod directly.
    // Unity's fake-null check handles destroyed instances correctly between play sessions,
    // so the null check in AtRuntime/AtScene is sufficient for no-domain-reload compatibility.

    public static T AtRuntime()
    {
        if( _instance == null )
        {
            _instance = ComponentsAggregator.Runtime.AddComponent<T>();
            Singleton.SayHello( _instance );
        }
        return _instance;
    }

    public static T AtScene()
    {
        if( _instance == null )
        {
            _instance = ComponentsAggregator.Scene.AddComponent<T>();
            Singleton.SayHello( _instance );
        }
        return _instance;
    }
}
