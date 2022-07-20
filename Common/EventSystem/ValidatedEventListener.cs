

public sealed class ValidatedEventListener : BaseConditionalEventListener, IEventTrigger
{
	IEventTrigger _evnt;
	private readonly int _hashCode = -1;
	public override bool IsValid => _evnt != null && base.IsValid && _evnt.IsValid;

	public ValidatedEventListener( System.Action action, IEventValidator validator ) : this( new ActionEventCapsule( action ), validator ) { }
	public ValidatedEventListener( IEventTrigger evnt, IEventValidator validator ) : base( validator.CurrentValidationCheck )
	{
		_evnt = evnt;
		validator.OnVoid.Register( new CallOnce( Clear ) );
		if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _evnt = null; _condition = null; }
	public void Trigger() { _evnt.Trigger(); }

	public override bool Equals( object obj )
	{
		if( obj is ConditionalEventListener && _evnt != null && _condition != null )
		{
			var del = (ValidatedEventListener)obj;
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() => _hashCode;
}

public sealed class ValidatedEventListener<T> : BaseConditionalEventListener, IEventTrigger<T>
{
	IEventTrigger<T> _evnt;
	private readonly int _hashCode = -1;
	public override bool IsValid => _evnt != null && base.IsValid && _evnt.IsValid;

	public ValidatedEventListener( System.Action<T> action, IEventValidator validator ) : this( new ActionEventCapsule<T>( action ), validator ) { }
	public ValidatedEventListener( IEventTrigger<T> evnt, IEventValidator validator ) : base( validator.CurrentValidationCheck )
	{
		_evnt = evnt;
		validator.OnVoid.Register( new CallOnce( Clear ) );
		if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _evnt = null; _condition = null; }
	public void Trigger( T t ) { _evnt.Trigger( t ); }

	public override bool Equals( object obj )
	{
		if( obj is ConditionalEventListener<T> && _evnt != null && _condition != null )
		{
			var del = (ValidatedEventListener<T>)obj;
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() => _hashCode;
}

public sealed class ValidatedEventListener<T, K> : BaseConditionalEventListener, IEventTrigger<T, K>
{
	IEventTrigger<T, K> _evnt;
	private readonly int _hashCode = -1;
	public override bool IsValid => _evnt != null && base.IsValid && _evnt.IsValid;

	public ValidatedEventListener( System.Action<T, K> action, IEventValidator validator ) : this( new ActionEventCapsule<T, K>( action ), validator ) { }
	public ValidatedEventListener( IEventTrigger<T, K> evnt, IEventValidator validator ) : base( validator.CurrentValidationCheck )
	{
		_evnt = evnt;
		validator.OnVoid.Register( new CallOnce( Clear ) );
		if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _evnt = null; _condition = null; }
	public void Trigger( T t, K k ) { _evnt.Trigger( t, k ); }

	public override bool Equals( object obj )
	{
		if( obj is ConditionalEventListener<T, K> && _evnt != null && _condition != null )
		{
			var del = (ValidatedEventListener<T, K>)obj;
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() => _hashCode;
}

public sealed class ValidatedEventListener<T,K,J> : BaseConditionalEventListener, IEventTrigger<T,K,J>
{
	IEventTrigger<T,K,J> _evnt;
	private readonly int _hashCode = -1;
	public override bool IsValid => _evnt != null && base.IsValid && _evnt.IsValid;

	public ValidatedEventListener( System.Action<T,K,J> action, IEventValidator validator ) : this( new ActionEventCapsule<T,K,J>( action ), validator ) { }
	public ValidatedEventListener( IEventTrigger<T,K,J> evnt, IEventValidator validator ) : base( validator.CurrentValidationCheck )
	{
		_evnt = evnt;
		validator.OnVoid.Register( new CallOnce( Clear ) );
		if( _evnt != null || _condition != null ) _hashCode = _evnt.GetHashCode() + _condition.GetHashCode();
	}

	public void Clear() { _evnt = null; _condition = null; }
	public void Trigger( T t, K k, J j ) { _evnt.Trigger( t, k, j ); }

	public override bool Equals( object obj )
	{
		if( obj is ConditionalEventListener<T,K,J> && _evnt != null && _condition != null )
		{
			var del = (ValidatedEventListener<T,K,J>)obj;
			return _evnt.Equals( del._evnt ) && _condition.Equals( del._condition );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() => _hashCode;
}
