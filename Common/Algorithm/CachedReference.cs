

public interface ICachedReference<T> : IReferenceHolder<T>, IReferenceSetter<T> { } //where T : class

public interface IReferenceHolder<T>
{
	T CurrentReference { get; }
	IEventRegister<T,IEventValidator> OnReferenceSet { get; }
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
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action evnt, System.Func<bool> validation ) { Synchronize( referenceHolder, new ConditionalEventListener( evnt, () => validation() ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action evnt, params IEventValidator[] validators ) { Synchronize( referenceHolder, new ActionEventCapsule( evnt ), validators ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger evnt ) { Synchronize( referenceHolder, evnt, true ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger evnt, System.Func<bool> validation ) { Synchronize( referenceHolder, new ConditionalEventListener( evnt, () => validation() && evnt.IsValid ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger evnt, params IEventValidator[] validators )
	{
		var vals = validators.GetCurrentValidators();
		Synchronize( referenceHolder, new ConditionalEventListener( evnt, () => vals.And() && evnt.IsValid ) );
	}
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger evnt, bool evenDefault )
	{
		referenceHolder.OnReferenceSet.Register( evnt );
		if( evenDefault || SafeNotDefault( referenceHolder.CurrentReference ) ) evnt.Trigger();
	}

	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T> evnt, bool evenDefault ) { Synchronize( referenceHolder, new ActionEventCapsule<T>( evnt ), evenDefault ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T> evnt ) { Synchronize( referenceHolder, new ActionEventCapsule<T>( evnt ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T> evnt, System.Func<bool> validation ) { Synchronize<T>( referenceHolder, new ConditionalEventListener<T>( evnt, () => validation() ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T> evnt, params IEventValidator[] validators ) { Synchronize( referenceHolder, new ActionEventCapsule<T>( evnt ), validators ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T> evnt ) { Synchronize( referenceHolder, evnt, true ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T> evnt, System.Func<bool> validation ) { Synchronize<T>( referenceHolder, new ConditionalEventListener<T>( evnt, () => validation() && evnt.IsValid ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T> evnt, params IEventValidator[] validators )
	{
		var vals = validators.GetCurrentValidators();
		Synchronize( referenceHolder, new ConditionalEventListener<T>( evnt, () => vals.And() && evnt.IsValid ) );
	}
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T> evnt, bool evenDefault )
	{
		referenceHolder.OnReferenceSet.Register( evnt );
		if( evenDefault || SafeNotDefault( referenceHolder.CurrentReference ) )
			evnt.Trigger( referenceHolder.CurrentReference );
	}


	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T,IEventValidator> evnt, bool evenDefault ) { Synchronize( referenceHolder, new ActionEventCapsule<T,IEventValidator>( evnt ), evenDefault ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T,IEventValidator> evnt ) { Synchronize( referenceHolder, new ActionEventCapsule<T,IEventValidator>( evnt ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T,IEventValidator> evnt, System.Func<bool> validation ) { Synchronize<T>( referenceHolder, new ConditionalEventListener<T,IEventValidator>( evnt, () => validation() ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T,IEventValidator> evnt, params IEventValidator[] validators ) { Synchronize( referenceHolder, new ActionEventCapsule<T,IEventValidator>( evnt ), validators ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T,IEventValidator> evnt ) { Synchronize( referenceHolder, evnt, true ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T,IEventValidator> evnt, System.Func<bool> validation ) { Synchronize<T>( referenceHolder, new ConditionalEventListener<T,IEventValidator>( evnt, () => validation() && evnt.IsValid ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T, IEventValidator> evnt, params IEventValidator[] validators )
	{
		var vals = validators.GetCurrentValidators();
		Synchronize( referenceHolder, new ConditionalEventListener<T,IEventValidator>( evnt, () => vals.And() && evnt.IsValid ) );
	}
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T, IEventValidator> evnt, bool evenDefault )
	{
		referenceHolder.OnReferenceSet.Register( evnt );
		if( evenDefault || SafeNotDefault( referenceHolder.CurrentReference ) )
			evnt.Trigger( referenceHolder.CurrentReference, referenceHolder.Validator );
	}
}

public class CachedReference<T> : ICachedReference<T>, ICustomDisposableKill
{
	T _current;
	public T CurrentReference { get { return _current; } set { ChangeReference( value ); } }

	private EventSlot<T, IEventValidator> _onReferenceSet;
	private EventSlot<T> _onReferenceRemove = new EventSlot<T>();

	public IEventRegister<T, IEventValidator> OnReferenceSet => _onReferenceSet ?? ( _onReferenceSet = new EventSlot<T, IEventValidator>() );
	public IEventRegister<T> OnReferenceRemove => _onReferenceRemove ?? ( _onReferenceRemove = new EventSlot<T>() );

	public bool IsNull => _current == null;

	private ConditionalEventsCollection _validator;
	public IEventValidator Validator => _validator ?? ( _validator = new ConditionalEventsCollection() );

	public void Clear() { ChangeReference( default(T) ); }

	public void Kill()
	{
		_onReferenceSet?.Kill();
		_onReferenceRemove?.Kill();
		_validator?.Void();
		_onReferenceSet = null;
		_onReferenceRemove = null;
		_validator = null;
	}

	public void ChangeReference( T newReference )
	{
		var old = _current;
		if( Algorithm.SafeEquals( old, newReference ) ) return;

		_onReferenceRemove?.Trigger( old );
		_validator?.Void();
		_current = newReference;
		if( _onReferenceSet != null ) _onReferenceSet.Trigger( newReference, Validator );
	}

	public CachedReference( T startData = default(T) )
	{
		_current = startData;
	}
}