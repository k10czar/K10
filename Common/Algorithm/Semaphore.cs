using System;
using System.Linq;
using System.Collections.Generic;

public interface ISemaphoreInfo : IBoolStateObserver
{
	bool Free { get; }

	int GetBlockCount(object key);

	bool HasBlocker(object key);
	IEventRegister OnBlock { get; }
	IEventRegister OnRelease { get; }
	IEventRegister<bool> OnStateChange { get; }
}

public interface ISemaphoreInterection
{
	void Interact( object key, bool block );
	bool Block( object obj, bool increaseBlock = true );
	bool BlockButDoNotIncrease( object obj );
	void Release( object obj );
	IEventValidator Validator { get; }
}

public static class ISemaphoreInterectionExtentions
{
	public static void BlockOn( this ISemaphoreInterection semaphore, IValueStateObserver<bool> source ) 
	{
		source.Synchronize( semaphore.Validator.Validated<bool>( ( value ) => { if( value ) semaphore.Block( source ); else semaphore.Release( source ); } ) );
	}

	public static void ReleaseOn( this ISemaphoreInterection semaphore, IValueStateObserver<bool> source )
	{ 
		source.Synchronize( semaphore.Validator.Validated<bool>( ( value ) => { if( value ) semaphore.Release( source ); else semaphore.Block( source ); } ) );
	}

	// public static void BlockOn( this ISemaphoreInterection semaphore, IValueStateObserver<bool> source, Func<bool> eventValidation )
	// {
	// 	source.Synchronize( semaphore.Validator.LeakedValidated<bool>( ( value ) => { if( value ) semaphore.Block( source ); else semaphore.Release( source ); }, eventValidation ) ); 
	// }

	// public static void ReleaseOn( this ISemaphoreInterection semaphore, IValueStateObserver<bool> source, Func<bool> eventValidation )
	// {
	// 	source.Synchronize( semaphore.Validator.LeakedValidated<bool>( ( value ) => { if( value ) semaphore.Release( source ); else semaphore.Block( source ); }, eventValidation ) );
	// }

	public static void BlockOn( this ISemaphoreInterection semaphore, IBoolStateObserver source )
	{
		source.RegisterOnTrue( semaphore.Validator.Validated( () => semaphore.Block( source ) ) );
		source.RegisterOnFalse( semaphore.Validator.Validated( () => semaphore.Release( source ) ) );
	}

	public static void BlockOn( this ISemaphoreInterection semaphore, IBoolStateObserver source, IEventValidator eventValidation )
	{
		Action releaseAction = () => semaphore.Release( source );
		source.RegisterOnTrue( semaphore.Validator.Validated( () => semaphore.Block( source ), eventValidation ) );
		source.RegisterOnFalse( semaphore.Validator.Validated( releaseAction, eventValidation ) );

		eventValidation.OnVoid.Register( new CallOnce( releaseAction ) );
	}

	// public static void BlockOn( this ISemaphoreInterection semaphore, IBoolStateObserver source, Func<bool> eventValidation )
	// {
	// 	source.RegisterOnTrue( semaphore.Validator.LeakedValidated( () => semaphore.Block( source ), eventValidation ) );
	// 	source.RegisterOnFalse( semaphore.Validator.LeakedValidated( () => semaphore.Release( source ), eventValidation ) );
	// }

	public static void ReleaseOn( this ISemaphoreInterection semaphore, IBoolStateObserver source )
	{
		source.RegisterOnTrue( semaphore.Validator.Validated( () => semaphore.Release( source ) ) );
		source.RegisterOnFalse( semaphore.Validator.Validated( () => semaphore.Block( source ) ) );
	}

	// public static void ReleaseOn( this ISemaphoreInterection semaphore, IBoolStateObserver source, Func<bool> eventValidation )
	// {
	// 	source.RegisterOnTrue( semaphore.Validator.LeakedValidated( () => semaphore.Release( source ), eventValidation ) );
	// 	source.RegisterOnFalse( semaphore.Validator.LeakedValidated( () => semaphore.Block( source ), eventValidation ) );
	// }

