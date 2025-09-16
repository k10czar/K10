using System.Collections;
using UnityEngine;
using Automation;
using System.Collections.Generic;

namespace Automation
{
    public class Loop : IOperation , IDrawGizmos, IDrawGizmosOnSelected, ISummarizable
	{
#if UNITY_2023_1_OR_NEWER
		[ExtendedDrawer, SerializeReference] IValueProvider<int> _repetitionsRef;
#else
		[SerializeField] int _repetitionsVal = 1;
#endif
		[ExtendedDrawer,SerializeReference] List<IOperation> _actions;

		public IEnumerator ExecutionCoroutine( bool log = false )
		{
#if UNITY_2023_1_OR_NEWER
			var repetitions = _repetitionsRef?.Value ?? 1;
#else
			var repetitions = _repetitionsVal;
#endif
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
					if( log ) Debug.Log( $"{"Start".Colorfy( Colors.Console.Verbs )} operation[{i}]: {act.ToStringOrNull()}" );
					yield return act.ExecutionCoroutine( log );
				}
			}
		}

        public override string ToString()
        {
#if UNITY_2023_1_OR_NEWER
            return $"🗃 {"Loop".Colorfy(Colors.Console.Fields)} {_repetitionsRef.ToStringOrNull()}x: {{\n  -{string.Join(",\n  -", _actions)} }}";
#else
            return $"🗃 {"Loop".Colorfy(Colors.Console.Fields)} {_repetitionsVal.ToStringOrNull()}x: {{\n  -{string.Join(",\n  -", _actions)} }}";
#endif
        }

        public string Summarize()
        {
#if UNITY_2023_1_OR_NEWER
            return $"🗃Loop{_repetitionsRef.TrySummarize()}x{{ {_actions.TrySummarize(", ")} }}";
#else
            return $"🗃Loop{_repetitionsVal.TrySummarize()}x{{ {_actions.TrySummarize(", ")} }}";
#endif
        }


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

		public Object[] LogOwners { get; } = { null };
	}
}