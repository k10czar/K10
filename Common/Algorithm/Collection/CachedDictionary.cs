using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;


public interface ICachedDictionaryObserver<K, T> : ICachedCollectionObserverEnumerable<T>
{
	bool GetFirst( K key, out T outValue );
	bool TryGetValues( K key, out ReadOnlyCollection<T> outValue );
	IBoolStateObserver GetEventDrivenContains( K key );
}

public class CachedDictionary<K,T> : ICachedDictionaryObserver<K,T>, ICustomDisposableKill
{
    private readonly Dictionary<K, List<T>> _dictionary = new Dictionary<K, List<T>>();
	private readonly Dictionary<K, BoolState> _eventDrivenContains = new Dictionary<K, BoolState>();
	private readonly Dictionary<K, IntState> _eventDrivenCount = new Dictionary<K, IntState>();

	private readonly EventSlot _onChange = new EventSlot();
    private readonly EventSlot<T> _onElementAdded = new EventSlot<T>();
    private readonly EventSlot<T> _onElementRemoved = new EventSlot<T>();

	public IEventRegister OnChange => _onChange;
    public IEventRegister<T> OnElementAdded => _onElementAdded;
    public IEventRegister<T> OnElementRemoved => _onElementRemoved;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public bool ContainsKey( K key ) => _dictionary.ContainsKey( key );

    public int KeyCount => _dictionary.Count;


	public void Kill()
	{
		// Clear();
		_dictionary?.Clear();
		_eventDrivenContains?.Clear();
		_eventDrivenCount?.Clear();
		_onChange?.Kill();
		_onElementAdded?.Kill();
		_onElementRemoved?.Kill();
	}


	public IEnumerator<T> GetEnumerator()
    {        
        foreach ( var list in _dictionary )
        {
			var listValue = list.Value;
			foreach( var value in listValue )
            {
                yield return value;
            }
        }
       
	}

    public List<K> GetKeys()
    {
        List<K> keys = new List<K>();

        foreach (var item in _dictionary)
        {
            keys.Add(item.Key);
        }

        return keys;
    }

	public bool TryGetValues( K key, out ReadOnlyCollection<T> outValue )
	{
		var exists = _dictionary.TryGetValue( key, out var list );
		if( exists ) outValue = list.AsReadOnly();
		else outValue = default(ReadOnlyCollection<T>);
		return exists;
	}
  
    public bool GetFirst( K key, out T outValue )
	{
		List<T> list;
        if( !_dictionary.TryGetValue( key, out list ) || list.Count == 0 )
        {
            outValue = default(T);
            return false;
        }
        
        outValue = list[0];
        return true;
    }

	public int Count( K key )
	{
		List<T> list;
		if( _dictionary.TryGetValue( key, out list ) ) return list.Count;
		return 0;
	}

    public void Add( K key, T value )
    {
        List<T> list;
        if( !_dictionary.TryGetValue( key, out list ) )
        {
            list = new List<T>();
            _dictionary.Add( key, list );
			GetEventDrivenContainsEditor( key ).SetTrue();
        }

        list.Add( value );
		GetEventDrivenCountEditor( key ).Setter( list.Count );
		_onElementAdded.Trigger( value );
		_onChange.Trigger();
    }

	public IBoolStateObserver GetEventDrivenContains( K key ) => GetEventDrivenContainsEditor( key );

	private BoolState GetEventDrivenContainsEditor( K key )
	{
		BoolState contains;
		if( !_eventDrivenContains.TryGetValue( key, out contains ) )
		{
			contains = new BoolState();
			_eventDrivenContains[key] = contains;
		}
		return contains;
	}

	public IValueStateObserver<int> GetEventDrivenCount( K key ) => GetEventDrivenCountEditor( key );

	private IntState GetEventDrivenCountEditor( K key )
	{
		IntState contains;
		if( !_eventDrivenCount.TryGetValue( key, out contains ) )
		{
			contains = new IntState( 0 );
			_eventDrivenCount[key] = contains;
		}
		return contains;
	}

    public void Remove( K key, T value )
    {
        List<T> list;
        if( !_dictionary.TryGetValue( key, out list ) ) return;

        var deleted = list.Remove( value );

        if (!deleted) return;

        var count = list.Count;
        GetEventDrivenCountEditor( key ).Setter( count );
        if( list.Count == 0 )
        {
            _dictionary.Remove( key );
            GetEventDrivenContainsEditor( key ).SetFalse();
        }

        _onElementRemoved.Trigger( value );
		_onChange.Trigger();
		// if( value != null )_ onNotNullElementRemoved.Trigger( value );
	}

    public bool Remove( K key )
    {
        if( !_dictionary.TryGetValue( key, out var list ) ) return false;

        _dictionary.Remove( key );

        for( int i = list.Count - 1; i >= 0; i-- )
        {
            var value = list[i];
            _onElementRemoved.Trigger( value );
            // if( value != null )_ onNotNullElementRemoved.Trigger( value );
        }

        list.Clear();

        GetEventDrivenContainsEditor( key ).SetFalse();
        GetEventDrivenCountEditor( key ).Setter( 0 );

		_onChange.Trigger();

        return true;
    }
}
