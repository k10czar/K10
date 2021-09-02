using System;
using UnityEngine;

public interface IGuidStateObserver : IValueStateObserver<Guid>
{
	IBoolStateObserver IsEmpty { get; }
}

public interface IGuidState : IGuidStateObserver, IValueState<Guid>
{
	void Clear();
	void Renew();
}

[System.Serializable]
public class GuidState : IGuidState
{
	[SerializeField] SerializableGUID _value = new SerializableGUID( Guid.Empty );
	[SerializeField] BoolState _isEmpty = new BoolState( true );
	[System.NonSerialized] private EventSlot<Guid> _onChange = new EventSlot<Guid>();

	public GuidState() : this( Guid.Empty ) { }

	public GuidState( Guid initialGuid )
	{
		_value.SetGuid( initialGuid );
		_isEmpty = new BoolState( initialGuid == Guid.Empty );
	}

	public Guid Value => _value.Value;
	public IBoolStateObserver IsEmpty => _isEmpty;
	public IEventRegister<Guid> OnChange => _onChange;

	public static implicit operator Guid( GuidState v ) => v._value.Value;
	public static implicit operator Guid?( GuidState v ) => v._value.Value;

	public Guid Get() => _value.Value;
	public void Setter( Guid value )
	{
		if( _value.Value == value ) return;
		_value.SetGuid( value );
		_isEmpty.Setter( value == Guid.Empty );
		_onChange.Trigger( value );
	}

	public void Clear() { Setter( Guid.Empty ); }
	public void Renew() { Setter( Guid.NewGuid() ); }

	public override string ToString() => _value.ToStringOrNull();
}
