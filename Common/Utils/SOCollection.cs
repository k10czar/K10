using UnityEngine;
using System.Collections.Generic;

public class SOCollection<T> : ScriptableObject where T : ScriptableObject
{
    [SerializeField] List<T> _objects = new List<T>();

    public int Count { get { return _objects.Count; } }
    public T this[int id] { get { return _objects[id]; } }
    public int IndexOf( T element ) { return _objects.IndexOf( element ); }

    public IEnumerable<T> Objects { get { return _objects; } }
    
    public void RequestMember( T obj )
    {
        if( obj == null || _objects.Contains( obj ) ) return;
        _objects.Add( obj );
    }
    
    public bool Contains( T obj )
    {
        if( obj == null ) return false;
        return _objects.Contains( obj );
    }

    public T GetByName(string searchQuery)
    {
        for (int i = 0; i < _objects.Count; i++)
        {
            if (searchQuery.ToLower().Equals(_objects[i].name.ToLower()))
                return _objects[i];
        }
        Debug.LogError($"Could not find item named '{searchQuery}' on {this.name}.");
        return null;
    }
}