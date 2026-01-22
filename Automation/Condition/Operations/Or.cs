using System.Linq;
using UnityEngine;

namespace K10.Conditions
{
	[ListingPath(nameof(Or))]
	public class Or : ICondition
	{
		[ExtendedDrawer, SerializeReference] ICondition[] _operands;

		public bool Check()
		{
			if( _operands == null ) return true;
			for( int i = 0; i < _operands.Length; i++ )
			{
				var cond = _operands[i];
				if( cond == null ) continue;
				if( cond.Check() ) return true;
			}
			return false;
		}
		
		override public string ToString() 
		{
			if( _operands == null ) return "NULL_OR";
			return $"( {string.Join( " || ", _operands.Select( ( o ) => o.ToStringOrNull() ).ToArray() )} )";
		}
	}
}
