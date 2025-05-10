using System.Linq;
using UnityEngine;

namespace K10.Conditions
{
	public class And : ICondition
	{
		[ExtendedDrawer, SerializeReference] ICondition[] _operands;
		public bool Check()
		{
			for( int i = 0; i < _operands.Length; i++ )
			{
				var cond = _operands[i];
				if( cond == null ) continue;
				if( !cond.Check() ) return false;
			}
			return true;
		}
		override public string ToString() 
		{
			return $"({string.Join( " && ", _operands.Select( ( o ) => o.ToStringOrNull() ).ToArray() )})";
		}
	}
}
