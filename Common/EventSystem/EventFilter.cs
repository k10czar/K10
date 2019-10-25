using System;

public class EventFilter<T,K> : IEventTrigger<T>
{
	private readonly IEventTrigger<K> _refinedEvent;
	private readonly Func<T, K> _filterFunction;

	public bool IsValid => _refinedEvent.IsValid;

	public EventFilter( IEventTrigger<K> refinedEvent, Func<T,K> filter )
	{
		_refinedEvent = refinedEvent;
		_filterFunction = filter;
	}

	public void Trigger( T t )
	{
		_refinedEvent.Trigger( _filterFunction( t ) );
	}

	public override bool Equals( object obj )
	{
		if( obj is EventFilter<T,K> )
		{
			var b = (EventFilter<T, K>)obj;
			return GetHashCode().Equals( b.GetHashCode() );
		}
		return base.Equals( obj );
	}

	public override int GetHashCode() => _refinedEvent.GetHashCode();
}
