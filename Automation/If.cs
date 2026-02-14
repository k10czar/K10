using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace K10.Automation
{
	[ListingPath(nameof(If))]
	public class If : BaseOperation
						, IDrawGizmos, IDrawGizmosOnSelected, ISummarizable
	{
		[ExtendedDrawer,SerializeReference] ICondition _condition;
		[ExtendedDrawer,SerializeReference] List<IOperation> _actions;

		public override string EmojiIcon => "🧐";

		public override IEnumerator ExecutionCoroutine( bool log = false ) 
		{
			var condition = _condition.SafeCheck();
			if( log ) Debug.Log( $"If {(condition?"succedded":"fail")} {_condition.ToStringOrNull()}" );
			if( condition )
			{
				for( int i = 0; i < _actions.Count; i++ ) 
				{
					var act = _actions[i];
					if( log ) Debug.Log( $"[{i}]{act.ToStringOrNull()}" );
					if( act == null ) continue;
					yield return act.TryExecute( log );
				}
			}
		}

		public override string ToString() => $"{base.ToString()} {_condition.ToStringOrNull()} {_actions.ToStringOrNull()}";
		public string Summarize() => $" If {{ {_actions.TrySummarize( ", " )} }}";

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