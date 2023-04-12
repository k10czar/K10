

using System;

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
	// public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action evnt, System.Func<bool> validation ) { Synchronize( referenceHolder, new ConditionalEventListener( evnt, () => validation() ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action evnt, IEventValidator validator ) { Synchronize( referenceHolder, validator.Validated( evnt ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger evnt ) { Synchronize( referenceHolder, evnt, true ); }
	// public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger evnt, System.Func<bool> validation ) { Synchronize( referenceHolder, new ConditionalEventListener( evnt, () => validation() && evnt.IsValid ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger evnt, IEventValidator validator ) { Synchronize( referenceHolder, validator.Validated( evnt ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger evnt, bool evenDefault )
	{
		referenceHolder.OnReferenceSet.Register( evnt );
		if( evenDefault || SafeNotDefault( referenceHolder.CurrentReference ) ) evnt.Trigger();
	}

	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T> evnt, bool evenDefault ) { Synchronize( referenceHolder, new ActionEventCapsule<T>( evnt ), evenDefault ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T> evnt ) { Synchronize( referenceHolder, new ActionEventCapsule<T>( evnt ) ); }
	// public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T> evnt, System.Func<bool> validation ) { Synchronize<T>( referenceHolder, new ConditionalEventListener<T>( evnt, () => validation() ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T> evnt, IEventValidator validator ) { Synchronize( referenceHolder, validator.Validated<T>( evnt ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T> evnt ) { Synchronize( referenceHolder, evnt, true ); }
	// public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T> evnt, System.Func<bool> validation ) { Synchronize<T>( referenceHolder, new ConditionalEventListener<T>( evnt, () => validation() && evnt.IsValid ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T> evnt, IEventValidator validator )  { Synchronize( referenceHolder, validator.Validated<T>( evnt ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T> evnt, bool evenDefault = true )
	{
		referenceHolder.OnReferenceSet.Register( evnt );
		if( evenDefault || SafeNotDefault( referenceHolder.CurrentReference ) )
			evnt.Trigger( referenceHolder.CurrentReference );
	}

	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T,IEventValidator> evnt, bool evenDefault ) { Synchronize( referenceHolder, new ActionEventCapsule<T,IEventValidator>( evnt ), evenDefault ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T,IEventValidator> evnt ) { Synchronize( referenceHolder, new ActionEventCapsule<T,IEventValidator>( evnt ) ); }
	// public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T,IEventValidator> evnt, System.Func<bool> validation ) { Synchronize<T>( referenceHolder, new ConditionalEventListener<T,IEventValidator>( evnt, () => validation() ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, System.Action<T,IEventValidator> evnt, IEventValidator validator ) { Synchronize( referenceHolder, validator.Validated<T,IEventValidator>( evnt ) ); }
	// public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T,IEventValidator> evnt, System.Func<bool> validation ) { Synchronize<T>( referenceHolder, new ConditionalEventListener<T,IEventValidator>( evnt, () => validation() && evnt.IsValid ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T, IEventValidator> evnt, IEventValidator validator ) { Synchronize( referenceHolder, validator.Validated<T, IEventValidator>( evnt ) ); }
	public static void Synchronize<T>( this IReferenceHolder<T> referenceHolder, IEventTrigger<T, IEventValidator> evnt, bool evenDefault = true )
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
	public bool IsNull => _current == null;

	// TODO: LazyOptimization
	private EventSlot<T> _onReferenceRemove;
	private EventSlot<T, IEventValidator> _onReferenceSet;
	private ConditionalEventsCollection _validator;
	// private EventSlot<T> _onReferenceRemove = new EventSlot<T>();
	// private EventSlot<T, IEventValidator> _onReferenceSet = new EventSlot<T,IEventValidator>();
	// private ConditionalEventsCollection _validator = new ConditionalEventsCollection();

	public IEventRegister<T> OnReferenceRemove => Lazy.Request( ref _onReferenceRemove );
	public IEventRegister<T, IEventValidator> OnReferenceSet => Lazy.Request( ref _onReferenceSet );
	public IEventValidator Validator => Lazy.Request( ref _validator );

	public CachedReference( T startData = default( T ) )
	{
		_current = startData;
	}

	public void Clear()
	{
		ChangeReference( default(T) );
	}

	public virtual void Kill()
	{
		_onReferenceSet?.Kill();
		_onReferenceRemove?.Kill();
		_validator?.Kill();
		_current = default(T);
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
}