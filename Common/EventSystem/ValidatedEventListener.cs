

[UnityEngine.HideInInspector]
public sealed class ValidatedEventListener : BaseConditionalEventListener, IEventTrigger
{
	IEventTrigger _evnt;
	private readonly int _hashCode = -1;
	public override bool IsValid => _evnt != null && base.IsValid && _evnt.IsValid;

	public ValidatedEventListener( System.Action action, IEventValidator validator, IEventValidator additionalValidator = null ) : this( new ActionCapsule( action ), validator, additionalValidator ) { }
	public ValidatedEventListener( IEventTrigger evnt, IEventValidator validator, IEventValidator additionalValidator = null ) : base( validator.TryCombineValidationCheck( additionalValidator ) )
	{
		if( _condition == null || !_condition() ) return;
		_evnt = evnt;
		var clearOnce = new CallOnceCapsule( Clear );
		validator.OnVoid.Register( clearOnce );
		if( additionalValidator != null ) additionalValidator.OnVoid.Register( clearOnce );
		if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _evnt = null; _condition = null; }
	public void Trigger() { _evnt.Trigger(); }

	public override bool Equals( object obj )
	{
		if( _evnt != null && _condition != null && obj is ValidatedEventListener del )
		{
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() => _hashCode;
}

[UnityEngine.HideInInspector]
public sealed class ValidatedEventListener<T> : BaseConditionalEventListener, IEventTrigger<T>
{
	IEventTrigger<T> _evnt;
	private readonly int _hashCode = -1;
	public override bool IsValid => _evnt != null && base.IsValid && _evnt.IsValid;

	public ValidatedEventListener( System.Action<T> action, IEventValidator validator, IEventValidator additionalValidator = null ) : this( new ActionCapsule<T>( action ), validator, additionalValidator ) { }
	public ValidatedEventListener( IEventTrigger<T> evnt, IEventValidator validator, IEventValidator additionalValidator = null ) : base( validator.TryCombineValidationCheck( additionalValidator ) )
	{
		if( _condition == null || !_condition() ) return;
		_evnt = evnt;
		var clearOnce = new CallOnceCapsule( Clear );
		validator.OnVoid.Register( clearOnce );
		if( additionalValidator != null ) additionalValidator.OnVoid.Register( clearOnce );
		if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _evnt = null; _condition = null; }
	public void Trigger( T t ) { _evnt.Trigger( t ); }

	public override bool Equals( object obj )
	{
		if( _evnt != null && _condition != null && obj is ValidatedEventListener<T> del)
		{
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() => _hashCode;
}

[UnityEngine.HideInInspector]
public sealed class ValidatedEventListener<T, K> : BaseConditionalEventListener, IEventTrigger<T, K>
{
	IEventTrigger<T, K> _evnt;
	private readonly int _hashCode = -1;
	public override bool IsValid => _evnt != null && base.IsValid && _evnt.IsValid;

	public ValidatedEventListener( System.Action<T, K> action, IEventValidator validator, IEventValidator additionalValidator = null ) : this( new ActionCapsule<T, K>( action ), validator, additionalValidator ) { }
	public ValidatedEventListener( IEventTrigger<T, K> evnt, IEventValidator validator, IEventValidator additionalValidator = null ) : base( validator.TryCombineValidationCheck( additionalValidator ) )
	{
		if( _condition == null || !_condition() ) return;
		_evnt = evnt;
		var clearOnce = new CallOnceCapsule( Clear );
		validator.OnVoid.Register( clearOnce );
		if( additionalValidator != null ) additionalValidator.OnVoid.Register( clearOnce );
		if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _evnt = null; _condition = null; }
	public void Trigger( T t, K k ) { _evnt.Trigger( t, k ); }

	public override bool Equals( object obj )
	{
		if( _evnt != null && _condition != null && obj is ValidatedEventListener<T,K> del)
		{
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() => _hashCode;
}

[UnityEngine.HideInInspector]
public sealed class ValidatedEventListener<T,K,J> : BaseConditionalEventListener, IEventTrigger<T,K,J>
{
	IEventTrigger<T,K,J> _evnt;
	private readonly int _hashCode = -1;
	public override bool IsValid => _evnt != null && base.IsValid && _evnt.IsValid;

	public ValidatedEventListener( System.Action<T,K,J> action, IEventValidator validator, IEventValidator additionalValidator = null ) : this( new ActionCapsule<T,K,J>( action ), validator, additionalValidator ) { }
	public ValidatedEventListener( IEventTrigger<T,K,J> evnt, IEventValidator validator, IEventValidator additionalValidator = null ) : base( validator.TryCombineValidationCheck( additionalValidator ) )
	{
		if( _condition == null || !_condition() ) return;
		_evnt = evnt;
		var clearOnce = new CallOnceCapsule( Clear );
		validator.OnVoid.Register( clearOnce );
		if( additionalValidator != null ) additionalValidator.OnVoid.Register( clearOnce );
		if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _evnt = null; _condition = null; }
	public void Trigger( T t, K k, J j ) { _evnt.Trigger( t, k, j ); }

	public override bool Equals( object obj )
	{
		if( _evnt != null && _condition != null && obj is ValidatedEventListener<T,K,J> del)
		{
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() => _hashCode;
}