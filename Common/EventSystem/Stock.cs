using System.Collections.Generic;

public class CatalogedUniqueStock<Key,Value> where Value : IObjectLifeState
{
    protected readonly Dictionary< Key, Value > _dict = new Dictionary< Key, Value >();

    EventSlot _onEntriesChanged = new EventSlot();
    public IEventRegister OnEntriesChanged => _onEntriesChanged;

    public void AddEntry( Key key, Value t )
    {
        if (_dict.ContainsKey(key)) _dict.Remove(key);
        _dict.Add( key, t );
        t.IsValid.RegisterOnFalse( () => RemoveEntry( key ) );
        _onEntriesChanged.Trigger();
    }

    public bool ContainsKey( Key key ) { return _dict.ContainsKey( key ); }
    public Value GetEntry( Key key ) { return _dict[ key ]; }

    public void RemoveEntry( Key key ) { _dict.Remove( key ); _onEntriesChanged.Trigger(); }
    public bool TryGetValue( Key key, out Value t ) { return _dict.TryGetValue( key, out t ); }
}