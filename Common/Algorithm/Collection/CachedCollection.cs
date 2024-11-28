using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICachedCollection<T> : ICachedCollectionObserver<T>, ICachedCollectionInteration<T> { }

public interface ICachedCollectionObserverEnumerable<T> : IEnumerable<T>
{
	IEventRegister OnChange { get; }
    IEventRegister<T> OnElementAdded { get; }
    IEventRegister<T> OnElementRemoved { get; }
}

public interface ICachedCollectionObserver<T> : ICachedCollectionObserverEnumerable<T>, IReadOnlyList<T>
{
    IEventRegister<T> OnNotNullElementRemoved { get; }
	List<TOutput> ConvertAll<TOutput>( System.Converter<T, TOutput> converter );
}

public interface ICachedCollectionInteration<T>
{
    void Add( T t );
    bool Remove( T t );
    void RemoveAt( int i );
    bool Contains(T t);
}

[System.Serializable]
public class CachedCollection<T> : ICachedCollection<T>, ICustomDisposableKill
{
    [SerializeField] protected List<T> _list = new List<T>();

	private EventSlot _onChange;
    private EventSlot<T> _onElementAdded;
    private EventSlot<T> _onElementRemoved;
    private EventSlot<T> _onNotNullElementRemoved;

	public IEventRegister OnChange => _onChange ?? ( _onChange = new EventSlot() );
    public IEventRegister<T> OnElementAdded => _onElementAdded ?? ( _onElementAdded = new EventSlot<T>() );
    public IEventRegister<T> OnElementRemoved => _onElementRemoved ?? ( _onElementRemoved = new EventSlot<T>() );
    public IEventRegister<T> OnNotNullElementRemoved => _onNotNullElementRemoved ?? ( _onNotNullElementRemoved = new EventSlot<T>() );

	public T this[int id]
    {
        get { return _list[id]; }
        
        set
        {
            TriggerRemoveEvents(_list[id]);
            _list[id] = value;
            _onElementAdded?.Trigger(value);
        }
    }

	public virtual int Count { get { return _list.Count; } }

    public void Add( T t )
    {
        _list.Add( t );
        _onElementAdded?.Trigger( t );
		_onChange?.Trigger();
	}

    public int IndexOf( T element ) => _list.IndexOf( element );
    public void Sort( IComparer<T> comparer ) { _list.Sort( comparer ); }
    public void Sort( System.Comparison<T> comparison ) { _list.Sort( comparison ); }

	public void AddRange( IEnumerable<T> range )
	{
		foreach( var t in range ) Add( t );
	}

    public bool Remove( T t )
    {
        var removed = _list.Remove( t );
        if( removed ) TriggerRemoveEvents( t );
        return removed;
    }

    public void RemoveAt( int i )
    {
        var e = _list[i];
        _list.RemoveAt( i );
        TriggerRemoveEvents( e );
    }

    public void Insert(int i, T t)
    {
        _list.Insert( i, t );
        _onElementAdded?.Trigger( t );
        _onChange?.Trigger();
    }

    public bool Contains( T t ){
        return _list.Contains(t);
    }

	public List<TOutput> ConvertAll<TOutput>( System.Converter<T, TOutput> converter ) => _list.ConvertAll<TOutput>( converter );

	public T Find( System.Predicate<T> findCondition ) => _list.Find( findCondition );

    public void Clear(){
        _list.Clear();
		_onChange?.Trigger();
	}

	public void Kill()
	{
		_list.Clear();
		// _list = null;
		_onChange?.Kill();
		_onElementAdded?.Kill();
		_onElementRemoved?.Kill();
		_onNotNullElementRemoved?.Kill();
		_onChange = null;
		_onElementAdded = null;
		_onElementRemoved = null;
		_onNotNullElementRemoved = null;
	}


	void TriggerRemoveEvents( T t )
    {
        _onElementRemoved?.Trigger( t );
        if( t != null ) _onNotNullElementRemoved?.Trigger( t );
		_onChange?.Trigger();
    }

    public override string ToString() { return string.Format( "[CachedCollection<{0}>{{ {1} }}]", typeof(T), string.Join( ", ", _list.ConvertAll( ( t ) => t.ToString() ).ToArray() ) ); }

	public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
}

public sealed class CachedConsistentCollection<T> : CachedCollection<T>
{
    public override int Count { get { CheckConsistency(); return base.Count; } }

    void CheckConsistency() { for( int i = _list.Count - 1; i >= 0; i-- ) { if( _list[i] == null ) _list.RemoveAt( i ); } }
}

public static class CachedCollectionExtentions
{
    static bool SafeNotDefault<T>( T a ) { return ( a != null ) && !a.Equals( default( T ) ); }

    public static void Synchronize<T>( this ICachedCollectionObserver<T> collection, IEventTrigger<T> evnt, bool evenDefault = true )
    { 
        collection.OnElementAdded.Register( evnt );
        
        var count = collection.Count;
        for( int i = 0; i < count; i++ )
        {
            var e = collection[ i ];
            if( evenDefault || SafeNotDefault( e ) ) evnt.Trigger( e ); 
        }
    }

    public static void Synchronize<T>( this ICachedCollectionObserver<T> collection, System.Action<T> evnt, bool evenDefault = true ) 
	{ 
		Synchronize( collection, new ActionEventCapsule<T>( evnt ), evenDefault );
	}

	public static void Synchronize<T>( this ICachedCollectionObserverEnumerable<T> collection, IEventTrigger<T> evnt, bool evenDefault = true )
	{
		collection.OnElementAdded.Register( evnt );

		foreach( var e in collection )
		{
			if( evenDefault || SafeNotDefault( e ) ) evnt.Trigger( e );
		}
	}

	public static void Synchronize<T>( this ICachedCollectionObserverEnumerable<T> collection, System.Action<T> evnt, bool evenDefault = true )
	{
		Synchronize( collection, new ActionEventCapsule<T>( evnt ), evenDefault );
	}

	public static void Synchronize<T>( this ICachedCollectionObserverEnumerable<T> collection, IEventTrigger<T> evnt, IEventValidator validator, bool evenDefault = true )
	{
		collection.Synchronize( validator.Validated<T>( evnt ), evenDefault );
	}

	public static void Synchronize<T>( this ICachedCollectionObserverEnumerable<T> collection, System.Action<T> evnt, IEventValidator validator, bool evenDefault = true )
	{
		collection.Synchronize( validator.Validated<T>( evnt ), evenDefault );
	}
}
