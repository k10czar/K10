using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using K10.Automation;

namespace K10.Automation
{
	[ListingPath(nameof(Loop))]
	public class Loop : BaseOperation
						, IDrawGizmos, IDrawGizmosOnSelected, ISummarizable
	{
#if UNITY_2023_1_OR_NEWER
		[ExtendedDrawer, SerializeReference] IValueProvider<int> _repetitionsRef;
#else
		[SerializeField] int _repetitionsVal = 1;
#endif
		[ExtendedDrawer,SerializeReference] List<IOperation> _actions;
		
		public override string EmojiIcon => "🔄";

		public int Repetitions =>	
#if UNITY_2023_1_OR_NEWER
			_repetitionsRef?.Value ?? 1;
#else
			_repetitionsVal;
#endif

		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{
			var repetitions = Repetitions;
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
					yield return act.TryExecute( log );
				}
			}
		}

		public override string ToString()
		{
#if UNITY_2023_1_OR_NEWER
			return $"{base.ToString()} {_repetitionsRef.ToStringOrNull()}x: {_actions.ToStringOrNull()}";
#else
			return $"{base.ToString()} {_repetitionsVal.ToStringOrNull()}x: {_actions.ToStringOrNull()}";
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
	}
}