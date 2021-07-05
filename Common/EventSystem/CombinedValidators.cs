using System;

public class CombinedValidators : IEventValidator
{
	private readonly IEventValidator[] _validators;
	private EventSlot _onVoid;

	public Func<bool> CurrentValidationCheck
	{ 
		get
		{
			var validationChecks = new Func<bool>[_validators.Length];
			for( int i = 0; i < _validators.Length; i++ ) validationChecks[i] = _validators[i].CurrentValidationCheck;
			return () =>
			{
				for( int i = 0; i < validationChecks.Length; i++ )
					if( !validationChecks[i]() ) return false;
				return true;
			};
		}
	}

	public IEventRegister OnVoid 
	{ 
		get 
		{
			if( _onVoid == null )
			{
				_onVoid = new EventSlot();
				foreach( var v in _validators ) v.OnVoid.Register( _onVoid );
			}
			return _onVoid;
		}
	}

	public CombinedValidators( params IEventValidator[] validators )
	{
		_validators = validators;
	}
}
