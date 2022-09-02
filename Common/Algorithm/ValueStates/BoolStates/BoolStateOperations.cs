using UnityEngine;
using System.Linq;

namespace BoolStateOperations
{
	public abstract class BoolStateOperation : IBoolStateObserver
	{
		bool _killed;
		protected IBoolStateObserver[] _variables;

		[SerializeField] bool _value;

		[System.NonSerialized] private EventSlot<bool> _onChange;
		[System.NonSerialized] private EventSlot _onTrue;
		[System.NonSerialized] private EventSlot _onFalse;
		[System.NonSerialized] private LazyBoolStateReverterHolder _not = new LazyBoolStateReverterHolder();

		public BoolStateOperation( IEventValidator validator, params IBoolStateObserver[] variables ) : base()
		{
			_variables = variables;
			_value = CalculateValue();
			InitEvents( validator );
		}

		protected virtual void InitEvents( IEventValidator validator )
		{
			for( int i = 0; i < _variables.Length; i++ ) _variables[i].OnChange.Register( validator.Validated( Update ) );
		}

		protected abstract bool CalculateValue();
		protected abstract string SIGN { get; }

		public void Kill()
		{
			_killed = true;
			_onChange?.Kill();
			_onTrue?.Kill();
			_onFalse?.Kill();
			_not.Kill();
			_onChange = null;
			_onTrue = null;
			_onFalse = null;
		}

		public IBoolStateObserver Not => _not.Request( this );
		public IEventRegister<bool> OnChange => Lazy.Request( ref _onChange, _killed );
		public IEventRegister OnTrueState => Lazy.Request( ref _onTrue, _killed );
		public IEventRegister OnFalseState => Lazy.Request( ref _onFalse, _killed );

		public bool Value => _value;
		public bool Get() => _value;

		protected void Update() { Setter( CalculateValue() ); }

		protected virtual void OnValueChange( bool value ) {  }

		public void Setter( bool value )
		{
			if( _value == value ) return;
			_value = value;

			_onChange?.Trigger( value );
			if( value ) _onTrue?.Trigger();
			else _onFalse?.Trigger();
			OnValueChange( value );
		}

		public override string ToString()
		{
			var elements = _variables.ToList().ConvertAll( ( e ) => e.ToString() );
			var expression = string.Join( $" {SIGN} ", elements );
			return $"( {Value} => ( {expression} ) )";
		}
	}

	public abstract class SelectiveBoolStateOperation : BoolStateOperation
	{
		protected readonly ConditionalEventsCollection _selectiveEvents = new ConditionalEventsCollection();
		public SelectiveBoolStateOperation( params IBoolStateObserver[] variables ) : base( null, variables ) { }

		~SelectiveBoolStateOperation() { _selectiveEvents.Void(); }

		protected override void InitEvents( IEventValidator validator ) { }
		protected override void OnValueChange( bool value )
		{
			_selectiveEvents.Void();
			for( int i = 0; i < _variables.Length; i++ ) _variables[i].RegisterOn( !value, _selectiveEvents.Validated( Update ), false );
		}
	}

	public class And : SelectiveBoolStateOperation
	{
		public And( params IBoolStateObserver[] variables ) : base( variables ) { }
		protected override bool CalculateValue() { for( int i = 0; i < _variables.Length; i++ ) if( !_variables[i].Value ) return false; return true; }
		protected override string SIGN => "&&";
	}

	public class Or : SelectiveBoolStateOperation
	{
		public Or( params IBoolStateObserver[] variables ) : base( variables ) { }
		protected override bool CalculateValue() { for( int i = 0; i < _variables.Length; i++ ) if( _variables[i].Value ) return true; return false; }
		protected override string SIGN => "||";
	}

	public class Xor : BoolStateOperation
	{
		public Xor( IEventValidator validator, params IBoolStateObserver[] variables ) : base( validator, variables ) { }

		protected override bool CalculateValue() { var count = 0; for( int i = 0; i < _variables.Length; i++ ) if( _variables[i].Value ) count++; return ( count % 2 ) == 1; }
		protected override string SIGN => "^";
	}

	public class OneHot : BoolStateOperation
	{
		public OneHot( IEventValidator validator, params IBoolStateObserver[] variables ) : base( validator, variables ) { }

		protected override bool CalculateValue() { var count = 0; for( int i = 0; i < _variables.Length; i++ ) if( _variables[i].Value ) count++; return count == 1; }
		protected override string SIGN => "ô";
	}
}