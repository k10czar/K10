using System;
using System.Collections.Generic;

public interface IEventValidator
{
	Func<bool> CurrentValidationCheck { get; }
	IEventRegister OnVoid { get; }
}

public interface IVoidableEventValidator : IEventValidator
{
	void Void();
}

public class NullValidator : IEventValidator
{
	private static readonly NullValidator _instance = new NullValidator();
	public static IEventValidator Instance => _instance;

	public Func<bool> CurrentValidationCheck => FuncBool.EverFalse;
	public IEventRegister OnVoid => FakeEventCallOnRegister.Instance;
}

public class ConditionalEventsCollection : IVoidableEventValidator
{
	int _validatorParity = 0;
	Func<bool> _currentValidationCheck;
	EventSlot _onVoid;

	public IEventRegister OnVoid => _onVoid ?? ( _onVoid = new EventSlot() );

	public Func<bool> CurrentValidationCheck
	{
		get
		{
			if( _currentValidationCheck == null ) _currentValidationCheck = BuildNewValidationCheck();
			return _currentValidationCheck;
		}
	}

	private Func<bool> BuildNewValidationCheck()
	{
		var currID = _validatorParity;
		return () => currID == _validatorParity;
	}

	public void Clear()
	{
		_onVoid?.Clear();
		_onVoid = null;
	}

	public void Void() 
	{
		_currentValidationCheck = null;
		_validatorParity = ( _validatorParity + 1 ) % int.MaxValue;
		_onVoid?.Trigger();
	}
}

public class ConditionalEventsCollectionBS : IVoidableEventValidator
{
	BoolState _currentValidation = new BoolState( true );
	public Func<bool> CurrentValidationCheck { get { return _currentValidation.Get; } }
	public IBoolStateObserver CurrentEventsValidation { get { return _currentValidation; } }

	EventSlot _onVoid = new EventSlot();
	public IEventRegister OnVoid => _onVoid;

	public void Void()
	{
		_currentValidation.SetFalse();
		_currentValidation = new BoolState( true );
		_onVoid.Trigger();
	}
}

public class LifetimeEventValidator : IEventValidator
{
	EventSlot _onVoid = new EventSlot();
	public IEventRegister OnVoid => _onVoid;
	readonly BoolState _lifetimeValidatior = new BoolState( true );
	public Func<bool> CurrentValidationCheck { get { return _lifetimeValidatior.Get; } }

	public void Void() { _lifetimeValidatior.SetFalse(); _onVoid.Trigger(); }
}

public static class EventValidatorExtentions
{
	// public static IEventTrigger<T, K, J> TryValidated<T, K, J>( this IEventValidator validator, Action<T, K, J> act ) => ( validator != null ) ? Validated<T, K, J>( validator, act ) : new ActionEventCapsule<T,K,J>( act );
	// public static IEventTrigger<T, K, J> TryValidated<T, K, J>( this IEventValidator validator, IEventTrigger<T, K, J> act ) => ( validator != null ) ? Validated<T, K, J>( validator, act ) : act;

	// public static IEventTrigger<T, K> TryValidated<T, K>( this IEventValidator validator, Action<T, K> act ) => ( validator != null ) ? Validated<T, K>( validator, act ) : new ActionEventCapsule<T,K>( act );
	// public static IEventTrigger<T, K> TryValidated<T, K>( this IEventValidator validator, IEventTrigger<T, K> act ) => ( validator != null ) ? Validated<T, K>( validator, act ) : act;

	// public static IEventTrigger<T> TryValidated<T>( this IEventValidator validator, Action<T> act ) => ( validator != null ) ? Validated<T>( validator, act ) : new ActionEventCapsule<T>( act );
	// public static IEventTrigger<T> TryValidated<T>( this IEventValidator validator, IEventTrigger<T> act ) => ( validator != null ) ? Validated<T>( validator, act ) : act;

	// public static IEventTrigger TryValidated( this IEventValidator validator, Action act ) => ( validator != null ) ? Validated( validator, act ) : new ActionEventCapsule( act );
	// public static IEventTrigger TryValidated( this IEventValidator validator, IEventTrigger act ) => ( validator != null ) ? Validated( validator, act ) : act;

