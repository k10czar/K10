using System.Collections;
using System.Collections.Generic;


public interface ICachedDictionaryObserver<K, T> : ICachedCollectionObserverEnumerable<T>
{
	bool GetFirst( K key, out T outValue );
	IBoolStateObserver GetEventDrivenContains( K key );
}

public class CachedDictionary<K,T> : ICachedDictionaryObserver<K,T>
{
    private readonly Dictionary<K, List<T>> _dictionary = new Dictionary<K, List<T>>();
	private readonly Dictionary<K, BoolState> _eventDrivenContains = new Dictionary<K, BoolState>();
	private readonly Dictionary<K, IntState> _eventDrivenCount = new Dictionary<K, IntState>();

    private readonly EventSlot<T> _onElementAdded = new EventSlot<T>();
    private readonly EventSlot<T> _onElementRemoved = new EventSlot<T>();
    //private readonly EventSlot<T> _onNotNullElementRemoved = new EventSlot<T>();

    public IEventRegister<T> OnElementAdded => _onElementAdded;
    public IEventRegister<T> OnElementRemoved => _onElementRemoved;
    //public IEventRegister<T> OnNotNullElementRemoved => _onNotNullElementRemoved;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public bool ContainsKey( K key ) => _dictionary.ContainsKey( key );

    public int KeyCount => _dictionary.Count;

    public IEnumerator<T> GetEnumerator()
    {        
        foreach ( var list in _dictionary )
        {
            foreach( var value in list.Value )
            {
                yield return value;
            }
        }
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

        if( deleted )
        {
            _onElementRemoved.Trigger( value );

			var count = list.Count;
			GetEventDrivenCountEditor( key ).Setter( count );
			if( list.Count == 0 )
			{
				_dictionary.Remove( key );
				GetEventDrivenContainsEditor( key ).SetFalse();
			}
			//_onNotNullElementRemoved.Trigger(value);
		}
    }

    public bool Remove( K key )
    {
        List<T> list;
        if( !_dictionary.TryGetValue( key, out list ) ) return false;

		for( int i = list.Count - 1; i >= 0; i-- )
		{
			var value = list[i];
        	_onElementRemoved.Trigger( value );
            // if( value != null )_ onNotNullElementRemoved.Trigger( value );
        }

		list.Clear();
		_dictionary.Remove( key );
		GetEventDrivenContainsEditor( key ).SetFalse();
		GetEventDrivenCountEditor( key ).Setter( 0 );

		return true;
	}
}
