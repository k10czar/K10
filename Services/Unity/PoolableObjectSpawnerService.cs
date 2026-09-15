using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public interface IPoolable
{
    void WakeUp();
    void Sleep();
}

public abstract class PoolableObjectSpawnerService<T> : ObjectSpawnerService<T> where T : MonoBehaviour, IPoolable
{
    [SerializeField] Transform _poolEditorParent;
    [SerializeField] Transform _poolRuntimeParent;

    List<T> _pool = new();

    EventSlot<T> _onObjectRequestFulfill = null;
    EventSlot<T> _onObjectReturned = null;

    public IEventRegister<T> OnObjectRequestFulfill => _onObjectRequestFulfill ??= new EventSlot<T>();
    public IEventRegister<T> OnObjectReturned => _onObjectReturned ??= new EventSlot<T>();

    public Transform PoolParent
    {
        [MethodImpl(Optimizations.INLINE_IF_CAN)]
        get
        {
#if UNITY_EDITOR
            return _poolEditorParent;
#else
            return _poolRuntimeParent;
#endif
        }
    }    

    public override T RequestInstance(Vector3? position = null, Quaternion? rotation = null, Transform parent = null)
    {
        var count = _pool.Count;
        if (_pool.Count == 0)
        {
            T instance = base.RequestInstance( position, rotation, parent );
            _onObjectRequestFulfill?.Trigger( instance );
            return instance;
        }
        var lastId = count - 1;
        var lastElement = _pool[lastId];
        _pool.RemoveAt(lastId);
        var eTrans = lastElement.transform;

        eTrans.parent = parent;
        if( position.HasValue && rotation.HasValue ) eTrans.SetLocalPositionAndRotation( position.Value, rotation.Value );
        else if( position.HasValue ) eTrans.position = position.Value;
        else if( rotation.HasValue ) eTrans.rotation = rotation.Value;
        
        _onObjectRequestFulfill?.Trigger( lastElement );
        return lastElement;
    }

    public void Return(T t)
    {
        if (t == null) return;
        t.Sleep();
        t.transform.parent = PoolParent;
        _pool.Add(t);
        _onObjectReturned?.Trigger( t );
    }

    public void ReturnAndClearRef(ref T t)
    {
        Return(t);
        t = null;
    }
}
