using UnityEngine;

[System.Serializable]
public class FloatState : INumericValueState<float>, ICustomDisposableKill
{
	[System.NonSerialized] bool _killed = false;
	[SerializeField] float _value;
	[System.NonSerialized] private EventSlot<float> _onChange;

	public float Value { get { return _value; } set { Setter( value ); } }
	public float Get() { return _value; }

	public void SetInt( int value ) { Setter( value ); }

	public void Setter( float value )
	{
		var diff = _value - value;
		if( diff < MathAdapter.EP2 && diff > MathAdapter.NEG_EP2 ) return;
		_value = value;
		if (_killed) return;
		_onChange?.Trigger( value );
	}

	public void Increment( float increment )
	{
		if( increment < MathAdapter.EP2 && increment > MathAdapter.NEG_EP2 ) return;
		Setter( _value + increment );
	}

	public void Kill()
	{
		_killed = true;
		_onChange?.Kill();
		_onChange = null;
	}

	public void Recycle()
	{
		_onChange.Clear();
	}

	public IEventRegister<float> OnChange => _killed ? _onChange : _onChange ??= new();

	public FloatState() : this( default ) { }
	public FloatState( float initialValue ) { _value = initialValue; }
	public FloatState( int eventProvision, int genericEventProvision )  : this() { _onChange = new(eventProvision, genericEventProvision); }


	public override string ToString() { return $"FS({_value})"; }
}