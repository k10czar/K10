using UnityEngine;
using System.Collections.Generic;

public class EternalPrefabInstanciator : MonoBehaviour
{
    [SerializeField] GameObject _prefab;

    static Dictionary<GameObject,GameObject> _instances = new Dictionary<GameObject, GameObject>();

    public static bool ContainsInstance( GameObject prefab ) => GetInstance( prefab ) != null;

    public static GameObject GetInstance( GameObject prefab )
    {
        if( prefab == null ) return null;
        _instances.TryGetValue( prefab, out var inst );
        return inst;
    }

    public void Start()
    {
        if( _prefab == null ) return;
        if( ContainsInstance( _prefab ) ) return;
        
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        var instance = GameObject.Instantiate( _prefab );
        _instances[_prefab] = instance;
        var instantiateDuration = watch.ElapsedMilliseconds;
        GameObject.DontDestroyOnLoad( instance );
        watch.Stop();
        var duration = watch.ElapsedMilliseconds;

#if UNITY_EDITOR
        Debug.Log( $"EternalPrefabInstanciator took <color=cyan>{duration}</color>ms in total to instantiate(<color=cyan>{instantiateDuration}</color>ms) and mark eternal(<color=cyan>{duration-instantiateDuration}</color>ms) {_prefab.NameOrNull()}" );
#else
        Debug.Log( $"EternalPrefabInstanciator took {duration}ms in total to instantiate({instantiateDuration}ms) and mark eternal({duration-instantiateDuration}ms) {_prefab.NameOrNull()}" );
#endif
    }
}
