using System.Collections.Generic;
using System.Linq;

public class CatalogedUniqueStock<Key,Value> : ICustomDisposableKill where Value : IObjectLifeState
{
    protected readonly Dictionary< Key, Value > _dict = new Dictionary< Key, Value >();

    EventSlot _onEntriesChanged = new EventSlot();
    public IEventRegister OnEntriesChanged => _onEntriesChanged;

	public void Kill()
	{
		_dict?.Clear();
		_onEntriesChanged?.Kill();
	}

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

	public override string ToString()
	{
		return $"[ {string.Join( ", ", _dict.ToList().ConvertAll( ( kvp ) => $"({kvp.Key.ToStringOrNull()} => {kvp.Value.ToStringOrNull()})" ) )} ]";
	}
}