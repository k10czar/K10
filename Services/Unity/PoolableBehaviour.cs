using UnityEngine;

public abstract class PoolableBehaviour : MonoBehaviour, IPoolable
{
    public virtual bool ControlGameObjectState => true;
    public virtual bool ControlComponentState => false;

    public virtual void Sleep()
    {
        if( ControlGameObjectState ) gameObject.SetActive(false);
        if( ControlComponentState ) this.enabled = false;
    }

    public virtual void WakeUp()
    {
        if( ControlGameObjectState ) gameObject.SetActive(true);
        if( ControlComponentState ) this.enabled = true;
    }
}
