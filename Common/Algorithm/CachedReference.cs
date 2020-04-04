

public interface ICachedReference<T> : IReferenceHolder<T>, IReferenceSetter<T> { }

public interface IReferenceHolder<T>
{
	T CurrentReference { get; }
	IEventRegister<T> OnReferenceSet { get; }
	IEventRegister<T> OnReferenceRemove { get; }
	IEventValidator Validator{ get; }
	bool IsNull { get; }
}

public interface IReferenceSetter<T>
{
	// T CurrentReference { set; }
	void ChangeReference( T value );
	void Clear();
}

public static class ReferenceHolderExtentions
{
	static bool SafeNotDefault<T>( T a ) { return ( a != null ) && !a.Equals( default(T) ); }

	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action evnt, bool evenDefault ) { Synchronize( referenceHolder, new ActionEventCapsule( evnt ), evenDefault ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action evnt ) { Synchronize( referenceHolder, new ActionEventCapsule( evnt ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger evnt ) { Synchronize( referenceHolder, evnt, true ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger evnt, bool evenDefault )
	{
		referenceHolder.OnReferenceSet.Register( evnt );
		if( evenDefault || SafeNotDefault( referenceHolder.CurrentReference ) ) evnt.Trigger();
	}

	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T> evnt, bool evenDefault ) { Synchronize( referenceHolder, new ActionEventCapsule<T>( evnt ), evenDefault ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T> evnt ) { Synchronize( referenceHolder, new ActionEventCapsule<T>( evnt ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T> evnt ) { Synchronize( referenceHolder, evnt, true ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T> evnt, bool evenDefault ) 
	{ 
		referenceHolder.OnReferenceSet.Register( evnt );
		if( evenDefault || SafeNotDefault( referenceHolder.CurrentReference ) ) 
			evnt.Trigger( referenceHolder.CurrentReference );
	}
}

public class CachedReference<T> : ICachedReference<T>
{
	T _current;
	public T CurrentReference { get { return _current; } set { ChangeReference( value ); } }

	private readonly EventSlot<T> _onReferenceSet = new EventSlot<T>();
	private readonly EventSlot<T> _onReferenceRemove = new EventSlot<T>();

	public IEventRegister<T> OnReferenceSet => _onReferenceSet;
	public IEventRegister<T> OnReferenceRemove => _onReferenceRemove;

	public bool IsNull => _current == null;

	private readonly ConditionalEventsCollection _validator = new ConditionalEventsCollection();
	public IEventValidator Validator => _validator;

	public void Clear() { ChangeReference( default(T) ); }

	public void ChangeReference( T newReference )
	{
		var old = _current;
		if( Algorithm.SafeEquals( old, newReference ) ) return;

		_onReferenceRemove.Trigger( old );
		_validator.Void();
		_current = newReference;
		_onReferenceSet.Trigger( newReference );
	}

	public CachedReference( T startData = default(T) )
	{
		_current = startData;
	}
}