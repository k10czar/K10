using System.Collections.Generic;
using UnityEngine;

public interface ICachedCollection<T> : ICachedCollectionObserver<T>, ICachedCollectionInteration<T> { }

public interface ICachedCollectionObserverEnumerable<T> : IEnumerable<T>
{  
    IEventRegister<T> OnElementAdded { get; }
    IEventRegister<T> OnElementRemoved { get; }
}

public interface ICachedCollectionObserver<T>
{
    int Count { get; }
    T this[int id] { get; }
    IEventRegister<T> OnElementAdded { get; }
    IEventRegister<T> OnElementRemoved { get; }
    IEventRegister<T> OnNotNullElementRemoved { get; }
}

public interface ICachedCollectionInteration<T>
{
    void Add( T t );
    bool Remove( T t );
    void RemoveAt( int i );
    bool Contains(T t);
}

[System.Serializable]
public class CachedCollection<T> : ICachedCollection<T>
{
    [SerializeField] protected List<T> _list = new List<T>();

    private readonly EventSlot<T> _onElementAdded = new EventSlot<T>();
    private readonly EventSlot<T> _onElementRemoved = new EventSlot<T>();
    private readonly EventSlot<T> _onNotNullElementRemoved = new EventSlot<T>();

    public IEventRegister<T> OnElementAdded { get { return _onElementAdded; } }
    public IEventRegister<T> OnElementRemoved { get { return _onElementRemoved; } }
    public IEventRegister<T> OnNotNullElementRemoved { get { return _onNotNullElementRemoved; } }


    public T this[int id]
    {
        get { return _list[id]; }
        
        set
        {
            TriggerRemoveEvents(_list[id]);
            _list[id] = value;
            _onElementAdded.Trigger(value);
        }
    }

	public virtual int Count { get { return _list.Count; } }

    public void Add( T t )
    {
        _list.Add( t );
        _onElementAdded.Trigger( t );
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

    public bool Contains( T t ){
        return _list.Contains(t);
    }

	public T Find( System.Predicate<T> findCondition ) => _list.Find( findCondition );

    public void Clear(){
        _list.Clear();
    }

    void TriggerRemoveEvents( T t )
    {
        _onElementRemoved.Trigger( t );
        if( t != null ) _onNotNullElementRemoved.Trigger( t );
    }

    public override string ToString() { return string.Format( "[CachedCollection<{0}>[{1}]]", typeof(T), string.Join( ", ", _list.ConvertAll( ( t ) => t.ToString() ).ToArray() ) ); }
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
}
