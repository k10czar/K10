using UnityEngine;

namespace NumberStateOperations
{
	public abstract class NumberBinaryComparerState : BoolState
	{
		object _refA, _refB;
		protected readonly ConditionalEventsCollection _events = new ConditionalEventsCollection();
		~NumberBinaryComparerState() { _events.Void(); }
		public NumberBinaryComparerState( object refA, object refB ) : base() { _refA = refA; _refB = refB; }
		protected abstract string SIGN { get; }
		public override string ToString() => $"( {Value} => ( {_refA.ToStringOrNull()} {SIGN} {_refB.ToStringOrNull()} ) )";
	}

	public class IsZero : Equals
	{
		public IsZero( IValueStateObserver<int> variable ) : base( variable, 0 ) { }
		public IsZero( IValueStateObserver<float> variable ) : base( variable, 0 ) { }
	}

	public class NotZero : Diff
	{
		public NotZero( IValueStateObserver<int> variable ) : base( variable, 0 ) { }
		public NotZero( IValueStateObserver<float> variable ) : base( variable, 0 ) { }
	}

	public class LessThanZero : Less
	{
		public LessThanZero( IValueStateObserver<int> variable ) : base( variable, 0 ) { }
		public LessThanZero( IValueStateObserver<float> variable ) : base( variable, 0 ) { }
	}

	public class GreaterThanZero : Greater
	{
		public GreaterThanZero( IValueStateObserver<int> variable ) : base( variable, 0 ) { }
		public GreaterThanZero( IValueStateObserver<float> variable ) : base( variable, 0 ) { }
	}

	public class Less : NumberBinaryComparerState
	{
		protected override string SIGN => "<";

		public Less( IValueStateObserver<int> variable, int constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( v < constant ), _events );
		}

		public Less( IValueStateObserver<float> variable, int constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( v < constant ), _events );
		}

		public Less( IValueStateObserver<float> variable, float constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( v < constant ), _events );
		}

		public Less( IValueStateObserver<int> variableA, IValueStateObserver<int> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( v < variableB.Value ), _events );
			variableB.Synchronize( ( v ) => Setter( v > variableA.Value ), _events );
		}

		public Less( IValueStateObserver<int> variableA, IValueStateObserver<float> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( v < variableB.Value ), _events );
			variableB.Synchronize( ( v ) => Setter( v > variableA.Value ), _events );
		}

		public Less( IValueStateObserver<float> variableA, IValueStateObserver<int> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( v < variableB.Value ), _events );
			variableB.Synchronize( ( v ) => Setter( v > variableA.Value ), _events );
		}

		public Less( IValueStateObserver<float> variableA, IValueStateObserver<float> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( v < variableB.Value ), _events );
			variableB.Synchronize( ( v ) => Setter( v > variableA.Value ), _events );
		}
	}

	public class Greater : NumberBinaryComparerState
	{
		protected override string SIGN => ">";

		public Greater( IValueStateObserver<int> variable, int constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( v > constant ), _events );
		}

		public Greater( IValueStateObserver<float> variable, int constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( v > constant ), _events );
		}

		public Greater( IValueStateObserver<float> variable, float constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( v > constant ), _events );
		}

		public Greater( IValueStateObserver<int> variableA, IValueStateObserver<int> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( v > variableB.Value ), _events );
			variableB.Synchronize( ( v ) => Setter( v < variableA.Value ), _events );
		}

		public Greater( IValueStateObserver<int> variableA, IValueStateObserver<float> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( v > variableB.Value ), _events );
			variableB.Synchronize( ( v ) => Setter( v < variableA.Value ), _events );
		}

		public Greater( IValueStateObserver<float> variableA, IValueStateObserver<int> variableB ) : base( variableA, variableB )
		{
			variableB.Synchronize( ( v ) => Setter( v < variableA.Value ), _events );
			variableA.Synchronize( ( v ) => Setter( v > variableB.Value ), _events );
		}

		public Greater( IValueStateObserver<float> variableA, IValueStateObserver<float> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( v > variableB.Value ), _events );
			variableB.Synchronize( ( v ) => Setter( v < variableA.Value ), _events );
		}
	}

	public class Equals : NumberBinaryComparerState
	{
		protected override string SIGN => "==";

		public Equals( IValueStateObserver<int> variable, int constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( v == constant ), _events );
		}

		public Equals( IValueStateObserver<float> variable, int constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( Mathf.Approximately( v, constant ) ), _events );
		}

		public Equals( IValueStateObserver<float> variable, float constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( Mathf.Approximately( v, constant ) ), _events );
		}

		public Equals( IValueStateObserver<int> variableA, IValueStateObserver<int> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( v == variableB.Value ), _events );
			variableB.Synchronize( ( v ) => Setter( v == variableA.Value ), _events );
		}

		public Equals( IValueStateObserver<int> variableA, IValueStateObserver<float> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( Mathf.Approximately( v, variableB.Value ) ), _events );
			variableB.Synchronize( ( v ) => Setter( Mathf.Approximately( v, variableA.Value ) ), _events );
		}

		public Equals( IValueStateObserver<float> variableA, IValueStateObserver<float> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( Mathf.Approximately( v, variableB.Value ) ), _events );
			variableB.Synchronize( ( v ) => Setter( Mathf.Approximately( v, variableA.Value ) ), _events );
		}
	}

	public class Diff : NumberBinaryComparerState
	{
		protected override string SIGN => "!=";

		public Diff( IValueStateObserver<int> variable, int constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( v != constant ), _events );
		}

		public Diff( IValueStateObserver<float> variable, int constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( !Mathf.Approximately( v, constant ) ), _events );
		}

		public Diff( IValueStateObserver<float> variable, float constant ) : base( variable, constant )
		{
			variable.Synchronize( ( v ) => Setter( !Mathf.Approximately( v, constant ) ), _events );
		}

		public Diff( IValueStateObserver<int> variableA, IValueStateObserver<int> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( v != variableB.Value ), _events );
			variableB.Synchronize( ( v ) => Setter( v != variableA.Value ), _events );
		}

		public Diff( IValueStateObserver<int> variableA, IValueStateObserver<float> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( !Mathf.Approximately( v, variableB.Value ) ), _events );
			variableB.Synchronize( ( v ) => Setter( !Mathf.Approximately( v, variableA.Value ) ), _events );
		}

		public Diff( IValueStateObserver<float> variableA, IValueStateObserver<float> variableB ) : base( variableA, variableB )
		{
			variableA.Synchronize( ( v ) => Setter( !Mathf.Approximately( v, variableB.Value ) ), _events );
			variableB.Synchronize( ( v ) => Setter( !Mathf.Approximately( v, variableA.Value ) ), _events );
		}
	}
}