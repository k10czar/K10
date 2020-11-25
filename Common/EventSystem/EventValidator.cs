using System;
using System.Collections.Generic;

public interface IEventValidator
{
	Func<bool> CurrentValidationCheck { get; }
}

public interface IVoidableEventValidator : IEventValidator
{
	void Void();
}

public class ConditionalEventsCollection : IVoidableEventValidator
{
	int _validatorParity = 0;
	Func<bool> _currentValidationCheck;

	public Func<bool> CurrentValidationCheck
	{
		get
		{
			if( _currentValidationCheck == null )
			{
				var currID = _validatorParity;
				_currentValidationCheck = () => currID == _validatorParity;
			}
			return _currentValidationCheck;
		}
	}

	public void Void() { _currentValidationCheck = null; _validatorParity = ( _validatorParity + 1 ) % int.MaxValue; }
}

public class ConditionalEventsCollectionBS : IVoidableEventValidator
{
	BoolState _currentValidation = new BoolState( true );
	public Func<bool> CurrentValidationCheck { get { return _currentValidation.Get; } }
	public IBoolStateObserver CurrentEventsValidation { get { return _currentValidation; } }

	public void Void()
	{
		_currentValidation.SetFalse();
		_currentValidation = new BoolState( true );
	}
}

public class LifetimeEventValidator : IEventValidator
{
	readonly BoolState _lifetimeValidatior = new BoolState( true );
	public Func<bool> CurrentValidationCheck { get { return _lifetimeValidatior.Get; } }

	public void Void() { _lifetimeValidatior.SetFalse(); }
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
		
	static Func<bool> CombinedCondition( IEventValidator validator, IValidatedObject obj )
	{
		var boxedValidation = validator.CurrentValidationCheck;
		return () => { return obj.IsValid && boxedValidation(); };
	}

	static Func<bool> CombinedCondition( IEventValidator validator, System.Func<bool> AdditionalCheck )
	{
		var boxedValidation = validator.CurrentValidationCheck;
		return () => { return AdditionalCheck() && boxedValidation(); };
	}

	static Func<bool> CombinedCondition( IEventValidator validator, UnityEngine.Transform transform )
	{
		var boxedValidation = validator.CurrentValidationCheck;
		return () => { return transform != null && boxedValidation(); };
	}

	public static IEventTrigger<T, K, J> Validated<T, K, J>( this IEventValidator validator, Action<T, K, J> act ) => new ConditionalEventListener<T, K, J>( act, validator.CurrentValidationCheck );
	public static IEventTrigger<T, K, J> Validated<T, K, J>( this IEventValidator validator, IEventTrigger<T, K, J> act ) => new ConditionalEventListener<T, K, J>( act, CombinedCondition( validator, act ) );

	public static IEventTrigger<T, K> Validated<T, K>( this IEventValidator validator, Action<T, K> act ) => new ConditionalEventListener<T, K>( act, validator.CurrentValidationCheck );
	public static IEventTrigger<T, K> Validated<T, K>( this IEventValidator validator, IEventTrigger<T, K> act ) => new ConditionalEventListener<T, K>( act, CombinedCondition( validator, act ) );

	public static IEventTrigger<T> Validated<T>( this IEventValidator validator, Action<T> act ) => new ConditionalEventListener<T>( act, validator.CurrentValidationCheck );
	public static IEventTrigger<T> Validated<T>( this IEventValidator validator, IEventTrigger<T> act ) => new ConditionalEventListener<T>( act, CombinedCondition( validator, act ) );

	public static IEventTrigger Validated( this IEventValidator validator, Action act ) => new ConditionalEventListener( act, validator.CurrentValidationCheck );
	public static IEventTrigger Validated( this IEventValidator validator, IEventTrigger act ) => new ConditionalEventListener( act, CombinedCondition( validator, act ) );

	public static IEventTrigger ValidatedVoid( this IVoidableEventValidator validator ) => validator.Validated( validator.Void );

	public static IEventTrigger<T> Validated<T>( this IEventValidator validator, Action<T> act, UnityEngine.Transform transform ) => new ConditionalEventListener<T>( act, CombinedCondition( validator, transform ) );
	public static IEventTrigger Validated( this IEventValidator validator, Action act, UnityEngine.Transform transform ) => new ConditionalEventListener( act, CombinedCondition( validator, transform ) );

	public static IEventTrigger<T> Validated<T>( this IEventValidator validator, Action<T> act, System.Func<bool> AdditionalCheck ) => new ConditionalEventListener<T>( act, CombinedCondition( validator, AdditionalCheck ) );
	public static IEventTrigger Validated( this IEventValidator validator, Action act, System.Func<bool> AdditionalCheck ) => new ConditionalEventListener( act, CombinedCondition( validator, AdditionalCheck ) );
}