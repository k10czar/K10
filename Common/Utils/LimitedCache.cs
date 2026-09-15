using System;
using System.Collections.Generic;

public class LimitedCache<Key,Value>
{
    private int _sizeTreshold = 100;
    private int _currentSizeTreshold = 100;
    private TimeSpan _timeWarranty = TimeSpan.FromMinutes(1);
    private Dictionary<Key,Value> _cachedData = new();
    private Dictionary<Key,DateTime> _lastUse = new();
    private List<Key> _toRemove = new();

    public LimitedCache( TimeSpan timeWarranty, int sizeTreshold = 100 )
    {
        this._sizeTreshold = sizeTreshold;
        _currentSizeTreshold = sizeTreshold;
        _timeWarranty = timeWarranty;
    }

    private void TryClearCache()
    {
        _toRemove.Clear();
        var now = DateTime.Now;
        foreach( var kvp in _lastUse )
        {
            if( now - kvp.Value < _timeWarranty ) continue;
            _toRemove.Add( kvp.Key );
        }
        foreach( var key in _toRemove )
        {
            _cachedData.Remove( key );
            _lastUse.Remove( key );
        }
        _toRemove.Clear();
        _currentSizeTreshold = _cachedData.Count + _sizeTreshold;
    }

    public bool TryGetValue(Key k, out Value value)
    {
        var found = _cachedData.TryGetValue(k, out value);
        if( found ) _lastUse[k] = DateTime.Now;
        return found;
    }

    public void Add( Key k, Value value )
    {
        _lastUse[k] = DateTime.Now;
        _cachedData[k] = value;
        if( _cachedData.Count > _currentSizeTreshold ) TryClearCache();
    }
}
