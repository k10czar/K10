using UnityEngine;
using System.Collections;

public class AttachedComponent<T> : MonoBehaviour where T : Component
{
    private T m_attachedWidget;
    
    protected T Attached
    {
        get
        { 
            if( m_attachedWidget == null )
                m_attachedWidget = GetComponent< T >();
            
            return m_attachedWidget; 
        } 
    }
}

public class CachedComponent<T> where T : Component
{
    private GameObject m_gameObject;
    private T m_cache;
    
    public T Component
    {
        get
        { 
            if( m_cache == null )
                m_cache = m_gameObject.GetComponent<T>();

            return m_cache; 
        }
    }

    public CachedComponent( GameObject go )
    {
        m_gameObject = go;
    }
}

public class CachedChildComponent<T> where T : Component
{
    private GameObject m_gameObject;
    private T m_cache;

    public T Component
    {
        get
        {
            if( m_cache == null )
                m_cache = m_gameObject.GetComponentInChildren<T>();

            return m_cache;
        }
    }

    public CachedChildComponent( GameObject go )
    {
        m_gameObject = go;
    }
}