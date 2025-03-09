using System.Collections;
using UnityEngine;
using Automation;
using System.Collections.Generic;

namespace Automation
{
    public class Loop : IOperation
						, IDrawGizmos, IDrawGizmosOnSelected, ISummarizable
	{
		[ExtendedDrawer,SerializeReference] IValueProvider<int> _repetitions;
		[ExtendedDrawer,SerializeReference] List<IOperation> _actions;

		public IEnumerator ExecutionCoroutine( bool log = false ) 
		{
			var repetitions = _repetitions?.Value ?? 1;
			for( int l = 0; l < repetitions; l++ )
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
					if( log ) Debug.Log( $"{"Start".Colorfy( Colors.Console.Verbs )} operation: {act.ToStringOrNull()}" );
					yield return act.ExecutionCoroutine( log );
				}
			}
		}

		public override string ToString() => $"🗃 {"Loop".Colorfy( Colors.Console.Fields )} {_repetitions.ToStringOrNull()}x: {{\n  -{string.Join( ",\n  -", _actions )} }}";
		public string Summarize() => $"🗃Loop{_repetitions.TrySummarize()}x{{ {_actions.TrySummarize( ", " )} }}";



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