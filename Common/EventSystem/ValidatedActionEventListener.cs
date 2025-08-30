using System;


[UnityEngine.HideInInspector]
public sealed class ValidatedActionEventListener : BaseConditionalEventListener, IEventTrigger
{
	Action _action;
	private int _hashCode = -1;
	public override bool IsValid => _action != null && base.IsValid;

	public ValidatedActionEventListener( Action action, IEventValidator validator, IEventValidator additionalValidator = null ) : base( validator.TryCombineValidationCheck( additionalValidator ) )
	{
		if( _condition == null || !_condition() ) return;
		_action = action;
		var clearOnce = new CallOnce( Clear );
		validator.OnVoid.Register( clearOnce );
		if( additionalValidator != null ) additionalValidator.OnVoid.Register( clearOnce );
		// if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _action = null; _condition = null; _hashCode = 0; }
	public void Trigger() { _action?.Invoke(); }

	public override bool Equals( object obj )
	{
		if (obj == null) return this == null;
		if (GetHashCode() != obj.GetHashCode()) return false;
		if (_action != null && _condition != null && obj is ValidatedActionEventListener del)
		{
			return _action.Equals(del._action) && _condition.Equals(del._condition);
		}
		return base.Equals( obj );
	}

	private int CalculateHashCode() => ( _action != null && _condition != null ) ? ( _action.GetHashCode() + _condition.GetHashCode() ) : 0;
	public override int GetHashCode() => (_hashCode != -1) ? _hashCode : ( _hashCode = CalculateHashCode() );
}


[UnityEngine.HideInInspector]
public sealed class ValidatedActionEventListener<T> : BaseConditionalEventListener, IEventTrigger<T>
{
	Action<T> _action;
	private int _hashCode = -1;
	public override bool IsValid => _action != null && base.IsValid;

	public ValidatedActionEventListener( Action<T> action, IEventValidator validator, IEventValidator additionalValidator = null ) : base( validator.TryCombineValidationCheck( additionalValidator ) )
	{
		if( _condition == null || !_condition() ) return;
		_action = action;
		var clearOnce = new CallOnce( Clear );
		validator.OnVoid.Register( clearOnce );
		if( additionalValidator != null ) additionalValidator.OnVoid.Register( clearOnce );
		// if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _action = null; _condition = null; _hashCode = 0; }
	public void Trigger( T t ) { _action?.Invoke( t ); }

	public override bool Equals( object obj )
	{
		if (obj == null) return this == null;
		if (GetHashCode() != obj.GetHashCode()) return false;
		if (_action != null && _condition != null && obj is ValidatedActionEventListener<T> del)
		{
			return _action.Equals(del._action) && _condition.Equals(del._condition);
		}
		return base.Equals( obj );
	}

	private int CalculateHashCode() => ( _action != null && _condition != null ) ? ( _action.GetHashCode() + _condition.GetHashCode() ) : 0;
	public override int GetHashCode() => (_hashCode != -1) ? _hashCode : ( _hashCode = CalculateHashCode() );
}


[UnityEngine.HideInInspector]
public sealed class ValidatedActionEventListener<T,K> : BaseConditionalEventListener, IEventTrigger<T,K>
{
	Action<T,K> _action;
	private int _hashCode = -1;
	public override bool IsValid => _action != null && base.IsValid;

	public ValidatedActionEventListener( Action<T,K> action, IEventValidator validator, IEventValidator additionalValidator = null ) : base( validator.TryCombineValidationCheck( additionalValidator ) )
	{
		if( _condition == null || !_condition() ) return;
		_action = action;
		var clearOnce = new CallOnce( Clear );
		validator.OnVoid.Register( clearOnce );
		if( additionalValidator != null ) additionalValidator.OnVoid.Register( clearOnce );
		// if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _action = null; _condition = null; _hashCode = 0; }
	public void Trigger( T t, K k ) { _action?.Invoke( t, k ); }

	public override bool Equals( object obj )
	{
		if (obj == null) return this == null;
		if (GetHashCode() != obj.GetHashCode()) return false;
		if (_action != null && _condition != null && obj is ValidatedActionEventListener<T,K> del)
		{
			return _action.Equals(del._action) && _condition.Equals(del._condition);
		}
		return base.Equals( obj );
	}

	private int CalculateHashCode() => ( _action != null && _condition != null ) ? ( _action.GetHashCode() + _condition.GetHashCode() ) : 0;
	public override int GetHashCode() => (_hashCode != -1) ? _hashCode : ( _hashCode = CalculateHashCode() );
}


[UnityEngine.HideInInspector]
public sealed class ValidatedActionEventListener<T,K,J> : BaseConditionalEventListener, IEventTrigger<T,K,J>
{
	Action<T,K,J> _action;
	private int _hashCode = -1;
	public override bool IsValid => _action != null && base.IsValid;

	public ValidatedActionEventListener( Action<T,K,J> action, IEventValidator validator, IEventValidator additionalValidator = null ) : base( validator.TryCombineValidationCheck( additionalValidator ) )
	{
		if( _condition == null || !_condition() ) return;
		_action = action;
		var clearOnce = new CallOnce( Clear );
		validator.OnVoid.Register( clearOnce );
		if( additionalValidator != null ) additionalValidator.OnVoid.Register( clearOnce );
		// if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _action = null; _condition = null; _hashCode = 0; }
	public void Trigger( T t, K k, J j ) { _action?.Invoke( t, k, j ); }

	public override bool Equals( object obj )
	{
		if (obj == null) return this == null;
		if (GetHashCode() != obj.GetHashCode()) return false;
		if (_action != null && _condition != null && obj is ValidatedActionEventListener<T,K,J> del)
		{
			return _action.Equals(del._action) && _condition.Equals(del._condition);
		}
		return base.Equals( obj );
	}

	private int CalculateHashCode() => ( _action != null && _condition != null ) ? ( _action.GetHashCode() + _condition.GetHashCode() ) : 0;
	public override int GetHashCode() => (_hashCode != -1) ? _hashCode : ( _hashCode = CalculateHashCode() );
}
