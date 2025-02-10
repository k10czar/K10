using System.Collections.Generic;

public class ListService<T> : IService
{
    List<T> _spawns = new List<T>();

    public T this[ int index ] => _spawns[index];
    public int Count => _spawns.Count;

    public void Add( T positionGenerator )
    {
        _spawns.Add( positionGenerator );
    }

    public bool Remove( T positionGenerator )
    {
        return _spawns.Remove( positionGenerator );
    }
}