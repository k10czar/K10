
using UnityEngine;

/// <summary> Place to aggregate multiple <b>MonoBehaviours</b> in a single object </summary>
public static class ComponentsAggregator
{
    static GameObject _runtime;
    static GameObject _scene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _runtime = null;
        _scene = null;
    }

    public static GameObject Runtime
    {
        get
        {
            if( _runtime == null )
            {
                _runtime = new GameObject("RuntimeComponentAggregator")
                {
                    hideFlags = HideFlags.DontSave
                };
                UnityEngine.Object.DontDestroyOnLoad( _runtime );
            }
            return _runtime;
        }
    }

    public static GameObject Scene
    {
        get
        {
            if( _scene == null )
            {
                _scene = new GameObject("SceneComponentAggregator")
                {
                    hideFlags = HideFlags.DontSave
                };
            }
            return _scene;
        }
    }

    public static T AtRuntime<T>() where T : Component { return AggregatedSingletonComponent<T>.AtRuntime(); }
    public static T AtScene<T>() where T : Component { return AggregatedSingletonComponent<T>.AtScene(); }
}
