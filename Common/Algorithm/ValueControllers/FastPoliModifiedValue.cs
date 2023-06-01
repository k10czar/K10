

public struct FastPoliModifiedValue
{
	const int DEFAULT_MAX_MODS = 8;

	public float _value;
	public float _minimumValue;
	public float _maximumValue;
	private int _activeModifiers;
	private ValueMod[] _modifiers;

	public FastPoliModifiedValue( float initialValue = 0, float minValue = 0, float maxValue = 1, int maxMods = DEFAULT_MAX_MODS )
	{
		_value = initialValue;
		_modifiers = new ValueMod[maxMods];
		_activeModifiers = 0;
		_minimumValue = minValue;
		_maximumValue = maxValue;
	}

	public void ForceValue( float value )
	{
		_value = value;
		if( _value < _minimumValue ) _value = _minimumValue;
		else if( _value > _maximumValue ) _value = _maximumValue;
		_activeModifiers = 0;
	}
	
	public void Add( IInterpolatedValueOverTime modifier )
	{
		Add( modifier.Value, modifier.Seconds, modifier.Interpolation );
	}

	public void Add( float value, float seconds = 0, IInterpolationFunction interpolation = null )
	{
		if( seconds < float.Epsilon )
		{
			var finalValue = value;
			if( interpolation != null ) finalValue = interpolation.Evaluate( finalValue );
			_value += finalValue;
			if( _value < _minimumValue ) _value = _minimumValue;
			else if( _value > _maximumValue ) _value = _maximumValue;
			return;
		}

		var id = _activeModifiers;
		var len = _modifiers.Length;
		if( id >= len )
		{
			var minValue = float.MaxValue;
			for( int i = 0; i < len; i++ )
			{
				var absVal = _modifiers[i].RemainingValue;
				if( absVal < 0 ) absVal = -absVal;
				if( absVal > minValue ) continue;
				minValue = absVal;
				id = i;
			}
			var val = _modifiers[id].RemainingValue;
			_value += val;
			if( _value < _minimumValue ) _value = _minimumValue;
			else if( _value > _maximumValue ) _value = _maximumValue;
		}
		else
		{
			_activeModifiers++;
		}
		_modifiers[id].Reset( value, seconds, interpolation );
	}

	public void Update( float deltaTime )
	{
        if( _activeModifiers <= 0 ) return;

		var modsum = 0f;

		for( int i = 0; i < _activeModifiers; i++ )
		{
			modsum += _modifiers[i].Update( deltaTime );
			if( _modifiers[i]._isActive ) continue;
			_activeModifiers--;
			for( int j = i + 1; j < _activeModifiers; j++ ) _modifiers[j-1] = _modifiers[j];
			i--;
		}
        
		_value += modsum;
		if( _value < _minimumValue ) _value = _minimumValue;
		else if( _value > _maximumValue ) _value = _maximumValue;
	}
	
	
	public struct ValueMod
	{
		public bool _isActive;
		float _value;
		float _currentValue;
		float _duration;
		float _accTime;

		IInterpolationFunction _interpolation;

		public float RemainingValue
		{
			get
			{
				var finalValue = _value;
				if( _interpolation != null ) finalValue = _interpolation.Evaluate( finalValue );
				return finalValue - _currentValue;
			}
		}
		
		public void Reset( float value, float duration, IInterpolationFunction interpolation = null )
		{
			_isActive = true;
			_value = value;
			_currentValue = 0;
			_accTime = 0;
			_duration = duration;
		}

		public float Update( float deltaTime )
		{
			if( !_isActive ) return 0;
			if( _duration < float.Epsilon )
			{
				_isActive = false;
				return _value;
			}

			var lastValue = _currentValue;
			_accTime += deltaTime;
			var timeToFinish = _duration - _accTime;
			if( timeToFinish < float.Epsilon )
			{
				_accTime = _duration;
				if( _interpolation == null ) _currentValue = _value;
				else _currentValue = _interpolation.Evaluate( _value );
				_isActive = false;
			}
			else
			{
				var percentage = _accTime / _duration;
				_currentValue = _value * percentage;
				if( _interpolation != null ) _currentValue = _interpolation.Evaluate( _currentValue );
			}

			return _currentValue - lastValue;
		}
	}
}