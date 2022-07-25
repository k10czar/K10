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
	[System.NonSerialized] private BoolState _isEmpty;
	[System.NonSerialized] private EventSlot<Guid> _onChange;

	public GuidState() : this( Guid.Empty ) { }
	public GuidState( Guid initialGuid ) { _value.SetGuid( initialGuid ); }

	public Guid Value => _value.Value;
	public IBoolStateObserver IsEmpty {
		get
		{
			if( _isEmpty == null ) _isEmpty = new BoolState( _value.Value == Guid.Empty );
			return _isEmpty;
		}
	}
	public IEventRegister<Guid> OnChange => _onChange ?? ( _onChange = new EventSlot<Guid>() );

	public static implicit operator Guid( GuidState v ) => v._value.Value;
	public static implicit operator Guid?( GuidState v ) => v._value.Value;

	public Guid Get() => _value.Value;
	public void Setter( Guid value )
	{
		if( _value.Value == value ) return;
		_value.SetGuid( value );
		_isEmpty?.Setter( value == Guid.Empty );
		_onChange?.Trigger( value );
	}
	
	public void Clear()
	{
		_isEmpty?.Clear();
		_onChange?.Clear();
		_isEmpty = null;
		_onChange = null;
	}

	public void ClearGuid() { Setter( Guid.Empty ); }
	public void Renew() { Setter( Guid.NewGuid() ); }

	public override string ToString() => _value.ToStringOrNull();
}
