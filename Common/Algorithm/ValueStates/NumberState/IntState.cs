using UnityEngine;

[System.Serializable]
public class IntState : INumericValueState<int>, ICustomDisposableKill
{
    [SerializeField] int _value;
    [System.NonSerialized] EventSlot<int> _onChange;

	public IEventRegister<int> OnChange => Lazy.Request( ref _onChange );
    public int Value { get { return _value; } set { Setter( value ); } }

    public int Get() { return _value; }

    public virtual void Setter( int value )
    {
        if( _value == value ) return;
        _value = value;
        _onChange?.Trigger( value );
	}

	public void Increment( int value = 1 )
	{
		if( value == 0 ) return;
		Setter( _value + value );
	}

	public void PlusOne() => Setter( _value + 1 );
	public void MinusOne() => Setter( _value - 1 );

	public void Kill()
	{
		_onChange?.Kill();
		_onChange = null;
	}

    public IntState( int initialValue = default( int ) ) { _value = initialValue; }


	public override string ToString() { return $"IS({_value})"; }
}