	// public static IEventTrigger<T> TryValidated<T>( this IEventValidator validator, Action<T> act, UnityEngine.Transform transform )
	// {
	// 	var boxedValidation = ( validator != null ) ? validator.CurrentValidationCheck : () => true;
	// 	return new ConditionalEventListener<T>( act, () => { return transform != null && boxedValidation(); } );
	// }

	// public static IEventTrigger TryValidated( this IEventValidator validator, Action act, UnityEngine.Transform transform )
	// {
	// 	var boxedValidation = ( validator != null ) ? validator.CurrentValidationCheck : () => true;
	// 	return new ConditionalEventListener( act, () => { return transform != null && boxedValidation(); } );
	// }

	public static System.Func<bool>[] GetCurrentValidators( this IEnumerable<IEventValidator> validators )
	{
		var count = 0;
		foreach( var val in validators ) count++;
		var vals = new System.Func<bool>[count];
		int i = 0;
		foreach( var val in validators ) vals[i++] = val.CurrentValidationCheck;
		return vals;
	}

	public static Func<bool> TryCombineValidationCheck( this IEventValidator validator, IEventValidator additionalValidator = null )
	{
		var masterCondition = validator.CurrentValidationCheck;
		if( additionalValidator == null ) return masterCondition;
		var addedCondition = additionalValidator.CurrentValidationCheck;
		return () => ( masterCondition() && addedCondition() );
	}

	public static IEventTrigger<T, K, J> Validated<T, K, J>( this IEventValidator validator, Action<T, K, J> act ) => new ValidatedEventListener<T, K, J>( act, validator );
	public static IEventTrigger<T, K, J> Validated<T, K, J>( this IEventValidator validator, IEventTrigger<T, K, J> act ) => new ValidatedEventListener<T, K, J>( act, validator );

	public static IEventTrigger<T, K> Validated<T, K>( this IEventValidator validator, Action<T, K> act ) => new ValidatedEventListener<T, K>( act, validator );
	public static IEventTrigger<T, K> Validated<T, K>( this IEventValidator validator, IEventTrigger<T, K> act ) => new ValidatedEventListener<T, K>( act, validator );

	public static IEventTrigger<T> Validated<T>( this IEventValidator validator, Action<T> act ) => new ValidatedEventListener<T>( act, validator );
	public static IEventTrigger<T> Validated<T>( this IEventValidator validator, IEventTrigger<T> act ) => new ValidatedEventListener<T>( act, validator );

	public static IEventTrigger Validated( this IEventValidator validator, Action act ) => new ValidatedEventListener( act, validator );
	public static IEventTrigger Validated( this IEventValidator validator, IEventTrigger act ) => new ValidatedEventListener( act, validator );

	public static IEventTrigger ValidatedVoid( this IVoidableEventValidator validator ) => validator.Validated( validator.Void );

	// public static IEventTrigger<T,K> Validated<T,K>( this IEventValidator validator, Action<T, K> act, UnityEngine.Transform transform ) => new ValidatedEventListener<T, K>( act, CombinedCondition( validator, transform ) );
	// public static IEventTrigger<T> Validated<T>( this IEventValidator validator, Action<T> act, UnityEngine.Transform transform ) => new ValidatedEventListener<T>( act, CombinedCondition( validator, transform ) );
	// public static IEventTrigger Validated( this IEventValidator validator, Action act, UnityEngine.Transform transform ) => new ValidatedEventListener( act, CombinedCondition( validator, transform ) );

	public static IEventTrigger<T,K> Validated<T, K>( this IEventValidator validator, Action<T, K> act, IEventValidator aditionalValidator ) => new ValidatedEventListener<T, K>( act, validator, aditionalValidator );
	public static IEventTrigger<T> Validated<T>( this IEventValidator validator, Action<T> act, IEventValidator aditionalValidator ) => new ValidatedEventListener<T>( act, validator, aditionalValidator );
	public static IEventTrigger Validated( this IEventValidator validator, Action act, IEventValidator aditionalValidator ) => new ValidatedEventListener( act, validator, aditionalValidator );
}
