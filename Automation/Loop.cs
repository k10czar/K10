using System.Collections;
using UnityEngine;
using Automation;
using System.Collections.Generic;

namespace Automation
{
    public class Loop : IOperation
						, IDrawGizmos, IDrawGizmosOnSelected
	{
		[SerializeField] int _repetitions = 1;
		[ExtendedDrawer,SerializeReference] List<IOperation> _actions;

		public IEnumerator ExecutionCoroutine( bool log = false ) 
		{
			for( int l = 0; l < _repetitions; l++ )
			{
				if( log ) Debug.Log( $"♻ {"Loop".Colorfy( Colors.Console.Fields )}[{l.ToStringColored(Colors.Console.Numbers)}] in {this.ToStringOrNull()}" );
				for( int i = 0; i < _actions.Count; i++ )
				{
					var act = _actions[i];
					if( act == null ) 
					{
						if( log ) Debug.LogError( $"{"Cannot".Colorfy( Colors.Console.Warning )} {"play".Colorfy( Colors.Console.Verbs )} null {"Operation".Colorfy( Colors.Console.TypeName )}" );
						continue;
					}
					if( log ) Debug.Log( $"{"Start".Colorfy( Colors.Console.Verbs )} operation[{i}]: {act.ToStringOrNull()}" );
					yield return act.ExecutionCoroutine( log );
				}
			}
		}

		public override string ToString() => $"🗃 {"Loop".Colorfy( Colors.Console.Fields )} {_repetitions.ToStringColored(Colors.Console.Numbers)}x: {{\n  -{string.Join( ",\n  -", _actions )} }}";

#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			foreach( var act in _actions ) if( act is IDrawGizmos dg ) dg.OnDrawGizmos();
		}

		public void OnDrawGizmosSelected()
		{
			foreach( var act in _actions ) if( act is IDrawGizmosOnSelected dgs ) dgs.OnDrawGizmosSelected();
		}
#endif
	}
}