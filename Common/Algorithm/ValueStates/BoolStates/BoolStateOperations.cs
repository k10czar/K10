using UnityEngine;
using System.Linq;

namespace BoolStateOperations
{
	public class Not : IBoolStateObserver
	{
		IBoolStateObserver _variable;
		readonly ConditionalEventsCollection _events = new ConditionalEventsCollection();

		EventSlot<bool> _onChange = new EventSlot<bool>();

		public bool Value => !_variable.Value;
		public bool Get() => !_variable.Value;
		public IEventRegister<bool> OnChange => _onChange;
		public IEventRegister OnTrueState => _variable.OnFalseState;
		public IEventRegister OnFalseState => _variable.OnTrueState;

		~Not() { _events.Void(); }
		public Not( IBoolStateObserver variable )
		{
			_variable = variable;
			_variable.OnChange.Register( _events.Validated<bool>( EventReverter ) );
		}

		void EventReverter( bool value ) { _onChange.Trigger( !value ); }

		public override string ToString() { return $"( {Value} => !{_variable} )"; }
	}

	public abstract class BoolStateOperation : BoolState
	{
		protected IBoolStateObserver[] _variables;
		readonly ConditionalEventsCollection _events = new ConditionalEventsCollection();

		~BoolStateOperation() { _events.Void(); }
		public BoolStateOperation( params IBoolStateObserver[] variables ) : base()
		{
			_variables = variables;
			Value = CalculateValue();
			this.Synchronize( OnValueChange );
		}

		protected abstract bool CalculateValue();
		protected abstract string SIGN { get; }

		void Update() { Value = CalculateValue(); }

		void OnValueChange( bool value )
		{
			_events.Void();
			for( int i = 0; i < _variables.Length; i++ ) _variables[i].RegisterOn( !value, _events.Validated( Update ), false );
		}

		public override string ToString()
		{
			var elements = _variables.ToList().ConvertAll( ( e ) => e.ToString() );
			var expression = string.Join( $" {SIGN} ", elements );
			return $"( {Value} => ( {expression} ) )";
		}
	}

	public class And : BoolStateOperation
	{
		public And( params IBoolStateObserver[] variables ) : base( variables ) { }

		protected override bool CalculateValue() { for( int i = 0; i < _variables.Length; i++ ) if( !_variables[i].Value ) return false; return true; }
		protected override string SIGN => "&&";
	}

	public class Or : BoolStateOperation
	{
		public Or( params IBoolStateObserver[] variables ) : base( variables ) { }

		protected override bool CalculateValue() { for( int i = 0; i < _variables.Length; i++ ) if( _variables[i].Value ) return true; return false; }
		protected override string SIGN => "||";
	}

	public class Xor : BoolState
	{
		protected IBoolStateObserver[] _variables;
		readonly ConditionalEventsCollection _events = new ConditionalEventsCollection();

		~Xor() { _events.Void(); }
		public Xor( params IBoolStateObserver[] variables ) : base()
		{
			_variables = variables;
			Value = CalculateValue();
			for( int i = 0; i < _variables.Length; i++ ) _variables[i].OnChange.Register( _events.Validated( Update ) );
		}

		void Update() { Value = CalculateValue(); }

		protected bool CalculateValue() { var count = 0; for( int i = 0; i < _variables.Length; i++ ) if( _variables[i].Value ) count++; return ( count % 2 ) == 1; }
		protected string SIGN => "^";


		public override string ToString()
		{
			var elements = _variables.ToList().ConvertAll( ( e ) => e.ToString() );
			var expression = string.Join( $" {SIGN} ", elements );
			return $"( {Value} => ( {expression} ) )";
		}
	}

	public class OneHot : BoolState
	{
		protected IBoolStateObserver[] _variables;
		readonly ConditionalEventsCollection _events = new ConditionalEventsCollection();

		~OneHot() { _events.Void(); }
		public OneHot( params IBoolStateObserver[] variables ) : base()
		{
			_variables = variables;
			Value = CalculateValue();
			for( int i = 0; i < _variables.Length; i++ ) _variables[i].OnChange.Register( _events.Validated( Update ) );
		}

		void Update() { Value = CalculateValue(); }

		protected bool CalculateValue() { var count = 0; for( int i = 0; i < _variables.Length; i++ ) if( _variables[i].Value ) count++; return count == 1; }
		protected string SIGN => "ô";


		public override string ToString()
		{
			var elements = _variables.ToList().ConvertAll( ( e ) => e.ToString() );
			var expression = string.Join( $" {SIGN} ", elements );
			return $"( {Value} => ( {expression} ) )";
		}
	}
}