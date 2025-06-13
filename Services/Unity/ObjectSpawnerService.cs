using UnityEngine;

public interface ISpawnerService<T> : IService where T : MonoBehaviour
{
    T RequestInstance(Vector3? position = null, Quaternion? rotation = null, Transform parent = null);
}

public abstract class ObjectSpawnerService<T> : ISpawnerService<T> where T : MonoBehaviour
{
    [SerializeField] T reference;
	
	EventSlot<T> _onObjectSpawned = null;
    public IEventRegister<T> OnObjectSpawned  => _onObjectSpawned ??= new EventSlot<T>();

    public virtual T RequestInstance( Vector3? position = null, Quaternion? rotation = null, Transform parent = null )
    {
        T instance = default;
        if( position.HasValue && rotation.HasValue ) instance = GameObject.Instantiate( reference, position.Value, rotation.Value, parent );
        else if( position.HasValue ) instance = GameObject.Instantiate( reference, position.Value, Quaternion.identity, parent );
        else if( rotation.HasValue ) instance = GameObject.Instantiate( reference, Vector3.zero, rotation.Value, parent );
        else instance = GameObject.Instantiate( reference, parent );
        ConfigureInstance( instance );
        _onObjectSpawned?.Trigger( instance );
        return instance;
    }

    public virtual void ConfigureInstance( T t ) { }
}