	public static void ReleaseOn( this ISemaphoreInterection semaphore, IBoolStateObserver source, IEventValidator eventValidation )
	{
		Action releaseAction = () => semaphore.Release( source );
		source.RegisterOnTrue( semaphore.Validator.Validated( releaseAction, eventValidation ) );
		source.RegisterOnFalse( semaphore.Validator.Validated( () => semaphore.Block( source ), eventValidation ) );
		eventValidation.OnVoid.Register( semaphore.Validator.Validated( new CallOnce( releaseAction ) ) );
	}

	public static void BlockOn( this ISemaphoreInterection semaphore, UnityEngine.GameObject go, IBoolStateObserver additionalCondition = null )
	{
		var goEvents = go.EventRelay();
		var name = go.HierarchyNameOrNull();
		var condition = goEvents.IsActive;
		if( additionalCondition != null ) condition = new BoolStateOperations.And( condition, additionalCondition );
		var goLifetime = goEvents.LifetimeValidator;
		Action releaseLambda = () => semaphore.Release( condition );
		var validator = semaphore.Validator;
		condition.RegisterOnTrue( validator.Validated( () => semaphore.Block( condition ), goLifetime ) );
		condition.RegisterOnFalse( validator.Validated( releaseLambda, goLifetime ) );
		goEvents.OnDestroy.Register( validator.Validated( releaseLambda ) );
	}

	public static void ReleaseOn( this ISemaphoreInterection semaphore, UnityEngine.GameObject go, IBoolStateObserver additionalCondition = null )
	{
		var goEvents = go.EventRelay();
		var name = go.HierarchyNameOrNull();
		var validator = semaphore.Validator;
		IBoolStateObserver condition = goEvents.IsActive.Not;
		if( additionalCondition != null ) condition = new BoolStateOperations.And( condition, additionalCondition );
		var goLifetime = goEvents.LifetimeValidator;
		Action releaseLambda = () => semaphore.Release( condition );
		condition.RegisterOnTrue( validator.Validated( releaseLambda, goLifetime ) );
		condition.RegisterOnFalse( validator.Validated( () => semaphore.Block( condition ), goLifetime ) );
		goEvents.OnDestroy.Register( validator.Validated( new CallOnce( releaseLambda ) ) );
	}
}

public interface ISemaphore : ISemaphoreInfo, ISemaphoreInterection
{
}

public class Semaphore : ISemaphore, ICustomDisposableKill
{
	public class SemaphoreObject { public int Value { get; set; } public override string ToString() => Value.ToString(); }

	private readonly Dictionary<object, SemaphoreObject> _semaphores = new Dictionary<object, SemaphoreObject>();
	public bool Free { get { return _semaphores.Count == 0; } }

	// TODO: LazyOptimization
	private EventSlot _blockEvent;
	private EventSlot _releaseEvent;
	private EventSlot<bool> _changeStateEvent;
	private EventSlot _onInteraction;
	// private EventSlot _blockEvent = new EventSlot();
	// private EventSlot _releaseEvent = new EventSlot();
	// private EventSlot<bool> _changeStateEvent = new EventSlot<bool>();
	// private EventSlot _onInteraction = new EventSlot();
	private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();

	public IBoolStateObserver Not => _not.Request( this );
	public IEventRegister OnBlock => Lazy.Request( ref _blockEvent );
	public IEventRegister OnRelease => Lazy.Request( ref _releaseEvent );
	public IEventRegister<bool> OnStateChange => Lazy.Request( ref _changeStateEvent );

	public bool Value { get { return Free; } }
	public bool Get() { return Free; }
	public IEventRegister<bool> OnChange => Lazy.Request( ref _changeStateEvent );
	public IEventRegister OnTrueState => Lazy.Request( ref _releaseEvent );
	public IEventRegister OnFalseState => Lazy.Request( ref _blockEvent );

	public IEventRegister OnInteraction => Lazy.Request( ref _onInteraction );

	private OneTimeValidator _validator = new OneTimeValidator();
	public IEventValidator Validator => _validator;

	public void RegisterAndStart( IEventTrigger<bool> evnt ) { Lazy.Request( ref _changeStateEvent ).Register( _validator.Validated( evnt ) ); evnt.Trigger( Free ); }
	public void RegisterAndStart( System.Action<bool> evnt ) { Lazy.Request( ref _changeStateEvent ).Register( _validator.Validated( evnt ) ); evnt( Free ); }

