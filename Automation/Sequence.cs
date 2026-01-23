using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using K10.Automation;

namespace K10.Automation
{
	[ListingPath(nameof(Sequence))]
	public class Sequence : BaseOperation
						, IDrawGizmos, IDrawGizmosOnSelected, ISummarizable
	{
		[ExtendedDrawer,SerializeReference] List<IOperation> _actions;

		public override string EmojiIcon => "🗒";

		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{
			if( log ) Debug.Log( $"Start Coroutine of {this.ToStringOrNull()}" );
			for( int i = 0; i < _actions.Count; i++ )
			{
				var act = _actions[i];
				if( act == null ) 
				{
					if( log ) Debug.LogError( $"{"Cannot".Colorfy( Colors.Console.Warning )} {"play".Colorfy( Colors.Console.Verbs )} null {"Operation".Colorfy( Colors.Console.TypeName )}" );
					continue;
				}
				if( log ) Debug.Log( $"{( act.CanExecute ? "Start" : "Skipped" ).Colorfy( act.CanExecute ? Colors.Console.Verbs : Colors.Console.LightDanger )} operation: {act.ToStringOrNull()}" );
				yield return act.TryExecute( log );
			}
		}

		public override string ToString() => $"{base.ToString()} {_actions.ToStringOrNull()}";
		public string Summarize() => $" Sequence {{ {_actions.TrySummarize( ", " )} }}";

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