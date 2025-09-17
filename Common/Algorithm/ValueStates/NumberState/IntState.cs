using UnityEngine;

[System.Serializable]
public class IntState : INumericValueState<int>, ICustomDisposableKill
{
	[System.NonSerialized] bool _killed = false;
    [SerializeField] int _value;
    [System.NonSerialized] EventSlot<int> _onChange;

	public IEventRegister<int> OnChange => _killed ? _onChange : _onChange ??= new();
    public int Value { get { return _value; } set { Setter( value ); } }

    public int Get() { return _value; }

    public virtual void Setter( int value )
    {
        if( _value == value ) return;
        _value = value;
		if (_killed) return;
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
		_killed = true;
		_onChange?.Kill();
		_onChange = null;
	}

    public IntState( int initialValue = default ) { _value = initialValue; }


	public override string ToString() { return $"IS({_value})"; }
}