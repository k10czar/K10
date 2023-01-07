using UnityEngine;

public class ColorState : IValueState<Color>
{
    Color _value;
    EventSlot<Color> _onChange;

	public Color Value => _value;
	public IEventRegister<Color> OnChange => _onChange ?? ( _onChange = new EventSlot<Color>() );

    public ColorState( Color initialValue )
    {
        _value = initialValue;
    }

	public Color Get() => _value;
	public void Setter( Color newValue )
	{
		_value = newValue;
        _onChange?.Trigger( _value );
	}
}