	public void Kill()
	{
		_onInteraction?.Kill();
		_blockEvent?.Kill();
		_releaseEvent?.Kill();
		_changeStateEvent?.Kill();
		_validator?.Kill();
		_semaphores.Clear();
	}

	public IDictionary<object, SemaphoreObject> Semaphores { get { return _semaphores; } }

	public void BlockOn( IBoolStateObserver source )
	{
		if( source == null ) return;
		source.RegisterOnTrue( _validator.Validated( () => Block( source ) ) );
		source.RegisterOnFalse( _validator.Validated( () => Release( source ) ) );
	}

	// public void BlockOn( IBoolStateObserver source, Func<bool> eventValidation )
	// {
	// 	if( source == null ) return;
	// 	source.RegisterOnTrue( _validator.LeakedValidated( () => Block( source ), eventValidation ) );
	// 	source.RegisterOnFalse( _validator.LeakedValidated( () => Release( source ), eventValidation ) );
	// }

	public void ReleaseOn( IBoolStateObserver source )
	{
		if( source == null ) return;
		source.RegisterOnTrue( _validator.Validated( () => Release( source ) ) );
		source.RegisterOnFalse( _validator.Validated( () => Block( source ) ) );
	}

	// public void ReleaseOn( IBoolStateObserver source, Func<bool> eventValidation )
	// {
	// 	source.RegisterOnTrue( _validator.LeakedValidated( () => Release( source ), eventValidation ) );
	// 	source.RegisterOnFalse( _validator.LeakedValidated( () => Block( source ), eventValidation ) );
	// }

	internal void Toggle( object obj )
	{
		if( _semaphores.ContainsKey( obj ) ) Release( obj );
		else Block( obj );
	}

	public void Interact( object key, bool block )
	{
		if( block ) BlockButDoNotIncrease( key );
		else Release( key );
	}

	public bool BlockButDoNotIncrease( object obj ) => Block( obj, false );
	public bool Block( object obj, bool increaseBlock = true )
	{
		if( obj == null )
			return false;

		bool trigger = Free;

		SemaphoreObject s;
		bool newKey = !_semaphores.TryGetValue( obj, out s );
		if( newKey || increaseBlock )
		{
			if( newKey )
			{
				s = new SemaphoreObject();
				_semaphores.Add( obj, s );
			}
			s.Value++;
			
		}

		if( trigger )
		{
			_blockEvent?.Trigger();
			_changeStateEvent?.Trigger( false );
		}

		_onInteraction?.Trigger();

		return newKey;
	}

	public void Release( object obj )
	{
		if( Free || obj == null )
			return;

		SemaphoreObject s;
		if( _semaphores.TryGetValue( obj, out s ) )
		{
			s.Value--;
			if( s.Value == 0 )
				_semaphores.Remove( obj );
		}

		if( Free )
		{
			_releaseEvent?.Trigger();
			_changeStateEvent?.Trigger( true );
		}
		
		_onInteraction?.Trigger();
	}

	public void Clear()
	{
		var initialState = Free;
		_semaphores.Clear();
		if( !initialState && Free )
		{
			_releaseEvent?.Trigger();
			_changeStateEvent?.Trigger( true );
		}
	}

	string KeyName( object obj )
	{
		if( obj == this ) return "THIS";
		if( obj is string ) return obj.ToStringOrNull();
		return obj.GetType().ToStringOrNull();
		// return obj.ToStringOrNull();
	}

	public int GetBlockCount(object key)
    {
		if(_semaphores.TryGetValue(key, out SemaphoreObject s))
			return s.Value;
		
		return 0;
    }

	public bool HasBlocker(object key) => _semaphores.ContainsKey(key);

	int _toStringCount;
	public override string ToString()
	{
		var freeStr = ( Free ? "Free" : "Blocked" );
		if( _toStringCount > 0 ) return $"**InfinityLoopCondition_{freeStr}Semaphore({this.GetHashCode()})**";
		_toStringCount++;
		var elementsStrings = _semaphores.ToList().ConvertAll( ( obj ) => $"{KeyName( obj.Key.ToStringOrNull() )}({obj.Value.Value})" );
		var elements = String.Join( ", ", elementsStrings );
		_toStringCount--;
		return $"( [{freeStr} Semaphore({this.GetHashCode()}) => {{ {elements} }}] )";
	}
}